using System;

namespace MiniSQL.BufferManager
{

    public class FileHandle
    {
        public FileHandle()
        {
            throw new NotImplementedException();
        }
        public PageHandle FirstPage
        {
            get { throw new NotImplementedException(); }
        }

        public PageHandle LastPage
        {
            get { throw new NotImplementedException(); }
        }
        public PageHandle GetNextPage(uint current)
        {
            throw new NotImplementedException();
        }
        public PageHandle GetPrevPage(uint current)
        {
            throw new NotImplementedException();
        }
        public PageHandle GetThisPage(uint current)
        {
            throw new NotImplementedException();
        }
        public PageHandle AllocatePage()
        {
            throw new NotImplementedException();
        }
        public void DisposePage()
        {
            throw new NotImplementedException();
        }
        public void MarkDirty()
        {
            throw new NotImplementedException();
        }
        public void UnpinPage(uint pageNumber)
        {
            throw new NotImplementedException();
        }
        public void ForcePage(uint pageNumber)
        {
            throw new NotImplementedException();
        }
        public void ForceAllPages()
        {
            throw new NotImplementedException();
        }
    }
}