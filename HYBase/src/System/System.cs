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
        internal API api;
        private BufferManager.PagedFileManager pagedFileManager;
        public DataBaseSystem(string workdir = ".")
        {
            Directory.CreateDirectory(workdir);
            Directory.SetCurrentDirectory(workdir);
            Console.WriteLine("Welcome to HYBase, a simple DBS");
            Console.WriteLine("working at " + Directory.GetCurrentDirectory());

            pagedFileManager = new BufferManager.PagedFileManager();
            recoardManager = new RecordManager.RecordManager(pagedFileManager);
            indexManager = new IndexManager.IndexManager(pagedFileManager);
            if (File.Exists("rel"))
            {
                catalogManager = new CatalogManager.CatalogManager(recoardManager,
                               new FileStream("rel", FileMode.OpenOrCreate, FileAccess.ReadWrite),
                               new FileStream("attr", FileMode.OpenOrCreate, FileAccess.ReadWrite),
                               new FileStream("ind", FileMode.OpenOrCreate, FileAccess.ReadWrite));
            }
            else
            {
                catalogManager = new CatalogManager.CatalogManager();
                Directory.CreateDirectory("table");
                Directory.CreateDirectory("index");

                catalogManager.Init(recoardManager,
                              new FileStream("rel", FileMode.OpenOrCreate, FileAccess.ReadWrite),
                              new FileStream("attr", FileMode.OpenOrCreate, FileAccess.ReadWrite),
                              new FileStream("ind", FileMode.OpenOrCreate, FileAccess.ReadWrite));
            }

            api = new API(this);
            interpreter = new Interpreter.Interpreter(api);

        }
        public void Repl()
        {
            while (true)
            {
                Console.Write("HYBase> ");
                var line = Console.ReadLine();
                try
                {
                    interpreter.Exec(line);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message} \n {ex.StackTrace}");
                }
            }
        }

    }
}