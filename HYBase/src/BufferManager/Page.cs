using System;
using System.IO;
using System.Runtime.InteropServices;

namespace HYBase.BufferManager
{
    [StructLayout(LayoutKind.Explicit, Size = 4096, Pack = 1)]
    struct PageData
    {
        [FieldOffset(0)]

        public int nextFree;

        [FieldOffset(8)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4096 - 8)]
        public byte[] data;

    }
    [StructLayout(LayoutKind.Explicit, Size = 4096)]
    public struct PagedFileHeader
    {
        [FieldOffset(0)]
        public int firstFree;
        [FieldOffset(4)]
        public int numPages;
        [FieldOffset(8)]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4096 - 8)]
        public byte[] data;



    }
    class BufferBlock
    {
        public bool Dirty;
        public int PinCount;
        public PageData page;
        public const int SIZE = 4096;
        public const int HEADER_SIZE = sizeof(int);
        public BufferBlock()
        {
            Dirty = false;
            PinCount = 0;
            page = new PageData();
            page.nextFree = -1;
            page.data = new byte[4096 - 8];
        }

    }
}