using System;
using System.Runtime.InteropServices;
using HYBase.Utils;

namespace HYBase.RecordManager
{
    struct RecordFilePage
    {
        internal int pageNum;
        public int RecordNum;
        public int NextFree;
        public BytesItem Data;
        public bool[] Valid;
        internal byte[] Raw;

        public void WriteBack(int recordSize)
        {
            ToByteArray(this, recordSize, Raw);
        }
        public void ReSync(int attributeLength)
        {
            FromByteArray(Raw, attributeLength, ref this);
        }
        public static int GetSizeCounts(int recordSize)
        {
            // return 4;
            int sizeCounts = (4088 - 4 - 4) * 4 / (recordSize * 4 + 4);
            //  sizeCounts -= sizeCounts % 4;
            return sizeCounts;
        }
        public static byte[] ToByteArray(RecordFilePage node, int recordSize, byte[] ret)
        {
            var sizeCounts = GetSizeCounts(recordSize);
            var retSpan = ret.AsSpan();
            BitConverter.TryWriteBytes(retSpan.Slice(0), node.NextFree);
            BitConverter.TryWriteBytes(retSpan.Slice(4), node.RecordNum);

            Buffer.BlockCopy(node.Data.Bytes, 0, ret, 8, sizeCounts * recordSize);
            Buffer.BlockCopy(node.Valid, 0, ret, 8 + sizeCounts * recordSize, sizeCounts);

            return ret;
        }
        public static RecordFilePage AllocateEmpty(int recordSize)
        {
            RecordFilePage node = new RecordFilePage();
            int sizeCounts = GetSizeCounts(recordSize);
            node.NextFree = -1;
            node.RecordNum = 0;
            node.Data = new BytesItem(new byte[sizeCounts * recordSize], recordSize);
            node.Valid = new bool[sizeCounts];
            return node;
        }
        public static RecordFilePage FromByteArray(byte[] bytes, int recordSize, ref RecordFilePage node)
        {
            int sizeCounts = GetSizeCounts(recordSize);
            var bytesSpan = bytes.AsSpan();


            node.NextFree = BitConverter.ToInt32(bytes, 0);
            node.RecordNum = BitConverter.ToInt32(bytes, 4);
            node.Raw = bytes;

            bytes.AsSpan().Slice(8, sizeCounts * recordSize).CopyTo(node.Data.Span.Slice(0, sizeCounts * recordSize));
            Buffer.BlockCopy(bytes, 8 + sizeCounts * recordSize, node.Valid, 0, sizeCounts);

            return node;
        }
    }
}