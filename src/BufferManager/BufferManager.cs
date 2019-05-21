using System.Collections.Generic;
using System.IO;
using HYBase.Utils;
using LanguageExt;
using static LanguageExt.Prelude;
using System;
using System.Diagnostics;
using System.Linq;

namespace HYBase.BufferManager
{
    struct Key
    {
        public FileStream file;
        public int pageNum;
        public override int GetHashCode()
            => HashCode.Combine(file.GetHashCode(), pageNum.GetHashCode());
        public static implicit operator Key((FileStream file, int pageNum) tuple)
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
        public PageData GetPage(FileStream file, int pageNum)
            => hashTable.TryGetValue((file, pageNum)).BiBind<PageData>(Some: page => page.Value.page.page
                , None: () =>
                {
                    var node = InternalAlloc();
                    ReadPage(file, pageNum, node.Value.value.page);
                    node.Value.value.PinCount = 1;
                    node.Value.value.Dirty = false;
                    return node.Value.value.page;

                }).First();

        public void MarkDirty(FileStream file, int pageNum)
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
        public void ForcePage(FileStream file, int pageNum)
        {
            hashTable.TryGetValue<Key, LinkedListNode<(Key key, Page value)>>((file, pageNum)).IfSome(node =>
           {
               node.Value.value.Dirty = false;
               WritePage(file, pageNum, node.Value.value.data);
           });
        }
        public void UnPin(FileStream file, int pageNum)
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
                    WritePage(k.Value.key.file, k.Value.key.pageNum, k.Value.value.data);
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
        private void ReadPage(FileStream file, int pageNum, ref PageData data)
        {
            int offset = pageNum * Page.SIZE + Page.SIZE;
            file.Seek(offset, SeekOrigin.Begin);

            file.Read(data.data, 0, Page.SIZE - sizeof(int));
        }
        private void WritePage(FileStream file, int pageNum, byte[] data)
        {
            int offset = pageNum * Page.SIZE + Page.SIZE;
            file.Seek(offset, SeekOrigin.Begin);
            file.Write(data, 0, Page.SIZE);
        }
    }
}