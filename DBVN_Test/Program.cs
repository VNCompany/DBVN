using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using VNC.dbvn;

namespace DBVN_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(VS.SystemExists(out string path));
            Console.WriteLine(path);
            Console.ReadLine();
        }
    }
}
