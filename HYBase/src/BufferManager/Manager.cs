using System;
using System.IO;
using static HYBase.Utils.Utils;

namespace HYBase.BufferManager
{
    public class PagedFileManager
    {
        private BufferManager buffer;
        public PagedFileManager()
        {
            buffer = new BufferManager(32);
        }
        public PagedFile CreateFile(Stream file)
        {
            PagedFileHeader header = new PagedFileHeader();
            header.firstFree = -1;
            header.numPages = 0;
            file.Write(StructureToByteArray(header));

            PagedFile pfile = new PagedFile(file, buffer);


            return pfile;
        }

        public PagedFile OpenFile(Stream file)
            => new PagedFile(file, buffer);
    }
}