using System;
using System.Runtime.InteropServices;

namespace HYBase.RecordManager
{
    public class Record
    {
        public Record(byte[] data, RID rid)
        {
            Data = data;
            Rid = rid;
        }
        public byte[] Data;
        public readonly RID Rid;

    }
}