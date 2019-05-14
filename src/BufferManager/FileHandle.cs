using System;
using System.Linq;
using LanguageExt;
using static HYBase.Utils.Utils;

namespace HYBase.BufferManager
{
    struct FileHeader
    {
        public int firstFree;
        public int numPages;
    };

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

        public Either<ErrorCode, PageHandle> LastPage
        {
            get => GetPrevPage(header.numPages);
        }
        private bool IsValidPageNum(int pageNum)
            => isFileOpen && pageNum >= 0 && pageNum < header.numPages;
        private Either<ErrorCode, PageHandle> GetNextOrPrevPage(int current, bool next)
        {
            if (!isFileOpen) return ErrorCode.CLOSEDFILE;
            if (current != -1 && !IsValidPageNum(current))
                return ErrorCode.INVALIDPAGE;

            var tmp = next ? Range(current + 1, header.numPages - 1) : Range(0, current - 1)
                .Reverse();

            return tmp.AggregateWhile(Either<ErrorCode, PageHandle>.Bottom, (now, s) =>
            {
                var thisPage = GetThisPage(s);
                return (thisPage.Match(
                    Left: code => code == ErrorCode.INVALIDPAGE ? true : false,
                    Right: _ => true), thisPage);
            });
        }
        public Either<ErrorCode, PageHandle> GetNextPage(int current)
            => GetNextOrPrevPage(current, true);
        public Either<ErrorCode, PageHandle> GetPrevPage(int current)
            => GetNextOrPrevPage(current, false);
        public Either<ErrorCode, PageHandle> GetThisPage(int current)
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
        public void UnpinPage(int pageID)
        {
            throw new NotImplementedException();
        }
        public void ForcePage(int pageID)
        {
            throw new NotImplementedException();
        }
        public void ForceAllPages()
        {
            throw new NotImplementedException();
        }

        private FileHeader header;
        private BufferManager bufferManager;
        bool isFileOpen;
        bool isHeaderCHanged;


    }
}