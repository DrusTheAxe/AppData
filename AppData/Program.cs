using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AppData
{

    class Program
    {
        static void Main(string[] args)
        {
            Command cmd = Command.CreateInstance(args);
            cmd.Execute();
        }
    }
}