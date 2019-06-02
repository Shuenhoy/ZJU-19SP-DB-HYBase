using System;
using System.IO;

namespace HYBase.RecordManager
{
    /// <summary>
    /// 打开、新建、销毁Record文件
    /// </summary>
    class RecordManager
    {
        public RecordManager(BufferManager.PagedFileManager pagedManager)
        {
            throw new NotImplementedException();
        }

        public RecordFile CreateFile(Stream file, int recordSize)
        {
            throw new NotImplementedException();
        }

        public RecordFile OpenFile(Stream file)
        {
            throw new NotImplementedException();
        }
        public void DestoryFile(Stream file)
        {
            throw new NotImplementedException();
        }
    }
}