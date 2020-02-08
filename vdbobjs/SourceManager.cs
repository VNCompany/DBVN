using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Win32;

namespace VNC.dbvn
{
    public enum VSType
    {
        Table = 0, Config = 1, Resource = 2, Cell = 3, Error = 4, Not_Exists = 5
    }

    public class VS
    {
        static Dictionary<string, string> link_types = new Dictionary<string, string>()
        {
            { "table", "tables" },
            { "cfg", "configs" },
            { "resource", "resources" },
            { "cell", "cells" },
        };

        DirectoryInfo dir;
        Regex r_link = new Regex(@"^([A-Za-z]+)://([^/?*:;{}\\]+)$");

        public string FullName => dir.FullName;

        public VS(DirectoryInfo path)
        {
            if (!Directory.Exists(path.FullName))
                throw new DirectoryNotFoundException("Директория \"" + path.FullName + "\" не существует");

            dir = path;
        }

        public VS(string string_path)
        {
            DirectoryInfo path = new DirectoryInfo(string_path);

            if (!path.Exists)
                throw new DirectoryNotFoundException("Директория \"" + path.FullName + "\" не существует");

            dir = path;
        }

        public string Src(string str, out VSType type, bool ignore_check=true, string security_key="")
        {
            Match m = r_link.Match(str);
            if (m.Success)
            {
                (string link, string value) = (m.Groups[1].Value, m.Groups[2].Value);
                if (link_types.ContainsKey(link))
                {
                    if (link == "table" && security_key != "qwsazx123rty")
                    {
                        type = VSType.Error;
                        return null;
                    }

                    int id = 4;
                    for (int i = 0; i < link_types.Keys.Count; i++)
                    {
                        if (link_types.Keys.ToArray()[i] == link)
                            id = i;
                    }
                    string result = Path.Combine(FullName, link_types[link], value);
                    type = (VSType)id;

                    if (!ignore_check && !File.Exists(result))
                    {
                        type = VSType.Not_Exists;
                        return result;
                    }

                    return result;
                }
                else
                    type = VSType.Error;
            }
            else
                type = VSType.Error;

            return null;
        }

        public static VS SetLayout(ushort auth_code, string name, string data_path, string author="nocopy", string desc="")
        {
            string dir_name = MD5Hash.Hash(auth_code.ToString() + ":" + name);
            DirectoryInfo di = new DirectoryInfo(Path.Combine(data_path, dir_name));
            if (di.Exists) throw new InvalidOperationException("База данных уже существует!");

            Directory.CreateDirectory(di.FullName);

            XDocument info = new XDocument();
            info.Add(new XElement("information",
                new XElement("auth_code", auth_code),
                new XElement("db_name", name),
                new XElement("author", author),
                new XElement("description", desc)
                ));
            info.Save(Path.Combine(di.FullName, "info.xml"));

            Directory.CreateDirectory(Path.Combine(di.FullName, "configs"));
            Directory.CreateDirectory(Path.Combine(di.FullName, "resources"));
            Directory.CreateDirectory(Path.Combine(di.FullName, "tables"));
            Directory.CreateDirectory(Path.Combine(di.FullName, "cells"));

            return new VS(di);
        }


        public static VS GetLayout(ushort auth_code, string name, string data_path)
        {
            string dir_name = MD5Hash.Hash(auth_code.ToString() + ":" + name);
            DirectoryInfo di = new DirectoryInfo(Path.Combine(data_path, dir_name));
            if (!di.Exists) throw new InvalidOperationException("База данных не существует!");

            return new VS(di);
        }

        public static bool LayoutExists(ushort auth_code, string name, string data_path)
        {
            string dir_name = MD5Hash.Hash(auth_code.ToString() + ":" + name);
            return Directory.Exists(Path.Combine(data_path, dir_name));
        }

        public static bool SystemExists(out string dbvn_path) 
        {
            RegistryKey reg = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            var reg_software = reg.OpenSubKey("SOFTWARE");
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

        public static void RegistryAdd(string path)
        {
            RegistryKey reg = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
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

        public static bool CheckValidateFileName(string filename)
        {
            char[] invalid_chars = Path.GetInvalidFileNameChars();
            foreach (char fn_char in filename)
            {
                if (fn_char == '\\' || fn_char == '/' || invalid_chars.Contains(fn_char))
                    return false;
            }

            return true;
        }
    }
}
