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

        public void CreateFile(String filename)
        {
            throw new NotImplementedException();
        }


        public void DestroyFile(String filename)
        {
            throw new NotImplementedException();
        }

        public FileHandle OpenFile(String filename)
        {
            throw new NotImplementedException();
        }
        public void CloseFIle(FileHandle file)
        {
            throw new NotImplementedException();
        }

        public Memory<byte> AllocateBlock()
        {
            /*
                Span<T> valSpan = MemoryMarshal.CreateSpan(ref val, 1);
                return MemoryMarshal.Cast<T, byte>(valSpan); */
            throw new NotImplementedException();
        }
        public void DisposeBlock(Memory<byte> block)
        {
            throw new NotImplementedException();
        }


    }
}