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
    }
}
