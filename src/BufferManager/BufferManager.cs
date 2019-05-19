using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static HYBase.Utils.Utils;
using LanguageExt;
using static LanguageExt.Prelude;

namespace HYBase.BufferManager
{

    struct BufferPageDesc
    {
        public Memory<byte> data;      // page contents
        public int next;        // next in the linked list of buffer pages
        public int prev;        // prev in the linked list of buffer pages
        public bool dirty;      // TRUE if page is dirty
        public int pinCount;    // pin count
        public int pageNum;     // page number for this page
        public FileStream file;          // OS file descriptor of this page
    }
    class BufferManager
    {
        static int PAGE_SIZE = 4096 - Marshal.SizeOf<PageHeader>();
        BufferPageDesc[] bufferTable;
        int first;                         // MRU page slot
        int last;                          // LRU page slot
        int free;                          // head of free list  
        int numPages;
        int pageSize;
        IDictionary<(FileStream, int), int> hashTable;
        void InitBuffer()
        {
            bufferTable =
                           Range(0, numPages - 1).Select(index => new BufferPageDesc
                           {
                               data = new Memory<byte>(new byte[pageSize]),
                               prev = index - 1,
                               next = index + 1
                           }
                           ).ToArray();
            bufferTable[0].prev = bufferTable[numPages - 1].next = -1;
        }
        public BufferManager(int _numPages)
        {
            pageSize = PAGE_SIZE + Marshal.SizeOf<PageHeader>();
            numPages = _numPages;
            hashTable = new Dictionary<(FileStream, int), int>();
            InitBuffer();
            first = last = -1;
            free = 0;

        }

        // Read pageNum into buffer, point *ppBuffer to location
        Either<ErrorCode, Memory<byte>> GetPage(FileStream file, int pageNum)
            => hashTable.TryGetValue((file, pageNum)).BiBind<Either<ErrorCode, Memory<byte>>>(
                Some: slot =>
                {
                    bufferTable[slot].pinCount++;
                    return
                        from x in Unlink(slot)
                        from y in LinkHead(slot)
                        select bufferTable[slot].data;
                },
                None: () => from slot in InternalAlloc()
                            from x in ReadPage(file, pageNum)
                            let _ = hashTable.TryAdd((file, pageNum), slot)
                            from _1 in InitPageDesc(file, pageNum, slot)
                            let _2 = Unlink(slot)
                            let _3 = InsertFree(slot)
                            select bufferTable[slot].data).First();


        Either<ErrorCode, Memory<byte>> AllocatePage(FileStream file, int pageNum)
            => hashTable.TryGetValue((file, pageNum)).BiBind<Either<ErrorCode, Memory<byte>>>(
                    Some: _ => Left<ErrorCode, Memory<byte>>(ErrorCode.PAGEINBUF),
                    None: () => from slot in InternalAlloc()
                                let _ = hashTable.TryAdd((file, pageNum), slot)
                                from _1 in InitPageDesc(file, pageNum, slot)
                                select bufferTable[slot].data
                ).First();


        Either<ErrorCode, Unit> MarkDirty(FileStream file, int pageNum)
            => hashTable.TryGetValue((file, pageNum)).BiBind<Either<ErrorCode, Unit>>(
                Some: slot =>
                {
                    if (bufferTable[slot].pinCount == 0)
                        return Left<ErrorCode, Unit>(ErrorCode.PAGEUNPINNED);
                    bufferTable[slot].dirty = true;
                    return
                        from x in Unlink(slot)
                        from y in LinkHead(slot)
                        select unit;
                },
                None: () => Left<ErrorCode, Unit>(ErrorCode.PAGENOTINBUF)
            ).First();


        Either<ErrorCode, Unit> UnpinPage(FileStream file, int pageNum)
            => hashTable.TryGetValue((file, pageNum)).BiBind<Either<ErrorCode, Unit>>(
                Some: slot =>
                    ((--bufferTable[slot].pinCount) == 0)
                    ? (from x in Unlink(slot)
                       from y in LinkHead(slot)
                       select unit)
                    : Right<ErrorCode, Unit>(unit)
                ,
                None: () => Left<ErrorCode, Unit>(ErrorCode.PAGENOTINBUF)
            ).First();

