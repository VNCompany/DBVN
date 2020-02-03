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
            VConfig cfg = VConfig.Create(@"C://Users/vnezn/Desktop/test.cfg");

            var sDefault = cfg.GetSection();

            sDefault.Add("hello", "world");

            Console.WriteLine(cfg.SectionExist("Def"));
            Console.WriteLine(cfg.SectionExist("Default"));
            Console.WriteLine(sDefault.ItemExists("world"));
            Console.WriteLine(sDefault.ItemExists("hello"));

            var sMainSettings = cfg.Add("MainSettings");
            sMainSettings.Add("main", "land");

            cfg.Save();
            Console.ReadLine();
        }
    }
}
