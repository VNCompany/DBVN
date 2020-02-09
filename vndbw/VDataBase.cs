using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;

namespace VNC.dbvn
{
    public class VDataBase : IVDataBase
    {
        DirectoryInfo db_dir;
        VS src;
        VResourceManager resMan;
        VConfigManager cfgMan;
        DBTableManager dbtMan;

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

        public static bool DataBaseExists(ushort auth_code, string db_name)
        {
            if (VS.SystemExists(out string d_path))
            {
                try
                {
                    VS.GetLayout(auth_code, db_name, Path.Combine(d_path, "data"));
                    return true;
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }
            else throw new VDataBaseException("DBVN не установлен на данной системе.", "vndbw.vdatabase", 0xff1);
        }

        private VDataBase(VS source)
        {
            src = source;
            db_dir = new DirectoryInfo(src.FullName);

            resMan = new VResourceManager(Path.Combine(db_dir.FullName, "resources"));
            cfgMan = new VConfigManager(Path.Combine(db_dir.FullName, "configs"));

            dbtMan = new DBTableManager(
                Path.Combine(db_dir.FullName, "tables"),
                src,
                DBTableParser
                );
            dbtMan.MTableDeleted += TableDeleted;
            dbtMan.MTableSaved += TableSaved;
        }

        private (int, string[][]) DBTableParser(string dbpath)
        {
            using(var dbbr = new DataBaseBinaryReader(dbpath))
            {
                dbbr.LoadFile();
                return dbbr.Parse();
            }
        }

        private void TableDeleted(DBTable table)
        {
            string dbt_path = Path.Combine(db_dir.FullName, "tables", table.Name + ".table");
            if (File.Exists(dbt_path))
                File.Delete(dbt_path);
        }

        private void TableSaved(DBTable table)
        {
            string dbt_path = Path.Combine(db_dir.FullName, "tables", table.Name + ".table");

            using(var dbbr = new DataBaseBinaryReader(dbt_path))
            {
                dbbr.Load(table.Parse(), table.Increment);
                dbbr.Write();
            }
        }

        public DBInfo GetInfo()
        {
            XElement doc = XDocument.Load(Path.Combine(src.FullName, "info.xml")).Element("information");
            return new DBInfo()
            {
                auth_code = doc.Element("auth_code").Value,
                db_name = doc.Element("db_name").Value,
                author = doc.Element("author").Value,
                desription = doc.Element("description").Value
            };
        }

        /// <summary>
        /// Get full path to file.
        /// </summary>
        /// <param name="cmd">Format: [app://filename].</param>
        /// <returns>Full path to file</returns>
        public string Source(string cmd)
        {
            return src.Src(cmd, out VSType t);
        }

        public IVResourceManager Resources => resMan;
        public IVConfigManager Configs => cfgMan;
        public DBTableManager Tables => dbtMan;
    }
}
