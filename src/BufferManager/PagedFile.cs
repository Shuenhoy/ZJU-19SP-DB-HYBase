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
        [FieldOffset(4)]
        public int numPages;
    }
    public class PagedFile
    {
        private Stream file;
        private BufferManager bufferManager;
        private PagedFileHeader fileHeader;
        private bool headerChanged;
        public PagedFile(Stream f)
        {

            var header = new byte[4096];
            file = f;
            file.Read(header, 0, 4096);
            headerChanged = false;
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
        public void WriteHeader()
        {
            if (headerChanged)
            {
                file.Seek(0, SeekOrigin.Begin);
                file.Write(StructureToByteArray(fileHeader));
                headerChanged = false;
            }
        }
        public void FlushPages()
        {
            WriteHeader();
            bufferManager.FlushPages(file);
        }
        public void DeallocatePage(int pageNum)
        {
            if (fileHeader.numPages > pageNum)
            {
                var page = bufferManager.GetPage(file, pageNum);
                page.nextFree = fileHeader.firstFree;
                fileHeader.firstFree = pageNum;
                headerChanged = true;
            }
        }
        public int AllocatePage()
        {
            headerChanged = true;

            if (fileHeader.firstFree != -1)
            {
                int pNum = fileHeader.firstFree;
                var page = bufferManager.GetPage(file, fileHeader.firstFree);
                bufferManager.MarkDirty(file, fileHeader.firstFree);
                fileHeader.firstFree = page.nextFree;
                page.data = new byte[4096 - sizeof(int)];
                return pNum;
            }
            else
            {

                int newPageNum = fileHeader.numPages;
                var page = new PageData();
                page.nextFree = -1;
                page.data = new byte[4096 - sizeof(int)];
                fileHeader.numPages++;

                return newPageNum;
            }
        }
        public void UnPin(int pageNum)
        {
            bufferManager.UnPin(file, pageNum);
        }

    }
}