using System;
using System.IO;
using Xunit;
using HYBase.IndexManager;
using static HYBase.Utils.Utils;
using HYBase.Utils;
using System.Runtime.InteropServices;
namespace HYBase.UnitTests
{
    public class IndexFileTest
    {
        [Fact]
        void LeafNodeMarshalTest()
        {
            const int attributeLength = 4;
            int sizeCounts = LeafNode.GetSizeCounts(attributeLength);
            LeafNode node = new LeafNode();
            node.Father = 1;
            node.Prev = 3;
            node.Next = 5;
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