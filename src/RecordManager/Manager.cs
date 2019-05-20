using System;
using System.IO;

namespace HYBase.RecordManager
{

    class RecordManager
    {
        public RecordManager(BufferManager.BufferManager bufferManager)
        {
            throw new NotImplementedException();
        }
        public Record GetRec(FileStream file, RID rid)
        {
            throw new NotImplementedException();
        }
        public void InsertRec(FileStream file, byte[] data, RID rid)
        {
            throw new NotImplementedException();
        }
        public void DeleteRec(FileStream file, RID rid)
        {
            throw new NotImplementedException();
        }
        public void UpdateRec(FileStream file, Record rec)
        {
            throw new NotImplementedException();
        }
        public void ForcePages(int pageNum)
        {
            throw new NotImplementedException();
        }
    }
}