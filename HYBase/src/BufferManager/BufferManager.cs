using System.Collections.Generic;
using System.IO;
using HYBase.Utils;
using LanguageExt;
using static LanguageExt.Prelude;
using static HYBase.Utils.Utils;
using System;
using System.Diagnostics;
using System.Linq;

namespace HYBase.BufferManager
{
    class Key
    {
        public Stream file;
        public int pageNum;
        public override int GetHashCode()
            => HashCode.Combine(file.GetHashCode(), pageNum.GetHashCode());
        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;
            if (!(obj is Key))
                return false;
            var other = (Key)obj;
            return file == other.file && pageNum == other.pageNum;
        }
        public static implicit operator Key((Stream file, int pageNum) tuple)
        {
            var ret = new Key();
            ret.file = tuple.Item1;
            ret.pageNum = tuple.Item2;
            return ret;
        }


    }
    class BufferManager
    {

        private LinkedList<(Key key, BufferBlock value)> used;
        private LinkedList<BufferBlock> free;
        private IDictionary<Key, LinkedListNode<(Key key, BufferBlock page)>> hashTable;
        public BufferManager(int cap)
        {
            free = new LinkedList<BufferBlock>();
            used = new LinkedList<(Key key, BufferBlock value)>();
            hashTable = new Dictionary<Key, LinkedListNode<(Key key, BufferBlock page)>>();
            for (int i = 0; i < cap; i++)
            {
                var n = new BufferBlock();
                free.AddFirst(n);
            }
        }
        public PageData GetPage(Stream file, int pageNum)
            => hashTable.TryGetValue((file, pageNum)).BiBind<PageData>(
                    Some: page => page.Value.page.page
                , None: () =>
                {
                    var node = InternalAlloc();
                    ReadPage(file, pageNum, ref node.Value.value.page);
                    hashTable.Add((file, pageNum), node);
                    node.Value.key.file = file;
                    node.Value.key.pageNum = pageNum;

                    node.Value.value.PinCount = 1;
                    node.Value.value.Dirty = false;
                    return node.Value.value.page;

                }).First();

        public void SetPage(Stream file, int pageNum, PageData newPage)
        {
            hashTable.TryGetValue((file, pageNum)).Match(Some: page =>
                {
                    page.Value.page.page = newPage;
                    page.Value.page.Dirty = true;
                    page.Value.page.PinCount++;
                }
                , None: () =>
                {
                    var node = InternalAlloc();
                    hashTable.Add((file, pageNum), node);

                    node.Value.value.page = newPage;
                    node.Value.key.file = file;
                    node.Value.key.pageNum = pageNum;

                    node.Value.value.PinCount = 1;
                    node.Value.value.Dirty = true;


                });
        }
        public void SetPageData(Stream file, int pageNum, byte[] newPageData)
        {
            hashTable.TryGetValue((file, pageNum)).Match(Some: page =>
                {
                    page.Value.page.page.data = newPageData;
                    page.Value.page.Dirty = true;
                }
                , None: () =>
                {
                    var node = InternalAlloc();
                    hashTable.Add((file, pageNum), node);

                    node.Value.value.page.data = newPageData;
                    node.Value.key.file = file;
                    node.Value.key.pageNum = pageNum;

                    node.Value.value.PinCount = 1;
                    node.Value.value.Dirty = true;


                });
        }
        public void MarkDirty(Stream file, int pageNum)
        {
            hashTable.TryGetValue<Key, LinkedListNode<(Key key, BufferBlock value)>>((file, pageNum)).IfSome(node =>
           {
               if (node.Value.value.PinCount > 0)
               {
                   node.Value.value.Dirty = true;
                   used.Remove(node);
                   used.AddFirst(node);
               }
           });
        }
        public void ForcePage(Stream file, int pageNum)
        {
            hashTable.TryGetValue<Key, LinkedListNode<(Key key, BufferBlock value)>>((file, pageNum)).IfSome(node =>
           {
               node.Value.value.Dirty = false;
               WritePage(file, pageNum, node.Value.value.page);
           });
        }
        public void ForcePages(Stream file)
        {
            foreach (var page in used)
            {
                if (page.key.file == file)
                {
                    if (page.value.Dirty)
                    {
                        WritePage(file, page.key.pageNum, page.value.page);
                    }
                }

            }
        }
        public void UnPin(Stream file, int pageNum)
        {
            hashTable.TryGetValue<Key, LinkedListNode<(Key key, BufferBlock value)>>((file, pageNum)).IfSome(node =>
           {
               node.Value.value.PinCount--;
               if (node.Value.value.PinCount == 0)
               {
                   node.Value.value.Dirty = true;
                   used.Remove(node);
                   used.AddFirst(node);
               }
           });
        }
        public void FlushPages(Stream file)
        {
            foreach (var page in used)
            {
                if (page.key.file == file)
                {
                    if (page.value.PinCount == 0)
                    {
                        if (page.value.Dirty)
                        {
                            WritePage(file, page.key.pageNum, page.value.page);
                            Remove(page.key);
                        }
                    }
                }

            }
        }
        public void Remove(LinkedListNode<(Key key, BufferBlock value)> ele)
        {
            free.AddFirst(ele.Value.value);
            hashTable.Remove(ele.Value.key);
            used.Remove(ele);
        }
        public void Remove(Key key)
        {
            hashTable.TryGetValue<Key, LinkedListNode<(Key key, BufferBlock value)>>(key).IfSome(node =>
            {
                free.AddFirst(node.Value.value);
                used.Remove(node);
                hashTable.Remove(key);
            });
        }
        private LinkedListNode<(Key key, BufferBlock value)> InternalAlloc()
        {
            if (free.Count == 0)
            {
                var k = used.Last;
                for (; k != null; k = k.Previous)
                {
                    if (k.Value.value.PinCount == 0)
                    {
                        break;
                    }
                }
                Debug.Assert(k != null, "all buffer block is pinned!");
                if (k.Value.value.Dirty)
                {
                    WritePage(k.Value.key.file, k.Value.key.pageNum, k.Value.value.page);
                    k.Value.value.Dirty = false;
                }
                hashTable.Remove(k.Value.key);
                used.Remove(k);
                used.AddFirst(k);
                return k;
            }
            else
            {
                var f = free.First;
                free.RemoveFirst();
                return used.AddFirst((new Key(), f.Value));
            }
        }
        private void ReadPage(Stream file, int pageNum, ref PageData data)
        {
            int offset = pageNum * BufferBlock.SIZE + BufferBlock.SIZE;
            file.Seek(offset, SeekOrigin.Begin);
            var bytes = new byte[4096];
            file.Read(bytes, 0, BufferBlock.SIZE - sizeof(int));
            data = ByteArrayToStructure<PageData>(bytes);
        }
        public void WritePage(Stream file, int pageNum, PageData data)
        {
            int offset = pageNum * BufferBlock.SIZE + BufferBlock.SIZE;
            file.Seek(offset, SeekOrigin.Begin);
            file.Write(StructureToByteArray(data), 0, BufferBlock.SIZE);
        }

    }
}