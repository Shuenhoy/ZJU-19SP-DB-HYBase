using System;
using System.Runtime.InteropServices;

namespace HYBase.BufferManager
{


    public class Manager
    {
        public Manager()
        {
            throw new NotImplementedException();
        }

        public void CreateFile(String fileName)
        {
            throw new NotImplementedException();
        }


        public void DestroyFile(String fileName)
        {
            throw new NotImplementedException();
        }

        public FileHandle OpenFile(String fileName)
        {
            throw new NotImplementedException();
        }
        public void CloseFIle(FileHandle file)
        {
            throw new NotImplementedException();
        }

        public Memory<byte> AllocateBlock()
        {
            throw new NotImplementedException();
        }
        public void DisposeBlock(Memory<byte> block)
        {
            throw new NotImplementedException();
        }


    }
}