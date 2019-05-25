using System;
using System.IO;
using Xunit;
using HYBase.IndexManager;
using static HYBase.Utils.Utils;
using System.Runtime.InteropServices;
namespace HYBase.UnitTests
{
    public class IndexFileTest
    {
        [Fact]
        void DataNodeMarshalTest()
        {
            const int attributeLength = 4;
            const int sizeCounts0 = 4060 * 4 / (1 + attributeLength * 4);
            const int sizeCounts = sizeCounts0 - sizeCounts0 % 4;
            DataNode node = new DataNode();
            node.Father = 1;
            node.Prev = 3;
            node.Next = 5;
            node.ChildrenNumber = 4;
            node.Data = new byte[sizeCounts * 4];
            node.Valid = new bool[sizeCounts];
            node.Valid[0] = true;
            node.Valid[1] = true;
            node.Valid[2] = false;
            node.Valid[3] = true;
            Buffer.BlockCopy(new int[] { 1, 8, 4, 2 }, 0, node.Data, 0, 16);
            var tmp = DataNode.AllocateEmpty(attributeLength);
            Assert.Equal(node, DataNode.FromByteArray(DataNode.ToByteArray(node, attributeLength, new byte[4092]), attributeLength, ref tmp));
        }

        [Fact]
        void InternalNodeMarshalTest()
        {
            const int attributeLength = 4;
            const int sizeCounts0 = 4086 * 4 / (1 + 4 * 4 + attributeLength * 4);
            const int sizeCounts = sizeCounts0 - sizeCounts0 % 4;
            InternalNode node = new InternalNode();

            node.Father = -1;
            node.ChildrenNumber = 4;
            node.Valid = new bool[sizeCounts];
            node.Valid[0] = true;
            node.Valid[1] = true;
            node.Valid[2] = false;
            node.Valid[3] = true;


            node.Children = new int[sizeCounts];
            node.Children[0] = 1;
            node.Children[1] = 19;
            node.Children[2] = 20;
            node.Children[3] = 123;


            node.Values = new byte[sizeCounts * 4];

            Buffer.BlockCopy(new int[] { 1, 8, 4, 2 }, 0, node.Values, 0, 16);
            var tmp = InternalNode.AllocateEmpty(attributeLength);
            Assert.Equal(node, InternalNode.FromByteArray(InternalNode.ToByteArray(node, attributeLength, new byte[4092]), attributeLength, ref tmp));
        }
    }
}