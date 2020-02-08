using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VNC.dbvn
{
    public interface IVResource
    {
        void Delete();
        bool Exists { get; }
        FileInfo FileInfo();
        string FullName { get; }
        void GetBytes(byte[] buffer, int offset, int count = -1);
    }

    public interface IVResourceManager
    {
        IVResource this[string res_name] { get; }
        bool ResourceExists(string res_name);
        void AddResource(FileInfo file, string name = null);
        void AddResource(string res_name, byte[] bytes);
        void AddResource(string res_name, string text, Encoding encoding);
        void AddResource(string res_name, string text);
        Stream AddResourceStream(string res_name);
    }
}
