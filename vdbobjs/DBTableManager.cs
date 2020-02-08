using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VNC.dbvn
{
    public class DBTableManager
    {
        public event Action<DBTable> MTableSaved;
        public event Action<DBTable> MTableDeleted;

        DirectoryInfo di;
        Func<string, (int, string[][])> generator;
        VS src;
        public DBTableManager(string path, VS source, Func<string, (int, string[][])> gen)
        {
            if (!Directory.Exists(path)) throw new InvalidOperationException("База данных повреждена.");

            di = new DirectoryInfo(path);
            generator = gen;
            src = source;
        }

        public string[] GetTableNames()
        {
            List<string> fn = new List<string>();
            foreach(FileInfo f in di.GetFiles("*.table"))
            {
                fn.Add(f.Name.Replace(".table", ""));
            }
            return fn.ToArray();
        }

        public int TablesCount()
        {
            return GetTableNames().Length;
        }

        private void TableSaved(DBTable table)
        {
            MTableSaved?.Invoke(table);
        }

        private void TableDeleted(DBTable table)
        {
            MTableDeleted?.Invoke(table);
        }

        public DBTable CreateTable(string name, params DBColumn[] columns)
        {
            if (GetTableNames().Contains(name)) throw new VDataBaseException("Данная таблица уже существует.", "DBTableManager", 0xdf1);
            if (!VS.CheckValidateFileName(name)) throw new ArgumentException("Имя таблицы содержит недопустимые символы.", "CreateTable");
            var table = DBTable.CreateTable(name, src, columns);
            table.TableDeleted += TableDeleted;
            table.TableSaved += TableSaved;
            return table;
        }

        public DBTable CreateTableIfNotExists(string name, params DBColumn[] columns)
        {
            if (GetTableNames().Contains(name))
                return OpenTable(name);
            else
                return CreateTable(name, columns);
        }

        public DBTable OpenTable(string name)
        {
            if (!GetTableNames().Contains(name)) throw new VDataBaseException("Данная таблица не существует.", "DBTableManager", 0xdf);

            (int increment, string[][] data) = generator(Path.Combine(di.FullName, name + ".table"));

            var table = new DBTable(data, increment, name, src);
            table.TableDeleted += TableDeleted;
            table.TableSaved += TableSaved;
            return table;
        }
    }
}
