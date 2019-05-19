using System.Collections.Generic;
using System.IO;
using HYBase.Utils;
using LanguageExt;
using static LanguageExt.Prelude;

namespace HYBase.BufferManager
{
    class BufferManager
    {

        public byte[] GetPageData(FileStream file, int pageNum)
            => cache.TryGet((file, pageNum)).BiBind<byte[]>(Some: page => page.data
                , None: () =>
                {

                }).First();

        private LRUCache<(FileStream file, int pageNum), Page> cache;
    }
}