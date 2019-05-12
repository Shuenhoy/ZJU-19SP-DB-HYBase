using System;
using System.Runtime.InteropServices;
namespace MiniSQL.BufferManager
{

    public class PageHandle
    {
        PageHandle()
        {
            throw new NotImplementedException();
        }
        Memory<byte> Data { get { throw new NotImplementedException(); } }
        uint PageNum { get { throw new NotImplementedException(); } }

    }
}