using System;
using System.IO;
using static HYBase.Utils.Utils;

namespace HYBase.BufferManager
{
    /// <summary>
    /// 勇于创建分页文件
    /// </summary>
    public class PagedFileManager
    {
        private BufferManager buffer;
        public PagedFileManager()
        {
            buffer = new BufferManager(32);
        }
        /// <summary>
        /// 创建新的分页文件
        /// </summary>
        /// <param name="file">文件流</param>
        /// <returns>创建好的分页文件</returns>
        public PagedFile CreateFile(Stream file)
        {
            PagedFileHeader header = new PagedFileHeader();
            header.firstFree = -1;
            header.numPages = 0;
            file.Write(StructureToByteArray(header));

            PagedFile pfile = new PagedFile(file, buffer);


            return pfile;
        }
        /// <summary>
        /// 打开已有的分页文件
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public PagedFile OpenFile(Stream file)
            => new PagedFile(file, buffer);
    }
}