using System;
using System.Runtime.InteropServices;
namespace MiniSQL.RecordManager
{
    /*
    class RM_FileHandle {
  public:
       RM_FileHandle  ();                                  // Constructor
       ~RM_FileHandle ();                                  // Destructor
    RC GetRec         (const RID &rid, RM_Record &rec) const;
                                                           // Get a record
    RC InsertRec      (const char *pData, RID &rid);       // Insert a new record,
                                                           //   return record id
    RC DeleteRec      (const RID &rid);                    // Delete a record
    RC UpdateRec      (const RM_Record &rec);              // Update a record
    RC ForcePages     (PageNum pageNum = ALL_PAGES) const; // Write dirty page(s)
                                                           //   to disk
};
     */
    public class FileHandle
    {
        public FileHandle() { throw new NotImplementedException(); }
        public Record GetRecord(RID rid) { throw new NotImplementedException(); }
        public RID InsertRecord(Span<byte> data) { throw new NotImplementedException(); }
        public void DeleteRecord(RID rid) { throw new NotImplementedException(); }
        public void UpdateRecord(Record record) { throw new NotImplementedException(); }
        public void ForcePage(uint pageNumber) { throw new NotImplementedException(); }
        public void ForceAllPages()
        {
            throw new NotImplementedException();
        }
    }
}