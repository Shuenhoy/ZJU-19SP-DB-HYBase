using System;
using HYBase.RecordManager;
using HYBase.BufferManager;


namespace HYBase.IndexManager
{
    public class Index
    {
        internal FileHeader fileHeader;
        internal PagedFile file;
        InternalNode GetInternalNode(int id)
        {
            return InternalNode.FromByteArray(file.GetPageData(id), fileHeader.AttributeLength);
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
    }
}