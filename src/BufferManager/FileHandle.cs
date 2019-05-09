using System;
using LanguageExt;

namespace MiniSQL.BufferManager
{

    public class FileHandle
    {
        public FileHandle()
        {
            throw new NotImplementedException();
        }
        public Either<PageHandle, ErrorCode> FirstPage
        {
            get { throw new NotImplementedException(); }
        }

        public Either<PageHandle, ErrorCode> LastPage
        {
            get { throw new NotImplementedException(); }
        }
        public Either<PageHandle, ErrorCode> GetNextPage(uint current)
        {
            throw new NotImplementedException();
        }
        public Either<PageHandle, ErrorCode> GetPrevPage(uint current)
        {
            throw new NotImplementedException();
        }
        public Either<PageHandle, ErrorCode> GetThisPage(uint current)
        {
            throw new NotImplementedException();
        }
        public Either<PageHandle, ErrorCode> AllocatePage()
        {
            throw new NotImplementedException();
        }
        public Either<Unit, ErrorCode> DisposePage()
        {
            throw new NotImplementedException();
        }
        public Either<Unit, ErrorCode> MarkDirty()
        {
            throw new NotImplementedException();
        }
        public Either<Unit, ErrorCode> UnpinPage(uint page_number)
        {
            throw new NotImplementedException();
        }
        public Either<Unit, ErrorCode> ForcePage(uint page_number)
        {
            throw new NotImplementedException();
        }
        public Either<Unit, ErrorCode> ForceAllPages()
        {
            throw new NotImplementedException();
        }
    }
}