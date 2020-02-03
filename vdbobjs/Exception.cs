using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNC.dbvn
{
    public class VDataBaseException : Exception
    {
        public string DLL { get; private set; }
        public int ErrorCode { get; private set; }

        public VDataBaseException(string msg, string dll, int error_code = 0x0) : base(msg)
        {
            DLL = dll;
            ErrorCode = error_code;
        }
    }
}
