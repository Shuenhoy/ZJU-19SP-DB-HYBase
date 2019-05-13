using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;

namespace HYBase.BufferManager
{
    struct HashKey
    {
        FileStream file;
        int pageNum;
        public override int GetHashCode()
            => HashCode.Combine(file, pageNum);
    }

    public class Manager
    {
        static int PAGE_SIZE = 4096 - Marshal.SizeOf<PageHeader>();
        public Manager(int _numPages)
        {
            pageSize = PAGE_SIZE + Marshal.SizeOf<PageHeader>();
            numPages = _numPages;
            hashTabel = new Dictionary<HashKey, int>();
        }

        public void CreateFile(String fileName)
        {
            throw new NotImplementedException();
        }


        public void DestroyFile(String fileName)
        {
            throw new NotImplementedException();
        }

        public FileHandle OpenFile(String fileName)
        {
            throw new NotImplementedException();
        }
        public void CloseFIle(FileHandle file)
        {
            throw new NotImplementedException();
        }

        public Memory<byte> AllocateBlock()
        {
            throw new NotImplementedException();
        }
        public void DisposeBlock(Memory<byte> block)
        {
            throw new NotImplementedException();
        }
        int numPages;
        int pageSize;
        Dictionary<HashKey, int> hashTabel;
    }
}