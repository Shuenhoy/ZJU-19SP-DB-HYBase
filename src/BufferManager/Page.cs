using System;
using System.IO;

namespace HYBase.BufferManager
{
    struct PageDesc
    {
        public bool Dirty;
        public int PinCount;

    }
    class Page
    {
        public PageDesc desc;
        public byte[] data;
        public FileStream file;
    }
}