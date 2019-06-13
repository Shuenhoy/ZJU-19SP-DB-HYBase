using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Linq;
using HYBase.Utils;
using HYBase.RecordManager;

[assembly: InternalsVisibleTo("test")]
namespace HYBase.IndexManager
{
    [StructLayout(LayoutKind.Sequential, Size = 4096 - 8)]
    internal struct FileHeader
    {
        public AttrType AttributeType;
        public int AttributeLength;
        public int Height;
        public int root;
    }

    internal struct LeafNode
    {
        internal int pageNum;

        public int Father;
        public int Prev;
        public int Next;
        public int ChildrenNumber;
        public BytesItem Data;
        public int[] ridSlot;
        public int[] ridPage;
        internal byte[] Raw;

        public override String ToString()
        {

            return $"({Father},{Prev},{Next},{ChildrenNumber},{Data.Bytes.Length}[{ BitConverter.ToString(Data.Bytes)}],({String.Join(',', ridSlot)}),({String.Join(',', ridPage)}))";
        }
        public override bool Equals(object obj)
        {

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (LeafNode)obj;
            return Father == other.Father && Prev == other.Prev
                && Next == other.Next
                && Enumerable.SequenceEqual(Data.Bytes, other.Data.Bytes);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return HashCode.Combine(Father.GetHashCode(), Prev.GetHashCode(),
                Next.GetHashCode(), ChildrenNumber.GetHashCode(), ridPage.GetHashCode(), ridSlot.GetHashCode(), Data.GetHashCode());
        }

        public void WriteBack(int attributeLength)
        {
            ToByteArray(this, attributeLength, Raw);
        }
        public void ReSync(int attributeLength)
        {
            FromByteArray(Raw, attributeLength, ref this);
        }
        public static int GetSizeCounts(int attributeLength)
        {
            int sizeCounts = 4052 / (attributeLength + 8);
            return sizeCounts;
        }
        public static byte[] ToByteArray(LeafNode node, int attributeLength, byte[] ret)
        {

            var sizeCounts = node.ridPage.Length;
            var retSpan = ret.AsSpan();
            BitConverter.TryWriteBytes(retSpan, node.Father);
            BitConverter.TryWriteBytes(retSpan.Slice(4), node.Prev);
            BitConverter.TryWriteBytes(retSpan.Slice(8), node.Next);
            BitConverter.TryWriteBytes(retSpan.Slice(12), node.ChildrenNumber);


            Buffer.BlockCopy(node.Data.Bytes, 0, ret, 16, sizeCounts * attributeLength);

            Buffer.BlockCopy(node.ridSlot, 0, ret, 16 + sizeCounts * attributeLength, sizeCounts * 4);
            Buffer.BlockCopy(node.ridPage, 0, ret, 16 + sizeCounts * (attributeLength + 4), sizeCounts * 4);

            return ret;
        }
        public static LeafNode AllocateEmpty(int attributeLength)
        {
            LeafNode node = new LeafNode();
            int sizeCounts = GetSizeCounts(attributeLength);
            node.ridSlot = new int[sizeCounts];
            node.ridPage = new int[sizeCounts];
            node.Father = -1;
            node.Next = node.Prev = -1;
            node.ChildrenNumber = 0;
            node.Data = new BytesItem(new byte[sizeCounts * attributeLength], attributeLength);

            return node;
        }
        public static LeafNode FromByteArray(byte[] bytes, int attributeLength, ref LeafNode node)
        {
            int sizeCounts = GetSizeCounts(attributeLength);
            var bytesSpan = bytes.AsSpan();
            node.Raw = bytes;
            node.Father = BitConverter.ToInt32(bytes, 0);
            node.Prev = BitConverter.ToInt32(bytes, 4);
            node.Next = BitConverter.ToInt32(bytes, 8);
            node.ChildrenNumber = BitConverter.ToInt32(bytes, 12);

            bytes.AsSpan().Slice(16, sizeCounts * attributeLength).CopyTo(node.Data.Span.Slice(0, sizeCounts * attributeLength));
            Buffer.BlockCopy(bytes, 16 + sizeCounts * attributeLength, node.ridSlot, 0, sizeCounts * 4);
            Buffer.BlockCopy(bytes, 16 + sizeCounts * (attributeLength + 4), node.ridPage, 0, sizeCounts * 4);

            return node;
        }

    }

    internal struct InternalNode
    {
        internal int pageNum;
        public int Father;
        public int ChildrenNumber;

        public int[] Children;
        public BytesItem Values;
        internal byte[] Raw;
        public override String ToString()
        {
            return $"({Father},{ChildrenNumber},[{String.Join(',', Children)}],[{ BitConverter.ToString(Values.Bytes)}])";
        }
        public override bool Equals(object obj)
        {

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (InternalNode)obj;
            return Father == other.Father && ChildrenNumber == other.ChildrenNumber
                && Enumerable.SequenceEqual(Children, other.Children)
                && Enumerable.SequenceEqual(Values.Bytes, other.Values.Bytes);
        }
        public void WriteBack(int attributeLength)
        {
            ToByteArray(this, attributeLength, Raw);
        }
        public void ReSync(int attributeLength)
        {
            FromByteArray(Raw, attributeLength, ref this);
        }
        // override object.GetHashCode
        public override int GetHashCode()
        {
            return HashCode.Combine(Father.GetHashCode(), ChildrenNumber.GetHashCode(), Children.GetHashCode(), Values.GetHashCode());
        }
        public static int GetSizeCounts(int attributeLength)
        {
            int sizeCounts = 4086 / (4 + attributeLength);
            return sizeCounts;
        }
        public static InternalNode AllocateEmpty(int attributeLength)
        {
            InternalNode node = new InternalNode();
            int sizeCounts = GetSizeCounts(attributeLength);
            node.Children = new int[sizeCounts];

            node.Values = new BytesItem(new byte[sizeCounts * attributeLength], attributeLength);

            return node;
        }
        public static byte[] ToByteArray(InternalNode node, int attributeLength, byte[] ret)
        {
            var sizeCounts = node.Children.Length;
            var retSpan = ret.AsSpan();
            BitConverter.TryWriteBytes(retSpan, node.Father);
            BitConverter.TryWriteBytes(retSpan.Slice(4), node.ChildrenNumber);
            Buffer.BlockCopy(node.Children, 0, ret, 8, sizeCounts * 4);
            Buffer.BlockCopy(node.Values.Bytes, 0, ret, 8 + sizeCounts * 4, sizeCounts * attributeLength);
            return ret;
        }
        public static InternalNode FromByteArray(byte[] bytes, int attributeLength, ref InternalNode node)
        {
            int sizeCounts = GetSizeCounts(attributeLength);
            var bytesSpan = bytes.AsSpan();
            node.Father = BitConverter.ToInt32(bytes, 0);
            node.ChildrenNumber = BitConverter.ToInt32(bytes, 4);
            node.Raw = bytes;
            Buffer.BlockCopy(bytes, 8, node.Children, 0, sizeCounts * 4);
            bytes.AsSpan().Slice(8 + sizeCounts * 4, sizeCounts * attributeLength).CopyTo(node.Values.Span.Slice(0, sizeCounts * attributeLength));


            return node;
        }
    }
}