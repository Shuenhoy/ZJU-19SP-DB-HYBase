using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using HYBase.BufferManager;
using static HYBase.Utils.Utils;
using HYBase.Utils;
using System.Runtime.InteropServices;
using System.Text;
namespace HYBase.UnitTests
{

    public class TestTest
    {

        [Fact]
        public void test()
        {

            ReadOnlySpan<byte> a = Encoding.UTF8.GetBytes("abc").AsSpan();
            Assert.Equal("abc", Encoding.UTF8.GetString(a));
        }


        [Fact]
        public void BytesItemTest()
        {
            BytesItem a = new BytesItem(new byte[6] { 1, 2, 3, 4, 5, 6 }, 1);
            a.Delete(3);
            Assert.Equal(new byte[] { 1, 2, 3, 5, 6, 6 }, a.Bytes);
            a.Insert(new byte[] { 10 }, 4);
            Assert.Equal(new byte[] { 1, 2, 3, 5, 10, 6 }, a.Bytes);
            Assert.Equal(new byte[] { 1 }, a.Get(0).ToArray());
            Assert.Equal(new byte[] { 2 }, a.Get(1).ToArray());
            Assert.Equal(new byte[] { 3 }, a.Get(2).ToArray());
            Assert.Equal(new byte[] { 5 }, a.Get(3).ToArray());
            Assert.Equal(new byte[] { 10 }, a.Get(4).ToArray());
            Assert.Equal(new byte[] { 6 }, a.Get(5).ToArray());




        }

    }
}