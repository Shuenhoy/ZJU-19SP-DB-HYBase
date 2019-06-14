using System;
using System.IO;
using System.Runtime.InteropServices;
using static HYBase.Utils.Utils;

namespace HYBase.BufferManager
{
    /// <summary>
    /// 分页文件
    /// </summary>
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
        /// <summary>
        ///  设置分页文件某页的内容
        /// </summary>
        /// <param name="pageNum"></param>
        /// <param name="data"></param>
        public void SetPageData(int pageNum, byte[] data)
        {
            bufferManager.SetPageData(file, pageNum, data);
        }
        public byte[] GetHeader()
        {
            return fileHeader.data;
        }
        // 修改文件头部数据（注释 to XY: 暂时还没实现）
        public void SetHeader(byte[] header)
        {
            fileHeader.data = header;
            headerChanged = true;
        }
        /// <summary>
        /// 获取分页文件某页的内容
        /// </summary>
        /// <param name="pageNum"></param>
        /// <returns></returns>
        public byte[] GetPageData(int pageNum)
            => bufferManager.GetPage(file, pageNum).data;
        internal bool GetDirty(int pageNum)
            => bufferManager.GetDirty(file, pageNum);
        internal int GetPinCount(int pageNum)
            => bufferManager.GetPinCount(file, pageNum);

        /// <summary>
        /// 标记某页需要写入文件
        /// </summary>
        /// <param name="pageNum"></param>
        public void MarkDirty(int pageNum)
        {
            bufferManager.MarkDirty(file, pageNum);
        }
        /// <summary>
        /// 强制将所有文件写入文件
        /// </summary>
        public void ForcePages()
        {
            bufferManager.ForcePages(file);
        }
        /// <summary>
        /// 将某页写入文件
        /// </summary>
        /// <param name="pageNum"></param>
        public void ForcePage(int pageNum)
        {
            bufferManager.ForcePage(file, pageNum);
        }
        /// <summary>
        ///  将文件头输出到文件
        /// </summary>
        public void WriteHeader()
        {
            if (headerChanged)
            {
                file.Seek(0, SeekOrigin.Begin);
                file.Write(StructureToByteArray(fileHeader));
                headerChanged = false;
            }
        }
        /// <summary>
        /// 将所有没有被pin的文件写入文件，并从缓存移除
        /// </summary>
        public void FlushPages()
        {
            WriteHeader();
            bufferManager.FlushPages(file);
        }
        /// <summary>
        /// 销毁某页
        /// </summary>
        /// <param name="pageNum"></param>
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
        public void DeallocatePages()
        {

            fileHeader.firstFree = -1;
            fileHeader.numPages = 0;
            headerChanged = true;

        }
        /// <summary>
        /// 分配一页数据
        /// </summary>
        /// <returns></returns>
        public int AllocatePage()
        {
            headerChanged = true;
            int pageNum;
            //var page = new PageData();

            if (fileHeader.firstFree == -1)
            {
                pageNum = fileHeader.numPages;
                fileHeader.numPages++;
            }
            else
            {
                pageNum = fileHeader.firstFree;
            }
            var page = bufferManager.GetPage(file, pageNum);
            fileHeader.firstFree = fileHeader.firstFree == -1 ? -1 : page.nextFree;
            page.nextFree = -1;
            Array.Clear(page.data, 0, page.data.Length);
            MarkDirty(pageNum);
            UnPin(pageNum);
            return pageNum;
        }
        /// <summary>
        /// 减少某页的pin的数量
        /// </summary>
        /// <param name="pageNum"></param>
        public void UnPin(int pageNum)
        {
            bufferManager.UnPin(file, pageNum);
        }
        public void Close()
        {
            WriteHeader();
            ForcePages();
            file.Close();
        }

    }
}