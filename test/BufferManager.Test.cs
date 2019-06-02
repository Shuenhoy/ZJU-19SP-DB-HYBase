using System;
using System.IO;
using System.Linq;
using Xunit;
using HYBase.BufferManager;
using static HYBase.Utils.Utils;
using System.Runtime.InteropServices;
namespace HYBase.UnitTests
{
    [StructLayout(LayoutKind.Explicit, Size = 4092)]
    struct TestPage
    {
        [FieldOffset(0)]
        int a;
        [FieldOffset(4)]
        int b;
        [FieldOffset(8)]
        int c;
        public TestPage(int aa, int bb, int cc)
        {
            a = aa; b = bb; c = cc;
        }
        public override String ToString()
        {
            return $"({a}, {b}, {c})";
        }
    }
    public class PagedFileTest
    {
        PagedFileManager pm;
        public PagedFileTest()
        {
            pm = new PagedFileManager();
        }


        [Fact]
        public void CreatePagedFileTest()
        {
            MemoryStream m = new MemoryStream();
            PagedFile pf = pm.CreateFile(m);
            Assert.Equal(0, pf.PageNum);
        }

        [Fact]
        public void AllocateTest()
        {
            MemoryStream m = new MemoryStream();
            PagedFile pf = pm.CreateFile(m);

            int al = pf.AllocatePage();
            Assert.Equal(0, al);
            pf.WriteHeader();

            Assert.Equal(1, GetHeaderFromStream(m).numPages);
            int a2 = pf.AllocatePage();
            Assert.Equal(1, a2);
            int a3 = pf.AllocatePage();
            Assert.Equal(2, a3);
            pf.WriteHeader();
            Assert.Equal(3, GetHeaderFromStream(m).numPages);
        }

        [Fact]
        public void DeallocateTest()
        {
            MemoryStream m = new MemoryStream();
            PagedFile pf = pm.CreateFile(m);

            int al = pf.AllocatePage();
            int a2 = pf.AllocatePage();
            int a3 = pf.AllocatePage();

            pf.WriteHeader();
            pf.DeallocatePage(1);
            pf.DeallocatePage(2);
            pf.WriteHeader();

            Assert.Equal(2, GetHeaderFromStream(m).firstFree);
            Assert.Equal(2, pf.AllocatePage());

            pf.WriteHeader();

            Assert.Equal(1, GetHeaderFromStream(m).firstFree);
            Assert.Equal(1, pf.AllocatePage());
        }

