using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;
using System.IO.Compression;

namespace DBVNInstaller
{
    class Program
    {
        static bool SystemExists(out string dbvn_path)
        {
            RegistryKey reg = Registry.CurrentUser;
            var reg_software = reg.OpenSubKey("Software");
            var reg_vnc = reg_software.OpenSubKey("VNCompany");
            if (reg_vnc != null)
            {
                var reg_dbvn = reg_vnc.OpenSubKey("DBVN");
                if (reg_dbvn != null)
                {
                    var path = reg_dbvn.GetValue("path");
                    if (path != null && Directory.Exists(path.ToString()))
                    {

                        dbvn_path = path.ToString();
                        return true;
                    }
                }
            }

            dbvn_path = null;
            return false;
        }

        static void RegistryAdd(string path)
        {
            RegistryKey reg = Registry.CurrentUser;
            var reg_software = reg.OpenSubKey("SOFTWARE", true);
            RegistryKey reg_vnc;
            if ((reg_vnc = reg_software.OpenSubKey("VNCompany", true)) == null)
            {
                reg_vnc = reg_software.CreateSubKey("VNCompany").CreateSubKey("DBVN");
                reg_vnc.SetValue("path", path);
            }
            else
            {
                RegistryKey reg_dbvn;
                if ((reg_dbvn = reg_vnc.OpenSubKey("DBVN", true)) == null) reg_dbvn = reg_vnc.CreateSubKey("DBVN");
                reg_dbvn.SetValue("path", path);
                reg_dbvn.Close();
            }
            reg_vnc.Close();
        }

        static void Main(string[] args)
        {
            var _path = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VNCompany", "DBVN"));
            bool dbvn_exists = SystemExists(out string dbvn_path);
            if (dbvn_exists && args.Length > 1 && args[0] == "--repair")
            {
                _path = new DirectoryInfo(dbvn_path);
                _path.Delete(true);
            }
            else if(dbvn_exists) return;

            if (_path.Exists) _path.Delete(true);

            _path.Create();
            _path.Refresh();

            if (!File.Exists("data.bin")) return;

            try
            {
                ZipFile.ExtractToDirectory("data.bin", _path.FullName);

                if(!dbvn_exists)
                    RegistryAdd(_path.FullName);
            }
            catch(Exception ex)
            {
                using (StreamWriter sw = new StreamWriter("log.txt"))
                {
                    sw.WriteLine(ex.Message);
                }
            }
        }
    }
}
