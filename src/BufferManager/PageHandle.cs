using System;
using System.Runtime.InteropServices;
namespace HYBase.BufferManager
{
    struct PageHeader
    {
        public int nextFree;
    }
    public class PageHandle
    {
        public PageHandle()
        {
            throw new NotImplementedException();
        }
        public readonly Memory<byte> Data;
        public readonly int PageID;
    }
}