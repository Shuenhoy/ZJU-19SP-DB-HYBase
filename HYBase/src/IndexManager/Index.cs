using System;
using HYBase.RecordManager;
using HYBase.BufferManager;


namespace HYBase.IndexManager
{
    public class Index
    {
        internal FileHeader fileHeader;
        internal PagedFile file;
        public Index(AttrType attributeType, int attributeLength, PagedFile f)
        {
            fileHeader = new FileHeader()
            {
                AttributeType = attributeType,
                AttributeLength = attributeLength
            };

            file = f;
        }
        public void Init()
        {
            file.DeallocatePages(); //Deallocate all pages
            var root = InternalNode.AllocateEmpty(fileHeader.AttributeLength);
            root.pageNum = file.AllocatePage();
            SetInternalNode(root);
        }

        public void InsertEntry(byte[] data, RID rid)
        {
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