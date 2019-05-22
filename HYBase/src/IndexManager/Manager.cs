using System;
using System.IO;
using HYBase.BufferManager;

namespace HYBase.IndexManager
{
    public class IndexManager
    {
        public IndexManager(PagedFileManager pagedManager)
        {

        }
        public Index CreateIndex(Stream file, int indexNo, AttrType attrType, int attrLength)
        {
            throw new NotImplementedException();
        }
        public Index OpenIndex(Stream file, int indexNo)
        {
            throw new NotImplementedException();
        }

    }
}