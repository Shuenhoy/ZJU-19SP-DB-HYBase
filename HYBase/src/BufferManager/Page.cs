using System;
using System.IO;
using System.Runtime.InteropServices;

namespace HYBase.BufferManager
{
    [StructLayout(LayoutKind.Explicit, Size = 4096)]
    struct PageData
    {
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.I4)]
        public int nextFree;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4096 - sizeof(int))]
        [FieldOffset(4)]
        public byte[] data;

    }

    class Page
    {
        public bool Dirty;
        public int PinCount;
        public PageData page;
        public const int SIZE = 4096;
        public const int HEADER_SIZE = sizeof(int);
        public Page()
        {
            Dirty = false;
            PinCount = 0;
            page = new PageData();
            page.data = new byte[4096 - sizeof(int)];
        }

    }
}