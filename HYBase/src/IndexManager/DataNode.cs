using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Linq;

[assembly: InternalsVisibleTo("test")]
namespace HYBase.IndexManager
{
    [StructLayout(LayoutKind.Sequential, Size = 4096)]
    internal struct FileHeader
    {
        public AttrType AttributeType;
        public int AttributeLength;
    }

    internal struct DataNode
    {
        public int Father;
        public int Prev;
        public int Next;
        public int ChildrenNumber;

        public bool[] Valid;
        public byte[] Data;
        internal byte[] Raw;

        public override String ToString()
        {

            return $"({Father},{Prev},{Next},{ChildrenNumber},[{String.Join(',', Valid)}],[{ BitConverter.ToString(Data)}])";
        }
        public override bool Equals(object obj)
        {

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (DataNode)obj;
            return Father == other.Father && Prev == other.Prev
                && Next == other.Next
                && Enumerable.SequenceEqual(Data, other.Data);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return HashCode.Combine(Father.GetHashCode(), Prev.GetHashCode(),
                Next.GetHashCode(), ChildrenNumber.GetHashCode(), Valid.GetHashCode(), Data.GetHashCode());
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
            int sizeCounts = 4060 * 4 / (1 + attributeLength * 4);
            sizeCounts -= sizeCounts % 4;
            return sizeCounts;
        }
        public static byte[] ToByteArray(DataNode node, int attributeLength, byte[] ret)
        {
            Debug.Assert(node.Valid.Length == node.Data.Length / attributeLength);
            Debug.Assert(node.Valid.Length % 4 == 0 && node.Valid.Length * (1 + attributeLength * 4) < 4060 * 4);
            var sizeCounts = node.Valid.Length;
            var retSpan = ret.AsSpan();
            BitConverter.TryWriteBytes(retSpan, node.Father);
            BitConverter.TryWriteBytes(retSpan.Slice(4), node.Prev);
            BitConverter.TryWriteBytes(retSpan.Slice(8), node.Next);
            BitConverter.TryWriteBytes(retSpan.Slice(12), node.ChildrenNumber);
            Buffer.BlockCopy(node.Valid, 0, ret, 16, sizeCounts / 4);
            Buffer.BlockCopy(node.Data, 0, ret, 16 + sizeCounts / 4, sizeCounts * attributeLength);
            return ret;
        }
        public static DataNode AllocateEmpty(int attributeLength)
        {
            DataNode node = new DataNode();
            int sizeCounts = GetSizeCounts(attributeLength);
            node.Valid = new bool[sizeCounts];
            node.Data = new byte[sizeCounts * attributeLength];

            return node;
        }
        public static DataNode FromByteArray(byte[] bytes, int attributeLength, ref DataNode node)
        {
            int sizeCounts = GetSizeCounts(attributeLength);
            sizeCounts -= sizeCounts % 4; // clamp to 4s
            var bytesSpan = bytes.AsSpan();
            node.Raw = bytes;
            node.Father = BitConverter.ToInt32(bytes, 0);
            node.Prev = BitConverter.ToInt32(bytes, 4);
            node.Next = BitConverter.ToInt32(bytes, 8);
            node.ChildrenNumber = BitConverter.ToInt32(bytes, 12);

            Buffer.BlockCopy(bytes, 16, node.Valid, 0, sizeCounts / 4);

            Buffer.BlockCopy(bytes, 16 + sizeCounts / 4, node.Data, 0, sizeCounts * attributeLength);

            return node;
        }
    }

    internal struct InternalNode
    {
        public int Father;
        public int ChildrenNumber;

        public bool[] Valid;
        public int[] Children;
        public byte[] Values;
        internal byte[] Raw;
        public override String ToString()
        {
            return $"({Father},{ChildrenNumber},[{String.Join(',', Valid)}],[{String.Join(',', Children)}],[{String.Join(',', Values)}])";
        }
        public override bool Equals(object obj)
        {

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (InternalNode)obj;
            return Father == other.Father && ChildrenNumber == other.ChildrenNumber
                && Enumerable.SequenceEqual(Valid, other.Valid)
                && Enumerable.SequenceEqual(Children, other.Children)
                && Enumerable.SequenceEqual(Values, other.Values);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return HashCode.Combine(Father.GetHashCode(), ChildrenNumber.GetHashCode(), Valid.GetHashCode(), Children.GetHashCode(), Values.GetHashCode());
        }
        public static int GetSizeCounts(int attributeLength)
        {
            int sizeCounts = 4086 * 4 / (1 + 4 * 4 + attributeLength * 4);
            sizeCounts -= sizeCounts % 4;
            return sizeCounts;
        }
        public static InternalNode AllocateEmpty(int attributeLength)
        {
            InternalNode node = new InternalNode();
            int sizeCounts = GetSizeCounts(attributeLength);
            node.Valid = new bool[sizeCounts];
            node.Children = new int[sizeCounts];

            node.Values = new byte[sizeCounts * attributeLength];

            return node;
        }
        public static byte[] ToByteArray(InternalNode node, int attributeLength, byte[] ret)
        {
            Debug.Assert(node.Valid.Length == node.Children.Length && node.Children.Length == node.Values.Length / attributeLength);
            Debug.Assert(node.Valid.Length % 4 == 0 && node.Valid.Length * (1 + 4 * 4 + attributeLength * 4) < 4086 * 4);
            var sizeCounts = node.Valid.Length;
            var retSpan = ret.AsSpan();
            BitConverter.TryWriteBytes(retSpan, node.Father);
            BitConverter.TryWriteBytes(retSpan.Slice(4), node.ChildrenNumber);
            Buffer.BlockCopy(node.Valid, 0, ret, 8, sizeCounts / 4);
            Buffer.BlockCopy(node.Children, 0, ret, 8 + sizeCounts / 4, sizeCounts * 4);
            Buffer.BlockCopy(node.Values, 0, ret, 8 + sizeCounts / 4 + sizeCounts * 4, sizeCounts * attributeLength);
            return ret;
        }
        public static InternalNode FromByteArray(byte[] bytes, int attributeLength, ref InternalNode node)
        {
            int sizeCounts = GetSizeCounts(attributeLength);
            sizeCounts -= sizeCounts % 4; // clamp to 4s
            var bytesSpan = bytes.AsSpan();
            node.Father = BitConverter.ToInt32(bytes, 0);
            node.ChildrenNumber = BitConverter.ToInt32(bytes, 4);
            node.Raw = bytes;
            Buffer.BlockCopy(bytes, 8, node.Valid, 0, sizeCounts / 4);
            Buffer.BlockCopy(bytes, 8 + sizeCounts / 4, node.Children, 0, sizeCounts * 4);

            Buffer.BlockCopy(bytes, 8 + sizeCounts / 4 + sizeCounts * 4, node.Values, 0, sizeCounts * attributeLength);

            return node;
        }
    }
}