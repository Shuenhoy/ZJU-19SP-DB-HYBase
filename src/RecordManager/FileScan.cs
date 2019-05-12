using System;

namespace HYBase.RecordManager
{
    enum CompOp
    {
        EQ, LT, GT, LE, GE, NE, NO
    }

    class FileScan
    {
        public FileScan() { throw new NotImplementedException(); }
        void OpenScan<T>(FileHandle fileHandle,
            int attrLength,
            int attrOffset,
            CompOp compOp,
            T value)
        { throw new NotImplementedException(); }

        Record NextRecord() { throw new NotImplementedException(); }
    }
}