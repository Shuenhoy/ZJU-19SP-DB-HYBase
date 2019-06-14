using System;
using System.IO;
using HYBase.BufferManager;

namespace HYBase.IndexManager
{
    public class IndexManager
    {
        PagedFileManager manager;
        public IndexManager(PagedFileManager pagedManager)
        {
            manager = pagedManager;

        }
        public Index CreateIndex(Stream file, AttrType attrType, int attrLength)
        {
            var index = new Index(manager.CreateFile(file), attrType, attrLength);
            return index;
        }
        public Index OpenIndex(Stream file)
        {
            var index = new Index(manager.OpenFile(file));
            return index;
        }

    }
}