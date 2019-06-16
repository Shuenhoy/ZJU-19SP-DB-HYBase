using System;
using System.IO;
using System.Diagnostics;
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
        Stopwatch watch;
        private BufferManager.PagedFileManager pagedFileManager;
        public DataBaseSystem(string workdir = ".")
        {
            Directory.CreateDirectory(workdir);
            Directory.SetCurrentDirectory(workdir);
            Console.WriteLine("Welcome to HYBase, a simple DBS");
            Console.WriteLine("working at " + Directory.GetCurrentDirectory());
            watch = new Stopwatch();
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
                    watch.Start();

                    interpreter.Exec(line);
                    watch.Stop();
                    Console.WriteLine($"Execution used time: {watch.Elapsed.TotalSeconds} second(s).\n");
                    api.ForcePages();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message} \n {ex.StackTrace}");
                }
            }
        }

    }
}