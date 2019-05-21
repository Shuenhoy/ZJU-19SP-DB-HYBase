using System;
using System.IO;
using System.Runtime.InteropServices;
using static HYBase.Utils.Utils;

namespace HYBase.BufferManager
{
    [StructLayout(LayoutKind.Explicit, Size = 4096)]
    struct PagedFileHeader
    {
        [FieldOffset(0)]
        public int firstFree;
    }
    public class PagedFile
    {
        private FileStream file;
        private BufferManager bufferManager;
        private PagedFileHeader fileHeader;

        public PagedFile(FileStream f)
        {
            var header = new byte[4096];
            file = f;
            file.Read(header, 0, 4096);
            fileHeader = ByteArrayToStructure<PagedFileHeader>(header);
        }
        public byte[] GetPageData(int pageNum)
            => bufferManager.GetPage(file, pageNum).data;

        public void MarkDirty(int pageNum)
        {
            bufferManager.MarkDirty(file, pageNum);
        }
        public void ForcePage(int pageNum)
        {
            bufferManager.ForcePage(file, pageNum);
        }
        public int AllocatePage()
        {

            if (fileHeader.firstFree != -1)
            {
                int pNum = fileHeader.firstFree;
                var page = bufferManager.GetPage(file, fileHeader.firstFree);
                bufferManager.MarkDirty(file, fileHeader.firstFree);
                fileHeader.firstFree = page.nextFree;
                page.data = new byte[4096 - sizeof(int)];
                return page;
            }
            else
            {
                int endoff = (int)file.Seek(0, SeekOrigin.End);
                int newPageNum = endoff / 4096 - 1;
                var page = new PageData();
                page.nextFree = -1;
                page.data = new byte[4096 - sizeof(int)];
                bufferManager
                return newPageNum;
            }
        }
        public void UnPin(int pageNum)
        {
            bufferManager.UnPin(file, pageNum);
        }

    }
}