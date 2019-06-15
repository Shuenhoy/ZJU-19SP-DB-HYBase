using System;
using System.IO;
using HYBase.CatalogManager;
using HYBase.RecordManager;
using HYBase.IndexManager;
using HYBase.Interpreter;

namespace HYBase.System
{
    public class DataBaseSystem
    {
        internal CatalogManager.CatalogManager catalogManager;
        internal RecordManager.RecordManager recoardManager;
        internal IndexManager.IndexManager indexManager;
        internal Interpreter.Interpreter interpreter;
        private BufferManager.PagedFileManager pagedFileManager;
        public DataBaseSystem()
        {
            pagedFileManager = new BufferManager.PagedFileManager();
            recoardManager = new RecordManager.RecordManager(pagedFileManager);
            indexManager = new IndexManager.IndexManager(pagedFileManager);
            if (File.Exists("rel"))
            {
                catalogManager = new CatalogManager.CatalogManager(recoardManager,
                               new FileStream("rel", FileMode.OpenOrCreate, FileAccess.ReadWrite),
                               new FileStream("attr", FileMode.OpenOrCreate, FileAccess.ReadWrite),
                               new FileStream("index", FileMode.OpenOrCreate, FileAccess.ReadWrite));
            }
            else
            {
                catalogManager = new CatalogManager.CatalogManager();
                Directory.CreateDirectory("table");
                Directory.CreateDirectory("index");

                catalogManager.Init(recoardManager,
                              new FileStream("rel", FileMode.OpenOrCreate, FileAccess.ReadWrite),
                              new FileStream("attr", FileMode.OpenOrCreate, FileAccess.ReadWrite),
                              new FileStream("index", FileMode.OpenOrCreate, FileAccess.ReadWrite));
            }

        }

    }
}