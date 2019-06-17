using System;
using HYBase.System;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;


namespace HYBase.Interpreter
{
    public class Interpreter
    {
        API api;
        StreamWriter writer;
        public Interpreter(API a)
        {
            api = a;
            writer = null;
        }
        public void Exec(string input)
        {
            foreach (var c in InterpreterParser.Parse(input))
            {
                ExecCommand(c);
            }
        }
        public void ExecCommand(Command command)
        {
            switch (command)
            {
                case Select s:
                    var res = api.Select(s);
                    if (writer == null)
                        Console.WriteLine(res.ToString(Formatting.Indented));
                    else
                        writer.WriteLine(res.ToString(Formatting.Indented));
                    Console.WriteLine($"total: {res.Length()} record(s).");
                    break;
                case CreateTable c:
                    api.CreateTable(c);
                    break;
                case CreateIndex c:
                    api.CreateIndex(c);
                    break;
                case DropTable d:
                    api.DropTable(d);
                    break;
                case Insert i:
                    api.Insert(i);
                    break;
                case DropIndex d:
                    api.DropIndex(d);
                    break;
                case Quit q:
                    api.Quit(q);
                    break;
                case ExecFile e:
                    api.ExecFile(e);
                    break;
                case Output o:
                    if (o.FileName == "stdout") writer = null;
                    else writer = new StreamWriter(o.FileName);
                    break;
                case Delete d:
                    api.Delete(d);
                    break;
            }
        }
    }
}