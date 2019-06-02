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
    /// 扫描一个Record文件，获得满足条件的记录
    /// </summary>
    class FileScan
    {
        public FileScan() { throw new NotImplementedException(); }
        /// <summary>
        /// 开启扫描
        /// </summary>
        /// <param name="file">要扫描的Record文件</param>
        /// <param name="attrLength">要扫描的字段</param>
        /// <param name="attrOffset">字段在记录中的偏移量</param>
        /// <param name="compOp">比较的操作符</param>
        /// <param name="value">要比较的值</param>
        /// <typeparam name="T">可能是int float 或string</typeparam>
        public void OpenScan(RecordFile file,
            int attrLength,
            int attrOffset,
            CompOp compOp,
            object value)
        { throw new NotImplementedException(); }
        public bool Eof() { throw new NotImplementedException(); }
        public bool NextRecord(out Record r) { throw new NotImplementedException(); }
        public void CloseScan()
        {
            throw new NotImplementedException();
        }
    }
}