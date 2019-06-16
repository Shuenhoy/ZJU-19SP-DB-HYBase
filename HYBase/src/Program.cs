using System;

namespace HYBase
{
    class Program
    {
        static void Main(string[] args)
        {
            System.DataBaseSystem system = new System.DataBaseSystem(args.Length == 0 ? "." : args[0]);
            system.Repl();
        }
    }
}
