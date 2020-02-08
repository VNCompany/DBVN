using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNC.dbvn
{
    public interface IVConfigManager
    {
        bool ConfigExists(string cfgName);
        VConfig GetConfig(string cfgName);
        VConfig AddConfig(string cfgName);
    }
}
