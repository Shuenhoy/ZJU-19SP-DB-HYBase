using System;
using System.IO;
using HYBase.BufferManager;

namespace HYBase.RecordManager
{
    /// <summary>
    /// 打开、新建、销毁Record文件
    /// </summary>
    class RecordManager
    {
        PagedFileManager manager;
        public RecordManager(BufferManager.PagedFileManager pagedManager)
        {
            manager = pagedManager;
        }

        public RecordFile CreateFile(Stream file, int recordSize)
        {
            var record = new RecordFile(manager.CreateFile(file), recordSize);
            return record;
        }

        public RecordFile OpenFile(Stream file)
        {
            var record = new RecordFile(manager.OpenFile(file));
            return record;
        }
        public void DestoryFile(Stream file)
        {
            throw new NotImplementedException();
        }
    }
}