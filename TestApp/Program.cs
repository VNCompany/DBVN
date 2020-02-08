using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNC.dbvn;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var db = new VNDataBase().DataBaseExists(1111, "admin");
            Console.WriteLine(db);
            Console.ReadKey();
        }
    }
}
