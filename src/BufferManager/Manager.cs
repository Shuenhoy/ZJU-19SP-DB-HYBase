using System.Collections.Generic;
using System.IO;


namespace HYBase.BufferManager
{
    class BufferManager
    {

        private Page[] bluffer;
        private Dictionary<(FileStream file, int pageNum), int> hashTable;
        private Queue<int> freeList;
    }
}