        [Fact]
        void ReadWrite1Test()
        {
            MemoryStream m = new MemoryStream();
            PagedFile pf = pm.CreateFile(m);

            int a1 = pf.AllocatePage();
            int a2 = pf.AllocatePage();
            int a3 = pf.AllocatePage();

            var p1 = new TestPage(1, 2, 3);

            var p2 = new TestPage(4, 5, 6);

            var p3 = new TestPage(7, 8, 9);

            pf.SetPageData(a1, StructureToByteArray(p1));
            pf.SetPageData(a2, StructureToByteArray(p2));
            pf.SetPageData(a3, StructureToByteArray(p3));
            Assert.Equal(p1, ByteArrayToStructure<TestPage>(pf.GetPageData(a1)));
            Assert.Equal(p2, ByteArrayToStructure<TestPage>(pf.GetPageData(a2)));
            Assert.Equal(p3, ByteArrayToStructure<TestPage>(pf.GetPageData(a3)));

            pf.ForcePages();
            Assert.Equal(p1, GetPageFromStream(m, 0));
            Assert.Equal(p2, GetPageFromStream(m, 1));
            Assert.Equal(p3, GetPageFromStream(m, 2));

            pf.SetPageData(a1, StructureToByteArray(p3));
            pf.SetPageData(a2, StructureToByteArray(p1));
            pf.SetPageData(a3, StructureToByteArray(p2));

            Assert.Equal(p1, GetPageFromStream(m, 0));
            Assert.Equal(p2, GetPageFromStream(m, 1));
            Assert.Equal(p3, GetPageFromStream(m, 2));

            pf.ForcePages();

            Assert.Equal(p3, GetPageFromStream(m, 0));
            Assert.Equal(p1, GetPageFromStream(m, 1));
            Assert.Equal(p2, GetPageFromStream(m, 2));

        }
        [Fact]
        void HeaderTest()
        {
            MemoryStream m = new MemoryStream();
            PagedFile pf = pm.CreateFile(m);
            var header = new byte[4096 - 8];
            header[3] = 4;
            header[6] = 1;
            pf.SetHeader(header);
            byte[] bytes = new byte[4096 - 8];
            m.Seek(8, SeekOrigin.Begin);
            m.Read(bytes, 0, 4096 - 8);
            Assert.Equal(header, pf.GetHeader());
            Assert.False(Enumerable.SequenceEqual(bytes, header));
            pf.WriteHeader();
            m.Seek(8, SeekOrigin.Begin);
            m.Read(bytes, 0, 4096 - 8);
            Assert.Equal(header, bytes);

        }
        [Fact]
        void ReadWrite2Test()
        {
            MemoryStream m = new MemoryStream();
            PagedFile pf = pm.CreateFile(m);

            int a1 = pf.AllocatePage();
            int a2 = pf.AllocatePage();
            int a3 = pf.AllocatePage();

            Assert.Equal(0, pf.GetPinCount(a1));
            Assert.Equal(0, pf.GetPinCount(a2));
            Assert.Equal(0, pf.GetPinCount(a3));


            Assert.Equal(0, a1);
            Assert.Equal(1, a2);
            Assert.Equal(2, a3);

            Assert.True(pf.GetDirty(a1));
            Assert.True(pf.GetDirty(a2));
            Assert.True(pf.GetDirty(a3));

            pf.ForcePages();
            Assert.False(pf.GetDirty(a1));
            Assert.False(pf.GetDirty(a2));
            Assert.False(pf.GetDirty(a3));

            var p1 = new TestPage(1, 2, 3);

            var p2 = new TestPage(4, 5, 6);

            var p3 = new TestPage(7, 8, 9);

            pf.SetPageData(a1, StructureToByteArray(p1));
            pf.SetPageData(a2, StructureToByteArray(p2));
            pf.SetPageData(a3, StructureToByteArray(p3));

            Assert.True(pf.GetDirty(a1));
            Assert.True(pf.GetDirty(a2));
            Assert.True(pf.GetDirty(a3));

            Assert.Equal(p1, ByteArrayToStructure<TestPage>(pf.GetPageData(a1)));
            Assert.Equal(p2, ByteArrayToStructure<TestPage>(pf.GetPageData(a2)));
            Assert.Equal(p3, ByteArrayToStructure<TestPage>(pf.GetPageData(a3)));
            pf.UnPin(a1);
            pf.UnPin(a2);
            pf.UnPin(a3);

            pf.ForcePages();
            Assert.Equal(p1, GetPageFromStream(m, 0));
            Assert.Equal(p2, GetPageFromStream(m, 1));
            Assert.Equal(p3, GetPageFromStream(m, 2));

            var d1 = pf.GetPageData(a1);
            var d2 = pf.GetPageData(a2);
            var d3 = pf.GetPageData(a3);

            Assert.Equal(1, pf.GetPinCount(a1));
            Assert.Equal(1, pf.GetPinCount(a2));
            Assert.Equal(1, pf.GetPinCount(a3));


            StructureToByteArray(p3).CopyTo(d1, 0);
            StructureToByteArray(p1).CopyTo(d2, 0);
            StructureToByteArray(p2).CopyTo(d3, 0);

            Assert.Equal(p1, GetPageFromStream(m, 0));
            Assert.Equal(p2, GetPageFromStream(m, 1));
            Assert.Equal(p3, GetPageFromStream(m, 2));

            pf.ForcePages();

            Assert.Equal(p1, GetPageFromStream(m, 0));
            Assert.Equal(p2, GetPageFromStream(m, 1));
            Assert.Equal(p3, GetPageFromStream(m, 2));

            pf.MarkDirty(a1);
            pf.MarkDirty(a2);
            pf.MarkDirty(a3);

            Assert.True(pf.GetDirty(a1));
            Assert.True(pf.GetDirty(a2));
            Assert.True(pf.GetDirty(a3));

            pf.ForcePages();
            Assert.Equal(p3, GetPageFromStream(m, 0));
            Assert.Equal(p1, GetPageFromStream(m, 1));
            Assert.Equal(p2, GetPageFromStream(m, 2));

        }
        TestPage GetPageFromStream(Stream m, int pageNum)
        {
            byte[] bytes = new byte[4092];
            m.Seek((pageNum + 1) * 4096 + 8, SeekOrigin.Begin);
            m.Read(bytes, 0, 4092);
            return ByteArrayToStructure<TestPage>(bytes);
        }
        PagedFileHeader GetHeaderFromStream(Stream m)
        {
            byte[] bytes = new byte[4096];
            m.Seek(0, SeekOrigin.Begin);
            m.Read(bytes, 0, 4096);
            return ByteArrayToStructure<PagedFileHeader>(bytes);
        }

        int Add(int x, int y)
        {
            return x + y;
        }
    }
}