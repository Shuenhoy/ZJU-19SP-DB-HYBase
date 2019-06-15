using System;
using System.IO;
using HYBase.Utils;
using System.Diagnostics;

namespace HYBase.RecordManager
{
    public enum CompOp
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
    public class FileScan
    {
        RecordFile rec;
        RecordFilePage page;
        bool stop;

        CompOp op;
        byte[] compValue;
        int pageNum;
        int attributeLength;
        int attributeOffset;
        int id;
        AttrType attributeType;
        public FileScan() { }
        /// <summary>
        /// 开启扫描
        /// </summary>
        /// <param name="file">要扫描的Record文件</param>
        /// <param name="attrLength">要扫描的字段</param>
        /// <param name="attrOffset">字段在记录中的偏移量</param>
        /// <param name="compOp">比较的操作符</param>
        /// <param name="value">要比较的值</param>
        public void OpenScan(RecordFile file,
            int attrLength,
            int attrOffset,
            AttrType attrType,
            CompOp compOp,
            byte[] value)
        {
            compValue = value;
            op = compOp;
            attributeLength = attrLength;
            attributeOffset = attrOffset;
            rec = file;
            page = rec.GetPage(0);
            id = 0;
            pageNum = 0;
            stop = false;
            attributeType = attrType;

            var data = page.Data.Get(id);
            var values = data.Slice(attributeOffset, attributeLength);
            var ret = new Record(data.ToArray(), new RID(pageNum, id));
            int temo = 0;
            while (!(satisfied(values) && page.Valid[id]))
            {
                temo++;

                if (!Forward()) break;
                values = page.Data.Get(id).Slice(attributeOffset, attributeLength);
            }

        }
        public void CloseScan()
        {
            if (!stop)
            {
                rec.UnPin(page);
            }
            stop = true;
        }
        public bool NextRecord(out Record r)
        {
            r = GetNextRecord();
            if (r == null)
                return false;
            return true;
        }
        bool satisfied(ReadOnlySpan<byte> value)
        {
            switch (op)
            {
                case CompOp.EQ:
                    return BytesComp.Comp(value, compValue.AsSpan(), attributeType) == 0;
                case CompOp.GE:
                    return BytesComp.Comp(value, compValue.AsSpan(), attributeType) >= 0;
                case CompOp.GT:
                    return BytesComp.Comp(value, compValue.AsSpan(), attributeType) > 0;
                case CompOp.LE:
                    return BytesComp.Comp(value, compValue.AsSpan(), attributeType) <= 0;
                case CompOp.LT:
                    return BytesComp.Comp(value, compValue.AsSpan(), attributeType) < 0;
                case CompOp.NE:
                    return BytesComp.Comp(value, compValue.AsSpan(), attributeType) != 0;
                default: throw new NotImplementedException();
            }
        }
        bool Forward()
        {
            id++;
            if (id >= page.Valid.Length)
            {
                rec.UnPin(page);
                id = 0;
                pageNum++;
                if (pageNum >= rec.fileHeader.numberPages)
                {
                    stop = true;
                    return false;
                }
                else
                {
                    page = rec.GetPage(pageNum);
                }
            }
            return true;
        }
        public Record GetNextRecord()
        {
            if (stop) return null;
            var data = page.Data.Get(id);
            var value = data.Slice(attributeOffset, attributeLength);
            Debug.Assert(satisfied(value));
            var ret = new Record(data.ToArray(), new RID(pageNum, id));
            int temo = 0;
            do
            {
                temo++;

                if (!Forward()) break;
                value = page.Data.Get(id).Slice(attributeOffset, attributeLength);
            } while (!(satisfied(value) && page.Valid[id]));

            return ret;

        }

    }
}