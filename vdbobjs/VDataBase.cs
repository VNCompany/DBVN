using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VNC.dbvn
{
    public class VDataBase
    {
        DirectoryInfo db_dir;
        VS src;

        public static VDataBase Open(ushort auth_code, string db_name)
        {
            if (VS.SystemExists(out string d_path))
            {
                var source = VS.GetLayout(auth_code, db_name, Path.Combine(d_path, "data"));
                return new VDataBase(source);
            }
            else throw new VDataBaseException("DBVN не установлен на данной системе.", "vndbw.vdatabase", 0xff1);
        }

        public static VDataBase Create(ushort auth_code, string db_name, string author="nocopy", string description="")
        {
            if (VS.SystemExists(out string d_path))
            {
                var source = VS.SetLayout(auth_code, db_name, Path.Combine(d_path, "data"), author, description);
                return new VDataBase(source);
            }
            else throw new VDataBaseException("DBVN не установлен на данной системе.", "vndbw.vdatabase", 0xff1);
        }

        private VDataBase(VS source)
        {
            src = source;
            db_dir = new DirectoryInfo(src.FullName);
        }
    }
}
