using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VNC.dbvn;
using System.Reflection;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var db = new VNDataBase().LoadDataBase(1212, "admin");

            var obj = db.GetInfo();
            PropertyInfo[] properties = obj.GetType().GetProperties();
            foreach(var item in properties)
            {
                Console.WriteLine(item.Name + ": " + item.GetValue(obj));
            }

            Console.ReadKey();
        }
    }
}
