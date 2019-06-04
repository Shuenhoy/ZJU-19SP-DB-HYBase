using System;
using System.Runtime.InteropServices;

namespace HYBase.RecordManager
{
    public class Record
    {
        public Record() { throw new NotImplementedException(); }
        public byte[] Data;
        public readonly RID Rid;

    }
}