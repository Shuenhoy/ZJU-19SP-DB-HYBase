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
    [StructLayout(LayoutKind.Sequential, Size = 4096 - 16)]
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
        public byte Tag;
        public int Father;
        public int Prev;
        public int Next;
        public int ChildrenNumber;
        public BytesItem Data;
        public int[] ridSlot;
        public int[] ridPage;
        internal byte[] Raw;

        public void Insert(int slot, int page, byte[] data, int index)
        {
            ChildrenNumber++;

            Data.Insert(data, index);
            Array.Copy(ridSlot, index, ridSlot, index + 1, ridSlot.Length - index - 1);
            ridSlot[index] = slot;
            Array.Copy(ridPage, index, ridPage, index + 1, ridSlot.Length - index - 1);
            ridPage[index] = page;
        }

        public void Delete(int index)
        {
            Data.Delete(index);
            Array.Copy(ridSlot, index + 1, ridSlot, index, ridSlot.Length - index - 1);
            Array.Copy(ridPage, index + 1, ridPage, index, ridSlot.Length - index - 1);
            ChildrenNumber--;
        }

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
            // return 4;
            int sizeCounts = 4061 / (attributeLength + 8);
            return sizeCounts;
        }
        public static byte[] ToByteArray(LeafNode node, int attributeLength, byte[] ret)
        {

            var sizeCounts = node.ridPage.Length;
            var retSpan = ret.AsSpan();
            retSpan[0] = node.Tag;
            BitConverter.TryWriteBytes(retSpan.Slice(1), node.Father);
            BitConverter.TryWriteBytes(retSpan.Slice(5), node.Prev);
            BitConverter.TryWriteBytes(retSpan.Slice(9), node.Next);
            BitConverter.TryWriteBytes(retSpan.Slice(13), node.ChildrenNumber);


            Buffer.BlockCopy(node.Data.Bytes, 0, ret, 17, sizeCounts * attributeLength);

            Buffer.BlockCopy(node.ridSlot, 0, ret, 17 + sizeCounts * attributeLength, sizeCounts * 4);
            Buffer.BlockCopy(node.ridPage, 0, ret, 17 + sizeCounts * (attributeLength + 4), sizeCounts * 4);

            return ret;
        }
        public static LeafNode AllocateEmpty(int attributeLength)
        {
            LeafNode node = new LeafNode();
            int sizeCounts = GetSizeCounts(attributeLength);
            node.ridSlot = new int[sizeCounts];
            node.ridPage = new int[sizeCounts];
            node.Tag = 1;
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
            node.Tag = bytes[0];
            Debug.Assert(node.Tag == 1, "This is an InternalNode!");

            node.Father = BitConverter.ToInt32(bytes, 1);
            node.Prev = BitConverter.ToInt32(bytes, 5);
            node.Next = BitConverter.ToInt32(bytes, 9);
            node.ChildrenNumber = BitConverter.ToInt32(bytes, 13);

            bytes.AsSpan().Slice(17, sizeCounts * attributeLength).CopyTo(node.Data.Span.Slice(0, sizeCounts * attributeLength));
            Buffer.BlockCopy(bytes, 17 + sizeCounts * attributeLength, node.ridSlot, 0, sizeCounts * 4);
            Buffer.BlockCopy(bytes, 17 + sizeCounts * (attributeLength + 4), node.ridPage, 0, sizeCounts * 4);

            return node;
        }

    }

    internal struct InternalNode
    {
        internal int pageNum;
        public byte Tag;

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


        public void Insert(int childId, byte[] key, int index)
        {
            Values.Insert(key, index);
            ChildrenNumber++;
            Debug.Assert(ChildrenNumber <= Children.Length);
            Array.Copy(Children, index, Children, index + 1, Children.Length - index - 1);
            Children[index] = childId;
        }

        public void Delete(int index)
        {
            Values.Delete(index);
            Array.Copy(Children, index + 1, Children, index, Children.Length - index - 1);
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
            // return 4;
            int sizeCounts = (4088 - 9) / (4 + attributeLength);
            return sizeCounts;
        }
        public static InternalNode AllocateEmpty(int attributeLength)
        {
            InternalNode node = new InternalNode();
            int sizeCounts = GetSizeCounts(attributeLength);
            node.Children = new int[sizeCounts];
            node.ChildrenNumber = 0;
            node.Tag = 2;
            node.Values = new BytesItem(new byte[sizeCounts * attributeLength], attributeLength);

            return node;
        }
        public static byte[] ToByteArray(InternalNode node, int attributeLength, byte[] ret)
        {
            var sizeCounts = node.Children.Length;
            var retSpan = ret.AsSpan();
            retSpan[0] = node.Tag;
            BitConverter.TryWriteBytes(retSpan.Slice(1), node.Father);
            BitConverter.TryWriteBytes(retSpan.Slice(5), node.ChildrenNumber);
            Buffer.BlockCopy(node.Children, 0, ret, 9, sizeCounts * 4);
            Buffer.BlockCopy(node.Values.Bytes, 0, ret, 9 + sizeCounts * 4, sizeCounts * attributeLength);
            return ret;
        }
        public static InternalNode FromByteArray(byte[] bytes, int attributeLength, ref InternalNode node)
        {
            int sizeCounts = GetSizeCounts(attributeLength);
            var bytesSpan = bytes.AsSpan();
            node.Tag = bytes[0];
            Debug.Assert(node.Tag == 2, "This is a InternalNode!");
            node.Father = BitConverter.ToInt32(bytes, 1);
            node.ChildrenNumber = BitConverter.ToInt32(bytes, 5);
            node.Raw = bytes;
            Buffer.BlockCopy(bytes, 9, node.Children, 0, sizeCounts * 4);
            bytes.AsSpan().Slice(9 + sizeCounts * 4, sizeCounts * attributeLength).CopyTo(node.Values.Span.Slice(0, sizeCounts * attributeLength));


            return node;
        }
    }
}