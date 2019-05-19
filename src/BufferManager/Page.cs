using System;
using System.IO;

namespace HYBase.BufferManager
{

    class Page
    {
        public bool Dirty;
        public int PinCount;
        public byte[] data;
        public const int SIZE = 4096;
        public const int HEADER_SIZE = sizeof(int);
        public Page()
        {
            Dirty = false;
            PinCount = 0;
            data = new byte[4096];
        }

    }
}