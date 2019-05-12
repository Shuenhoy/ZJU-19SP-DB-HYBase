using System;
using System.Runtime.InteropServices;
namespace HYBase.RecordManager
{

    public class FileHandle
    {
        public FileHandle() { throw new NotImplementedException(); }
        public Record GetRecord(RID rid) { throw new NotImplementedException(); }
        public RID InsertRecord(Span<byte> data) { throw new NotImplementedException(); }
        public void DeleteRecord(RID rid) { throw new NotImplementedException(); }
        public void UpdateRecord(Record record) { throw new NotImplementedException(); }
        public void ForcePage(uint pageID) { throw new NotImplementedException(); }
        public void ForceAllPages()
        {
            throw new NotImplementedException();
        }
    }
}