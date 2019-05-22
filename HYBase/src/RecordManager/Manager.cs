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

        public RecordFile CreateFile(String filename, int recordSize)
        {
            throw new NotImplementedException();
        }

        public RecordFile OpenFile(String filename)
        {
            throw new NotImplementedException();
        }
        public void DestoryFile(String filename)
        {
            throw new NotImplementedException();
        }
    }
}