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
        public override string ToString()
        {
            return $"({PageID},{SlotID})";
        }
        // override object.Equals
        public override bool Equals(object obj)
        {
            //
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            var other = ((RID)(obj));
            return PageID == other.PageID && SlotID == other.SlotID;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return HashCode.Combine(PageID, SlotID);
        }
    }
}