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
            VResourceManager rss = new VResourceManager("C://Users/vnezn/Desktop/resources");
            rss.AddResource("text.txt", Encoding.UTF8.GetBytes("Привет, мир!"));
        }
    }
}
