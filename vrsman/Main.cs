using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VNC.dbvn
{
    public class VResourceManager : IVResourceManager
    {
        DirectoryInfo path;

        public VResourceManager(string path)
        {
            this.path = new DirectoryInfo(path);

            if (!this.path.Exists)
                throw new DirectoryNotFoundException("База повреждена");
        }

        public IVResource this[string res_name]
        {
            get
            {
                var result = path.GetFiles().FirstOrDefault(t => t.Name == res_name);
                if (result == null)
                    return null;

                return new VResource(result.FullName);
            }
        }

        public bool ResourceExists(string res_name)
        {
            return path.GetFiles().FirstOrDefault(t => t.Name == res_name) != null;
        }

        public void AddResource(FileInfo file, string name=null)
        {
            if (file == null || !file.Exists)
                throw new FileNotFoundException("Файл загрузки не найден");
            if (name == null)
                name = file.Name;
            if (ResourceExists(name))
                throw new FileNotFoundException("Ресурс с таким же именем уже существует. " + name);
            file.CopyTo(Path.Combine(path.FullName, name));
        }
        
        public void AddResource(string res_name, byte[] bytes)
        {
            FileInfo fn = new FileInfo(Path.Combine(path.FullName, res_name));
            if (fn.Exists)
                throw new FileNotFoundException("Ресурс с таким же именем уже существует. " + fn.Name);

            using(var ow = fn.OpenWrite())
            {
                ow.Write(bytes, 0, bytes.Length);
            }
        }

        public void AddResource(string res_name, string text, Encoding encoding)
        {
            AddResource(res_name, encoding.GetBytes(text));
        }

        /// <summary>
        /// Кодировка UTF-8
        /// </summary>
        /// <param name="res_name"></param>
        /// <param name="text"></param>
        public void AddResource(string res_name, string text)
        {
            AddResource(res_name, text, Encoding.UTF8);
        }

        public Stream AddResourceStream(string res_name)
        {
            FileInfo fn = new FileInfo(Path.Combine(path.FullName, res_name));
            if (fn.Exists)
                throw new FileNotFoundException("Ресурс с таким же именем уже существует. " + fn.Name);

            return new FileStream(fn.FullName, FileMode.Create, FileAccess.Write);
        }
    }

    public class VResource : IVResource
    {
        FileInfo fi;
        public VResource(string file)
        {
            fi = new FileInfo(file);

            if (!fi.Exists)
                throw new DirectoryNotFoundException("Ресурс не найден");
        }

        public void Delete()
        {
            fi.Delete();
        }

        public bool Exists
        {
            get { return fi.Exists; }
        }

        public FileInfo FileInfo()
        {
            return fi;
        }

        public string FullName => fi.FullName;

        public void GetBytes(byte[] buffer, int offset, int count=-1)
        {
            if (count == -1) count = Convert.ToInt32(fi.Length);
            using (var or = fi.OpenRead())
            {
                or.Read(buffer, offset, count);
            }
        }
    }
}
