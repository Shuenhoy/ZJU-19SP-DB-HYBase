using System;
using HYBase.System;

namespace HYBase.Interpreter
{
    public class Interpreter
    {
        API api;
        public Interpreter(API a)
        {
            api = a;
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
                    api.Select(s);
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
                case DropIndex d:
                    api.DropIndex(d);
                    break;
                case Quit q:
                    api.Quit(q);
                    break;
                case ExecFile e:
                    api.ExecFile(e);
                    break;
                case Delete d:
                    api.Delete(d);
                    break;
            }
        }
    }
}