        Either<ErrorCode, Unit> FlushPages(FileStream file)
        {
            int slot = first;
            while (slot != -1)
            {
                int next = bufferTable[slot].next;
                if (bufferTable[slot].file == file)
                {
                    if (bufferTable[slot].pinCount == 0)
                    {
                        var rc = WritePage(file, bufferTable[slot].pageNum, bufferTable[slot].data.Span);
                        if (rc.IsLeft) return rc;
                        bufferTable[slot].dirty = false;
                        if (hashTable.Remove((file, bufferTable[slot].pageNum)))
                        {
                            rc = from x in Unlink(slot)
                                 from y in InsertFree(slot)
                                 select x;
                            if (rc.IsLeft) return rc;
                        }
                    }
                }
                slot = next;
            }
            return unit;
        }                 // Flush pages for file

        // Force a page to the disk, but do not remove from the buffer pool
        Either<ErrorCode, Unit> ForcePages(FileStream file, int pageNum)
        {
            int slot = first;
            while (slot != -1)
            {
                int next = bufferTable[slot].next;
                if (bufferTable[slot].file == file && bufferTable[slot].pageNum == pageNum)
                {
                    if (bufferTable[slot].dirty)
                    {
                        var rc = WritePage(file, bufferTable[slot].pageNum, bufferTable[slot].data.Span);
                        if (rc.IsLeft) return rc;
                        bufferTable[slot].dirty = false;
                    }
                }
                slot = next;
            }
            return unit;
        }


        // Remove all entries from the Buffer Manager.
        Either<ErrorCode, Unit> ClearBuffer()
        {
            int slot = first;
            while (slot != -1)
            {
                int next = bufferTable[slot].next;

                if (bufferTable[slot].pinCount == 0)
                {

                    bufferTable[slot].dirty = false;
                    if (hashTable.Remove((bufferTable[slot].file, bufferTable[slot].pageNum)))
                    {
                        var rc = from x in Unlink(slot)
                                 from y in InsertFree(slot)
                                 select x;
                        if (rc.IsLeft) return rc;
                    }
                }

                slot = next;
            }
            return unit;
        }


        // Attempts to resize the buffer to the new size
        Either<ErrorCode, Unit> ResizeBuffer(int newSize)
        {
            ClearBuffer();
            var oldBuffer = bufferTable;
            InitBuffer();
            int oldFirst = first;
            numPages = newSize;
            first = last = -1;
            free = 0;
            int slot = oldFirst, next;
            while (slot != -1)
            {
                next = oldBuffer[slot].next;
                hashTable.Remove((oldBuffer[slot].file, oldBuffer[slot].pageNum));
                slot = next;
            }
            slot = oldFirst;
            while (slot != -1)
            {
                next = oldBuffer[slot].next;
                var rc =
                    InternalAlloc().Bind<Unit>(newSlot =>
                    {
                        if (hashTable.TryAdd((oldBuffer[slot].file, oldBuffer[slot].pageNum), newSlot))
                            return Left<ErrorCode, Unit>(ErrorCode.INVALIDPAGE);
                        var rc1 = InitPageDesc(oldBuffer[slot].file, oldBuffer[slot].pageNum, newSlot);
                        if (rc1.IsLeft) return rc1;
                        Unlink(newSlot);
                        InsertFree(newSlot);
                        return Right<ErrorCode, Unit>(unit);

                    });
                if (rc.IsLeft) return rc;
                slot = next;
            }
            return unit;
        }

        // Three Methods for manipulating raw memory buffers.  These memory
        // locations are handled by the buffer manager, but are not
        // associated with a particular file.  These should be used if you
        // want memory that is bounded by the size of the buffer pool.

        // Return the size of the block that can be allocated.
        Either<ErrorCode, int> GetBlockSize()
        {
            return pageSize;
        }

        // Allocate a memory chunk that lives in buffer manager
        Memory<byte> AllocateBlock()
        {
            int slot;

        }
        // Dispose of a memory chunk managed by the buffer manager.
        void DisposeBlock(Memory<byte> buffer);

        private
        Either<ErrorCode, Unit> InsertFree(int slot);                 // Insert slot at head of free
        Either<ErrorCode, Unit> LinkHead(int slot);                 // Insert slot at head of used
        Either<ErrorCode, Unit> Unlink(int slot);                 // Unlink slot
        Either<ErrorCode, int> InternalAlloc();                // Get a slot to use

        // Read a page 
        Either<ErrorCode, Memory<byte>> ReadPage(FileStream file, int pageNum);

        // Write a page
        Either<ErrorCode, Unit> WritePage(FileStream file, int pageNum, Span<byte> source);

        // Init the page desc entry
        Either<ErrorCode, Unit> InitPageDesc(FileStream file, int pageNum, int slot);



    }
}