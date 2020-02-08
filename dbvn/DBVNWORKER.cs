using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace VNC.dbvn
{
    public class VNDataBase
    {
        string dbvn_path = null;

        object dbw;
        MethodInfo loaddb;
        MethodInfo createdb;
        MethodInfo existdb;
        public static bool VNDBInstalled()
        {
            return VS.SystemExists(out string db);
        }

        public VNDataBase()
        {
            if (VS.SystemExists(out string path) && File.Exists(Path.Combine(path, "vndbw.dll")))
            {
                dbvn_path = path;

                Assembly asm = Assembly.LoadFrom(Path.Combine(path, "vndbw.dll"));
                Type t = asm.GetType("VNC.dbvn.DataBaseWorker", true, true);
                dbw = Activator.CreateInstance(t);

                loaddb = t.GetMethod("LoadDB");
                createdb = t.GetMethod("CreateDB");
                existdb = t.GetMethod("DBExists");
            }
            else
                throw new VDataBaseException("DBVN не установлен в системе.", "DBVNWORKER");
        }
        
        public IVDataBase LoadDataBase(ushort auth_code, string db_name)
        {
            return loaddb.Invoke(dbw, new object[] { auth_code, db_name }) as IVDataBase;
        }
        
        public IVDataBase CreateDataBase(ushort auth_code, string db_name, string author = "nocopy", string description = "")
        {
            return createdb.Invoke(dbw, new object[] { auth_code, db_name, author, description }) as IVDataBase;
        }
        
        public bool DataBaseExists(ushort auth_code, string db_name)
        {
            return (bool)existdb.Invoke(dbw, new object[] { auth_code, db_name });
        }
    }
}
