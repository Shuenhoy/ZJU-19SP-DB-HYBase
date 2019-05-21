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
        int firstFree;
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
            => bufferManager.GetPageData(file, pageNum);

        public void MarkDirty(int pageNum)
        {
            bufferManager.MarkDirty(file, pageNum);
        }
        public void ForcePage(int pageNum)
        {
            bufferManager.ForcePage(file, pageNum);
        }
        public void UnPin(int pageNum)
        {
            bufferManager.UnPin(file, pageNum);
        }

    }
}