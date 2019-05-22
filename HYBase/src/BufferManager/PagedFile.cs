using System;
using System.IO;
using System.Runtime.InteropServices;
using static HYBase.Utils.Utils;

namespace HYBase.BufferManager
{
    [StructLayout(LayoutKind.Explicit, Size = 4096)]
    public struct PagedFileHeader
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
        public int PageNum
        {
            get => fileHeader.numPages;
        }
        internal PagedFile(Stream f, BufferManager buffer)
        {

            var header = new byte[4096];
            file = f;
            file.Seek(0, SeekOrigin.Begin);
            file.Read(header, 0, 4096);
            headerChanged = false;
            bufferManager = buffer;
            fileHeader = ByteArrayToStructure<PagedFileHeader>(header);
        }
        public void SetPageData(int pageNum, byte[] data)
        {
            bufferManager.SetPageData(file, pageNum, data);
        }
        public byte[] GetPageData(int pageNum)
            => bufferManager.GetPage(file, pageNum).data;

        public void MarkDirty(int pageNum)
        {
            bufferManager.MarkDirty(file, pageNum);
        }
        public void ForcePages()
        {
            bufferManager.ForcePages(file);
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
                bufferManager.SetPage(file, pageNum, page);
                fileHeader.firstFree = pageNum;
                headerChanged = true;
            }
        }
        public int AllocatePage()
        {
            headerChanged = true;
            int pageNum;
            var page = new PageData();

            if (fileHeader.firstFree == -1)
            {
                pageNum = fileHeader.numPages;
                fileHeader.numPages++;
            }
            else
            {
                pageNum = fileHeader.firstFree;
                fileHeader.firstFree = bufferManager.GetPage(file, pageNum).nextFree;
            }
            page.nextFree = -1;
            page.data = new byte[4096 - 8];
            bufferManager.SetPage(file, pageNum, page);

            return pageNum;
        }
        public void UnPin(int pageNum)
        {
            bufferManager.UnPin(file, pageNum);
        }
        public void Close()
        {
            file.Close();
        }

    }
}