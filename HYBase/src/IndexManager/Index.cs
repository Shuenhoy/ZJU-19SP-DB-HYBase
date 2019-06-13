using System;
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
        public void Init()
        {
            file.DeallocatePages(); //Deallocate all pages
            var root = AllocateLeafNode();
            root.Father = -1;
            root.Prev = -1;
            root.Next = -1;
            root.WriteBack(fileHeader.AttributeLength);
            fileHeader.root = root.pageNum;
        }

        public void InsertEntry(byte[] data, RID rid)
        {
            List<int> l;
            int id = fileHeader.root;

            for (int level = 0; level <= fileHeader.Height; level++)
            {
                if (level == fileHeader.Height)
                {
                    var leaf = GetLeafNode(id);

                    for (int i = 0; i < leaf.ChildrenNumber; i++)
                    {
                        //     if (
                        //         i == leaf.ChildrenNumber - 1 || BytesComp.Comp(
                        //             data.AsSpan(),
                        //             leaf.Data.AsSpan().Slice(i * fileHeader.AttributeLength, fileHeader.AttributeLength),
                        //                 fileHeader.AttributeType) < 0)
                        //     {
                        //         if (leaf.ChildrenNumber + 1 >= leaf.ridPage.Length)
                        //         {//split

                        //         }
                        //         else
                        //         {
                        //             Buffer.BlockCopy(leaf.Data,
                        //                 i * fileHeader.AttributeLength, leaf.Data, (i + 1) * fileHeader.AttributeLength, (leaf.ChildrenNumber - i) * fileHeader.AttributeLength);
                        //         }
                        //         break;
                        //     }
                    }
                    file.UnPin(id);
                }
                else
                {
                    var inter = GetInternalNode(id);
                    for (int i = 0; i < inter.ChildrenNumber; i++)
                    {
                        // if (
                        //     i == inter.ChildrenNumber - 1 || BytesComp.Comp(
                        //         data.AsSpan(),
                        //         inter.Values.AsSpan().Slice(i * fileHeader.AttributeLength, fileHeader.AttributeLength),
                        //             fileHeader.AttributeType) < 0)
                        // {
                        //     id = inter.Children[i];
                        //     break;
                        // }
                    }
                    file.UnPin(id);
                }
            }
            throw new NotImplementedException();
        }
        public void DeleteEntry(byte[] data, RID rid)
        {
            throw new NotImplementedException();
        }
        public void ForcePages()
        {
            throw new NotImplementedException();
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
            node.WriteBack(node.pageNum);
            file.MarkDirty(node.pageNum);
        }

        void SetLeafNode(in LeafNode node)
        {
            node.WriteBack(node.pageNum);
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