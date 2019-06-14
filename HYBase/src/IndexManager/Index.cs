using System;
using System.IO;
using System.Diagnostics;
using HYBase.RecordManager;
using HYBase.BufferManager;
using System.Collections.Generic;
using static HYBase.Utils.Utils;


namespace HYBase.IndexManager
{
    public class Index
    {
        internal FileHeader fileHeader;
        internal PagedFile file;
        public Index(PagedFile f, AttrType attrType, int attrLength)
        {
            fileHeader = new FileHeader
            {
                AttributeType = attrType,
                AttributeLength = attrLength,
                root = 0,
                Height = 0,
            };
            file = f;
            Init();
        }
        public Index(PagedFile f)
        {
            fileHeader = Utils.Utils.ByteArrayToStructure<FileHeader>(f.GetHeader());

            file = f;
        }
        internal void WriteHeader()
        {
            file.SetHeader(Utils.Utils.StructureToByteArray(fileHeader));
        }
        public void Close()
        {
            WriteHeader();
            file.Close();
        }
        public void Init()
        {
            file.DeallocatePages(); //Deallocate all pages

            var rootc = AllocateLeafNode();
            rootc.Father = -1;
            rootc.Prev = -1;
            rootc.Next = -1;
            SetLeafNode(rootc);
            UnPin(rootc);
            fileHeader.root = rootc.pageNum;
        }

        private (int childId, byte[] child1Key, byte[] child2Key) SplitInsert(InternalNode inter, byte[] key, int childId, int insertIndex)
        {
            int os = inter.ChildrenNumber;
            int center = inter.ChildrenNumber / 2;
            int r = inter.ChildrenNumber - center;
            InternalNode sinter = AllocateInternalNode();

            inter.Values.Span.Slice(center * fileHeader.AttributeLength).CopyTo(sinter.Values.Span);
            Array.Copy(inter.Children, center, sinter.Children, 0, r);
            inter.ChildrenNumber = center;
            sinter.ChildrenNumber = r;
            if (insertIndex >= center)
            {
                sinter.Insert(childId, key, insertIndex - center);
            }
            else
            {
                inter.Insert(childId, key, insertIndex);
            }

            Debug.Assert(inter.ChildrenNumber <= inter.Children.Length);
            Debug.Assert(sinter.ChildrenNumber <= sinter.Children.Length);
            Debug.Assert(inter.ChildrenNumber + sinter.ChildrenNumber - 1 == os);
            SetInternalNode(inter);
            SetInternalNode(sinter);
            UnPin(inter);
            UnPin(sinter);

            return (sinter.pageNum, inter.Values.Get(inter.ChildrenNumber - 1).ToArray(), sinter.Values.Get(sinter.ChildrenNumber - 1).ToArray());
        }
        private (int childId, byte[] child1Key, byte[] child2Key) SplitInsert(LeafNode leaf, byte[] data, RID rid, int insertIndex)
        {
            int os = leaf.ChildrenNumber;
            int center = leaf.ChildrenNumber / 2;
            int r = leaf.ChildrenNumber - center;
            LeafNode sleaf = AllocateLeafNode();

            leaf.Data.Span.Slice(center * fileHeader.AttributeLength).CopyTo(sleaf.Data.Span);
            Array.Copy(leaf.ridSlot, center, sleaf.ridSlot, 0, r);
            Array.Copy(leaf.ridPage, center, sleaf.ridPage, 0, r);
            leaf.ChildrenNumber = center;
            sleaf.ChildrenNumber = r;

            sleaf.Next = leaf.Next;
            leaf.Next = sleaf.pageNum;

            if (insertIndex >= center)
            {
                sleaf.Insert(rid.SlotID, rid.PageID, data, insertIndex - center);
            }
            else
            {
                leaf.Insert(rid.SlotID, rid.PageID, data, insertIndex);
            }
            Debug.Assert(leaf.ChildrenNumber + sleaf.ChildrenNumber - 1 == os);


            SetLeafNode(leaf);
            SetLeafNode(sleaf);
            UnPin(leaf);
            UnPin(sleaf);
            return (sleaf.pageNum, leaf.Data.Get(leaf.ChildrenNumber - 1).ToArray(), sleaf.Data.Get(sleaf.ChildrenNumber - 1).ToArray());
        }

        private (int childId, byte[] child1Key, byte[] child2Key) insert(byte[] data, RID rid, int id)
        {
            var leaf = GetLeafNode(id);
            for (int i = 0; i <= leaf.ChildrenNumber; i++)
            {
                if (
                    i == leaf.ChildrenNumber || BytesComp.Comp(
                        data.AsSpan(),
                        leaf.Data.Get(i), fileHeader.AttributeType) < 0)
                {
                    if (leaf.ChildrenNumber >= leaf.ridPage.Length)
                    {
                        return SplitInsert(leaf, data, rid, i);
                    }
                    else
                    {
                        leaf.Insert(rid.SlotID, rid.PageID, data, i);
                        Debug.Assert(leaf.ChildrenNumber <= leaf.ridPage.Length);
                        SetLeafNode(leaf);
                        UnPin(leaf);
                        return (-1, null, null);
                    }

                }
            }
            throw new NotSupportedException();
        }

