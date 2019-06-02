using System;

namespace HYBase.RecordManager
{
    struct RecordFileHeader
    {
        public int recordSize;
        public int numberRecordsOnPage;
        public int numberPages;
    }
    public class RecordFile
    {
        public int IncreaseKey()
        {
            throw new NotImplementedException();
        }
        public Record GetRec(RID rid)
        {
            throw new NotImplementedException();
        }

        public RID InsertRec(byte[] data)
        {
            throw new NotImplementedException();
        }


        public void DeleteRec(RID rid)
        {
            throw new NotImplementedException();
        }

        public void UpdateRec(Record rec)
        {
            throw new NotImplementedException();
        }
        public void ForcePages(int pageNum)
        {
            throw new NotImplementedException();
        }
    }
}