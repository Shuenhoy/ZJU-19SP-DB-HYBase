using System;
using System.Runtime.InteropServices;
namespace HYBase.BufferManager
{

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