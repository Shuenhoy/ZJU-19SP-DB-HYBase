using System;
using System.Runtime.InteropServices;
using LanguageExt;

namespace MiniSQL.BufferManager
{

    public class PageHandle
    {
        PageHandle()
        {
            throw new NotImplementedException();
        }
        Either<Memory<byte>, ErrorCode> Data { get { throw new NotImplementedException(); } }
        Either<uint, ErrorCode> PageNum { get { throw new NotImplementedException(); } }

    }
}