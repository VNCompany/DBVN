using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace VNC.dbvn
{
    public class MD5Hash
    {
        public static string Hash(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);
            MD5 md5 = MD5.Create();
            byte[] buffer = md5.ComputeHash(data);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in buffer)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
