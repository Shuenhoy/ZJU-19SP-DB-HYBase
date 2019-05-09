using LanguageExt;
using System;
using System.Runtime.InteropServices;

namespace MiniSQL.BufferManager
{
    public class Manager
    {
        public Manager()
        {
            throw new NotImplementedException();
        }
        public Either<Unit, ErrorCode> CreateFile(String filename)
        {
            throw new NotImplementedException();
        }
        public Either<Unit, ErrorCode> DestroyFile(String filename)
        {
            throw new NotImplementedException();
        }
        public Either<FileHandle, ErrorCode> OpenFile(String filename)
        {
            throw new NotImplementedException();
        }
        public Either<Unit, ErrorCode> CloseFIle(FileHandle file)
        {
            throw new NotImplementedException();
        }

        public Either<Memory<byte>, ErrorCode> AllocateBlock()
        {
            /*
                Span<T> valSpan = MemoryMarshal.CreateSpan(ref val, 1);
                return MemoryMarshal.Cast<T, byte>(valSpan); */
            throw new NotImplementedException();
        }
        public Either<Unit, ErrorCode> DisposeBlock(Memory<byte> block)
        {
            throw new NotImplementedException();
        }


    }
}