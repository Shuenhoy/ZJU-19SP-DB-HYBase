using System;
using System.IO;

namespace HYBase.RecordManager
{
    enum CompOp
    {
        EQ,
        LT,
        GT,
        LE,
        GE,
        NE,
        NO
    }

    /// <summary>
    /// The RM_FileScan class provides clients the capability to perform scans over the records of an RM component file,
    ///  where a scan may be based on a specified condition. 
    /// </summary>
    class FileScan
    {
        public FileScan() { throw new NotImplementedException(); }
        void OpenScan<T>(FileStream file,
            int attrLength,
            int attrOffset,
            CompOp compOp,
            T value)
        { throw new NotImplementedException(); }

        Record NextRecord() { throw new NotImplementedException(); }
        void CloseScan()
        {
            throw new NotImplementedException();
        }
    }
}