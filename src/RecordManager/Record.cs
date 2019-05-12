using System;
using System.Runtime.InteropServices;

namespace MiniSQL.RecordManager
{
    public class Record
    {
        public Record() { }
        public Span<byte> Data { get { throw new NotImplementedException(); } }
        public RID Rid { get { throw new NotImplementedException(); } }

    }
}