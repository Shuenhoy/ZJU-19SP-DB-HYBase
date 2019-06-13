using System;

namespace HYBase.Utils
{
    public class BytesItem
    {
        Memory<byte> mem;
        int itemLength;
        public Span<byte> Span
        {
            get
            {
                return mem.Span;
            }
        }
        public byte[] Bytes
        {
            get
            {
                return mem.ToArray();
            }
        }
        public BytesItem(byte[] bytes, int len)
        {
            mem = bytes.AsMemory();
            itemLength = len;
        }
        public int Length
        {
            get
            {
                return mem.Length / itemLength;
            }
        }

        public ReadOnlySpan<byte> Get(int index)
        {
            return mem.Span.Slice(index * itemLength, itemLength);
        }
        public void Insert(byte[] item, int index)
        {
            mem.Slice(index * itemLength, (Length - index - 1) * itemLength).CopyTo(mem.Slice((index + 1) * itemLength));
            item.CopyTo(mem.Slice(index * itemLength));
        }
        public void Delete(int index)
        {
            mem.Slice((index + 1) * itemLength).CopyTo(mem.Slice((index) * itemLength));

        }

    }
}