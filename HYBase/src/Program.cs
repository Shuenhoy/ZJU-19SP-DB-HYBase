using System;
using System.IO;
using System.Linq;
using LanguageExt;
using static LanguageExt.Prelude;

namespace HYBase
{
    class Program
    {
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[rng.Next(s.Length)]).ToArray());
        }
        private
          static Random rng = new Random();
        static void Generate()
        {
            using (StreamWriter sw = new StreamWriter("../insert.sql"))
            {
                for (int i = 0; i < 1_000_000; i++)
                {
                    sw.WriteLine($"insert into student values('{RandomString(5)}',{rng.Next()},{Math.Round(rng.NextDouble() * 100, 5) });");
                }
            }

        }
        static void Main(string[] args)
        {
            if (args[0] == "--gen")
            {
                Generate();
            }
            else
            {
                System.DataBaseSystem system = new System.DataBaseSystem(args.Length == 0 ? "." : args[0]);
                system.Repl();
            }
        }
    }
}
