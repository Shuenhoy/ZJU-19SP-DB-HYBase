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
    struct Key
    {
        public Stream file;
        public int pageNum;
        public override int GetHashCode()
            => HashCode.Combine(file.GetHashCode(), pageNum.GetHashCode());
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

        private LinkedList<(Key key, Page value)> used;
        private LinkedList<Page> free;
        private IDictionary<Key, LinkedListNode<(Key key, Page page)>> hashTable;
        public BufferManager(int cap)
        {
            for (int i = 0; i < cap; i++)
            {
                var n = new Page();
                free.AddFirst(n);
            }
        }
        public PageData GetPage(Stream file, int pageNum)
            => hashTable.TryGetValue((file, pageNum)).BiBind<PageData>(Some: page => page.Value.page.page
                , None: () =>
                {
                    var node = InternalAlloc();
                    ReadPage(file, pageNum, ref node.Value.value.page);
                    node.Value.value.PinCount = 1;
                    node.Value.value.Dirty = false;
                    return node.Value.value.page;

                }).First();

        public void MarkDirty(Stream file, int pageNum)
        {
            hashTable.TryGetValue<Key, LinkedListNode<(Key key, Page value)>>((file, pageNum)).IfSome(node =>
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
            hashTable.TryGetValue<Key, LinkedListNode<(Key key, Page value)>>((file, pageNum)).IfSome(node =>
           {
               node.Value.value.Dirty = false;
               WritePage(file, pageNum, node.Value.value.page);
           });
        }
        public void UnPin(Stream file, int pageNum)
        {
            hashTable.TryGetValue<Key, LinkedListNode<(Key key, Page value)>>((file, pageNum)).IfSome(node =>
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
        public void Remove(LinkedListNode<(Key key, Page value)> ele)
        {
            free.AddFirst(ele.Value.value);
            hashTable.Remove(ele.Value.key);
            used.Remove(ele);
        }
        public void Remove(Key key)
        {
            hashTable.TryGetValue<Key, LinkedListNode<(Key key, Page value)>>(key).IfSome(node =>
            {
                free.AddFirst(node.Value.value);
                used.Remove(node);
                hashTable.Remove(key);
            });
        }
        private LinkedListNode<(Key key, Page value)> InternalAlloc()
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
                return used.AddFirst((new Key(), f.Value));
            }
        }
        private void ReadPage(Stream file, int pageNum, ref PageData data)
        {
            int offset = pageNum * Page.SIZE + Page.SIZE;
            file.Seek(offset, SeekOrigin.Begin);
            var bytes = new byte[4096];
            file.Read(bytes, 0, Page.SIZE - sizeof(int));
            data = ByteArrayToStructure<PageData>(bytes);
        }
        public void WritePage(Stream file, int pageNum, PageData data)
        {
            int offset = pageNum * Page.SIZE + Page.SIZE;
            file.Seek(offset, SeekOrigin.Begin);
            file.Write(StructureToByteArray(data), 0, Page.SIZE);
        }

    }
}