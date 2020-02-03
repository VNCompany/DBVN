using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNC.dbvn
{
    public enum BCodes : byte
    {
        HeaderStart = 0x1,
        HeaderEnd = 0x4,
        TextStart = 0x2,
        TextEnd = 0x3,
        Request = 0x5,
        Bel = 0x7,
        NewLine = 0xa,
        Confirmation = 0x6
    }
}
