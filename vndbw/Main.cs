using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNC.dbvn
{
    public class DataBaseWorker
    {
        public static IVDataBase LoadDB(ushort auth_code, string db_name)
        {
            return VDataBase.Open(auth_code, db_name);
        }

        public static IVDataBase CreateDB(ushort auth_code, string db_name, string author = "nocopy", string description = "")
        {
            return VDataBase.Create(auth_code, db_name, author, description);
        }

        public static bool DBExists(ushort auth_code, string db_name)
        {
            return VDataBase.DataBaseExists(auth_code, db_name);
        }
    }
}
