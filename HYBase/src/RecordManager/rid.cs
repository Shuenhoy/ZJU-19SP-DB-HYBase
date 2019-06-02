using System;
namespace HYBase.RecordManager
{
    public struct RID
    {
        public readonly int PageID;
        public readonly int SlotID;
        public RID(int pid, int sid)
        {
            PageID = pid;
            SlotID = sid;
        }
    }
}