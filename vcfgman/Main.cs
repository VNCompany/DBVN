using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VNC.dbvn
{
    public class VConfigManager : IVConfigManager
    {
        DirectoryInfo path;

        public VConfigManager(string path)
        {
            this.path = new DirectoryInfo(path);
        }

        public bool ConfigExists(string cfgName)
        {
            cfgName += "ini";
            string full_path = Path.Combine(path.FullName, cfgName);

            return File.Exists(full_path);
        }

        public VConfig GetConfig(string cfgName)
        {
            cfgName += ".ini";
            string full_path = Path.Combine(path.FullName, cfgName);

            if (!File.Exists(full_path)) return null;

            List<string> lines = new List<string>();
            using (var sr = new StreamReader(full_path))
            {
                string text;
                while((text = sr.ReadLine()) != null)
                {
                    lines.Add(text);
                }
            }

            return VConfig.Parse(lines.ToArray(), full_path);
        }

        public VConfig AddConfig(string cfgName)
        {
            if (ConfigExists(cfgName)) throw new InvalidOperationException("Такой конфиг уже существует.");

            cfgName += ".ini";
            string full_path = Path.Combine(path.FullName, cfgName);

            var cfg = VConfig.Create(full_path);
            cfg.Save();
            return cfg;
        }
    }
}
