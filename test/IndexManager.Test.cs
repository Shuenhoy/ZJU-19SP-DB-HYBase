using System;
using System.IO;
using System.Linq;
using Xunit;
using HYBase.BufferManager;
using HYBase.IndexManager;
using static HYBase.Utils.Utils;
using HYBase.Utils;
using System.Runtime.InteropServices;
namespace HYBase.UnitTests
{

    public class IndexFileTest
    {
        PagedFileManager pagedFileManager;
        IndexManager.IndexManager indexManager;
        static Random rand = new Random(123);
        public IndexFileTest()
        {
            pagedFileManager = new PagedFileManager();
            indexManager = new IndexManager.IndexManager(pagedFileManager);
        }
        [Fact]
        void IndexCreateTest()
        {
            MemoryStream m1 = new MemoryStream();
            var index = indexManager.CreateIndex(m1, AttrType.Int, 4);
            index.Close();
            MemoryStream m2 = new MemoryStream(m1.ToArray());

            index = indexManager.OpenIndex(m2);
            Assert.Equal(4, index.fileHeader.AttributeLength);
            Assert.Equal(AttrType.Int, index.fileHeader.AttributeType);
            Assert.Equal(0, index.fileHeader.Height);
            Assert.Equal(0, index.fileHeader.root);
        }


        [Fact]
        void IndexInsertSmallTest()
        {
            MemoryStream m1 = new MemoryStream();
            var index = indexManager.CreateIndex(m1, AttrType.Int, 4);
            index.InsertEntry(BitConverter.GetBytes(1), new RecordManager.RID(1, 1));
            index.InsertEntry(BitConverter.GetBytes(10), new RecordManager.RID(1, 2));
            index.InsertEntry(BitConverter.GetBytes(3), new RecordManager.RID(1, 3));
            index.InsertEntry(BitConverter.GetBytes(4), new RecordManager.RID(1, 4));
            index.InsertEntry(BitConverter.GetBytes(2), new RecordManager.RID(1, 5));
            {
                var l = index.Find(BitConverter.GetBytes(10));
                Assert.NotNull(l);
                var (leaf, id) = l.Value;
                Assert.Equal(1, leaf.ridPage[id]);
                Assert.Equal(2, leaf.ridSlot[id]);
            }
            {
                var l = index.Find(BitConverter.GetBytes(4));
                Assert.NotNull(l);
                var (leaf, id) = l.Value;
                Assert.Equal(1, leaf.ridPage[id]);
                Assert.Equal(4, leaf.ridSlot[id]);
            }

        }
        [Fact]
        void IndexInsertLargeTest()
        {
            MemoryStream m1 = new MemoryStream();
            var index = indexManager.CreateIndex(m1, AttrType.Int, 4);
            var lists = Enumerable.Range(0, 100000).Select(x => (x, rand.Next(), rand.Next())).ToList();
            //  lists.Shuffle();

            foreach (var (value, p, s) in lists)
            {
                index.InsertEntry(BitConverter.GetBytes(value), new RecordManager.RID(p, s));
            }


            foreach (var (value, p, s) in lists)
            {
                var ex = BitConverter.GetBytes(value);
                var l = index.Find(ex);
                Assert.NotNull(l);
                var (leaf, id) = l.Value;
                var ac = leaf.Data.Get(id).ToArray();
                Assert.Equal(ex, ac);
                Assert.Equal(p, leaf.ridPage[id]);
                Assert.Equal(s, leaf.ridSlot[id]);
            }
        }
        [Fact]
        void LeafNodeMarshalTest()
        {
            const int attributeLength = 4;
            int sizeCounts = LeafNode.GetSizeCounts(attributeLength);
            LeafNode node = new LeafNode();
            node.Father = 1;
            node.Prev = 3;
            node.Next = 5;
            node.Tag = 1;
            node.ChildrenNumber = 4;
            node.Data = new BytesItem(new byte[sizeCounts * 4], attributeLength);
            node.ridPage = new int[sizeCounts];
            Array.Copy(new[] { 1, 2, 3, 4 }, node.ridPage, 4);
            node.ridSlot = new int[sizeCounts];

            Array.Copy(new[] { 5, 2, 3, 4 }, node.ridSlot, 4);

            var temp = new int[sizeCounts];
            Array.Copy(new[] { 1, 8, 4, 2 }, temp, 4);

            MemoryMarshal.Cast<int, byte>(temp.AsSpan()).CopyTo(node.Data.Span);

            var tmp = LeafNode.AllocateEmpty(attributeLength);
            Assert.Equal(node, LeafNode.FromByteArray(LeafNode.ToByteArray(node, attributeLength, new byte[4092]), attributeLength, ref tmp));
        }

        [Fact]
        void InternalNodeMarshalTest()
        {
            const int attributeLength = 4;
            int sizeCounts = InternalNode.GetSizeCounts(attributeLength);
            InternalNode node = new InternalNode();
            node.Tag = 2;
            node.Father = -1;
            node.ChildrenNumber = 4;



            node.Children = new int[sizeCounts];
            node.Children[0] = 1;
            node.Children[1] = 19;
            node.Children[2] = 20;
            node.Children[3] = 123;


            node.Values = new BytesItem(new byte[sizeCounts * 4], attributeLength);

            var temp = new int[] { 1, 8, 4, 2 };
            MemoryMarshal.Cast<int, byte>(temp.AsSpan()).CopyTo(node.Values.Span);
            var tmp = InternalNode.AllocateEmpty(attributeLength);
            Assert.Equal(node, InternalNode.FromByteArray(InternalNode.ToByteArray(node, attributeLength, new byte[4092]), attributeLength, ref tmp));
        }
    }
}