        private (int childId, byte[] child1Key, byte[] child2Key) insert(byte[] data, RID rid, int id, int level)
        {
            var inter = GetInternalNode(id);
            for (int i = 0; i < inter.ChildrenNumber; i++)
            {
                if (
                    i == inter.ChildrenNumber - 1 || BytesComp.Comp(data.AsSpan(), inter.Values.Get(i), fileHeader.AttributeType) <= 0)
                {
                    var (newChild, child1Key, child2Key)
                        = level == fileHeader.Height - 1 ?
                         insert(data, rid, inter.Children[i]) :
                         insert(data, rid, inter.Children[i], level + 1);

                    if (newChild != -1)
                    {
                        inter.Values.Set(i, child1Key);
                        if (inter.ChildrenNumber >= inter.Children.Length)
                        {
                            return SplitInsert(inter, child2Key, newChild, i + 1);
                        }
                        else
                        {
                            inter.Insert(newChild, child2Key, i + 1);
                            SetInternalNode(inter);
                            UnPin(inter);
                            return (-1, null, null);
                        }
                    }
                    else
                    {

                        UnPin(inter);
                        return (-1, null, null);
                    }
                }

            }
            throw new NotSupportedException();
        }


        public void InsertEntry(byte[] data, RID rid)
        {
            if (fileHeader.Height == 0)
            {
                var (nc, key1, key2) = insert(data, rid, fileHeader.root);
                if (nc != -1)
                {
                    var al = AllocateInternalNode();
                    al.Insert(fileHeader.root, key1, 0);
                    al.Insert(nc, key2, 1);
                    fileHeader.root = al.pageNum;
                    fileHeader.Height++;

                    WriteHeader();
                    SetInternalNode(al);
                    UnPin(al);
                }
            }
            else
            {
                var (nc, key1, key2) = insert(data, rid, fileHeader.root, 0);
                if (nc != -1)
                {
                    var al = AllocateInternalNode();
                    al.Insert(fileHeader.root, key1, 0);
                    al.Insert(nc, key2, 1);
                    fileHeader.root = al.pageNum;
                    fileHeader.Height++;
                    WriteHeader();
                    SetInternalNode(al);
                    UnPin(al);

                }
            }
        }
        internal (LeafNode l, int id)? Find(byte[] key)
        {
            int id = fileHeader.root;
            for (int level = 0; level < fileHeader.Height; level++)
            {
                var inter = GetInternalNode(id);
                for (int i = 0; i < inter.ChildrenNumber; i++)
                {
                    if (i == inter.ChildrenNumber - 1 || BytesComp.Comp(key.AsSpan(), inter.Values.Get(i), fileHeader.AttributeType) <= 0)
                    {
                        id = inter.Children[i];
                        break;
                    }
                }
                UnPin(inter);
            }
            var leaf = GetLeafNode(id);
            for (int i = 0; i < leaf.ChildrenNumber; i++)
            {
                if (BytesComp.Comp(key.AsSpan(), leaf.Data.Get(i), fileHeader.AttributeType) <= 0)
                {
                    UnPin(leaf);
                    return (leaf, i);
                }
            }
            return null;
        }
        public int FirstLeaf()
        {
            if (fileHeader.Height == 0)
            {
                return fileHeader.root;
            }
            else
            {
                int id = fileHeader.root;
                for (int level = 0; level < fileHeader.Height; level++)
                {
                    var inter = GetInternalNode(id);
                    id = inter.Children[0];
                    UnPin(inter);
                }
                return id;
            }
        }
        public void PrintAllKeysDEBUG()
        {
            for (int id = FirstLeaf(); id != -1;)
            {
                var leaf = GetLeafNode(id);
                for (int i = 0; i < leaf.ChildrenNumber; i++)
                {
                    Console.WriteLine(BitConverter.ToInt32(leaf.Data.Get(i).ToArray()));
                }
                UnPin(leaf);
                id = leaf.Next;
            }
        }
        bool deleteInLeaf(byte[] data, int id, RID rid)
        {
            var leaf = GetLeafNode(id);
            while (true)
            {
                for (int i = 0; i < leaf.ChildrenNumber; i++)
                {
                    if (BytesComp.Comp(data.AsSpan(), leaf.Data.Get(i), fileHeader.AttributeType) == 0
                        && leaf.ridPage[i] == rid.PageID && leaf.ridSlot[i] == rid.SlotID)
                    {
                        leaf.Delete(i);
                        if (leaf.ChildrenNumber == 0)
                        {
                            SetLeafNode(leaf);
                            UnPin(leaf);
                            DeallocateNode(leaf.pageNum);
                            if (leaf.Prev != -1)
                            {
                                var pleav = GetLeafNode(leaf.Prev);
                                pleav.Next = leaf.Next;
                                SetLeafNode(pleav);
                                UnPin(pleav);
                            }
                            if (leaf.Next != -1)
                            {
                                var pnext = GetLeafNode(leaf.Next);
                                pnext.Prev = leaf.Prev;
                                SetLeafNode(pnext);
                                UnPin(pnext);
                            }
                            return true;
                        }
                        else
                        {
                            UnPin(leaf);
                            return false;
                        }
                    }
                    else if (BytesComp.Comp(data.AsSpan(), leaf.Data.Get(i), fileHeader.AttributeType) < 0)
                    {
                        UnPin(leaf);
                        return false;
                    }
                }
                UnPin(leaf);
                if (leaf.Next != -1)
                {

                    leaf = GetLeafNode(leaf.Next);
                }
                else
                {
                    return false;
                }
            }
        }
        bool deleteInInternal(byte[] data, int id, int level, RID rid)
        {
            var inter = GetInternalNode(id);
            for (int i = 0; i < inter.ChildrenNumber; i++)
            {
                if (BytesComp.Comp(data.AsSpan(), inter.Values.Get(i), fileHeader.AttributeType) <= 0)
                {

                    bool dc = level + 1 == fileHeader.Height ? deleteInLeaf(data, inter.Children[i], rid)
                     : deleteInInternal(data, inter.Children[i], level + 1, rid);
                    if (dc)
                    {
                        inter.Delete(i);
                        if (inter.ChildrenNumber == 0)
                        {
                            SetInternalNode(inter);
                            UnPin(inter);
                            DeallocateNode(inter.pageNum);
                            return true;
                        }
                        else
                        {
                            UnPin(inter);
                            return false;
                        }
                    }
                    return false;
                }
            }
            return false;
        }

