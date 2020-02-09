using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNC.dbvn
{
    public interface IVDataBase
    {
        string Source(string cmd);
        IVResourceManager Resources { get; }
        IVConfigManager Configs { get; }
        DBTableManager Tables { get; }
        DBInfo GetInfo();
    }

    public struct DBInfo
    {
        public string auth_code { get; set; }
        public string db_name { get; set; }
        public string author { get; set; }
        public string desription { get; set; }
    }
}