        public void DeleteEntry(byte[] data, RID rid)
        {
            bool dc = 0 == fileHeader.Height ? deleteInLeaf(data, fileHeader.root, rid)
                                 : deleteInInternal(data, fileHeader.root, 0, rid);
            if (dc)
            {
                Init();
            }
        }
        public void ForcePages()
        {
            WriteHeader();
            file.ForcePages();
        }

        void UnPin(in InternalNode node)
        {
            file.UnPin(node.pageNum);
        }
        void UnPin(in LeafNode node)
        {
            file.UnPin(node.pageNum);
        }
        void SetInternalNode(in InternalNode node)
        {
            node.WriteBack(fileHeader.AttributeLength);
            file.MarkDirty(node.pageNum);
        }

        void SetLeafNode(in LeafNode node)
        {
            node.WriteBack(fileHeader.AttributeLength);
            file.MarkDirty(node.pageNum);
        }
        /// <summary>
        /// 销毁一个节点
        /// </summary>
        /// <param name="pageNum"></param>
        internal void DeallocateNode(int pageNum)
        {
            file.DeallocatePage(pageNum);
        }
        /// <summary>
        /// 分配一个内部节点
        /// </summary>
        /// <returns></returns>
        internal InternalNode AllocateInternalNode()
        {
            var page = InternalNode.AllocateEmpty(fileHeader.AttributeLength);
            page.pageNum = file.AllocatePage();
            page.Raw = file.GetPageData(page.pageNum);
            SetInternalNode(page);
            return page;
        }

        /// <summary>
        /// 分配一个叶子节点
        /// </summary>
        /// <returns></returns>
        internal LeafNode AllocateLeafNode()
        {
            var page = LeafNode.AllocateEmpty(fileHeader.AttributeLength);
            page.pageNum = file.AllocatePage();
            page.Raw = file.GetPageData(page.pageNum);
            SetLeafNode(page);

            return page;
        }

        /// <summary>
        /// 获取编号为 `id` 的内部节点，使用完毕后务必使用UnPin
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal InternalNode GetInternalNode(int id)
        {
            var node = InternalNode.AllocateEmpty(fileHeader.AttributeLength);
            node.pageNum = id;
            InternalNode.FromByteArray(file.GetPageData(id), fileHeader.AttributeLength, ref node);
            return node;
        }
        /// <summary>
        /// 获取编号为 `id` 的叶子节点，使用完毕后务必使用UnPin
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal LeafNode GetLeafNode(int id)
        {
            var node = LeafNode.AllocateEmpty(fileHeader.AttributeLength);
            node.pageNum = id;
            LeafNode.FromByteArray(file.GetPageData(id), fileHeader.AttributeLength, ref node);
            return node;
        }
    }
}