using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using VNC.dbvn;

namespace DBVN_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            #region old
            //var db = DBTable.CreateTable(
            //    new DBColumn("user", false),
            //    new DBColumn("password", true),
            //    new DBColumn("permissions", true),
            //    new DBColumn("reg_date", false, ColumnDateType.DateTime)
            //    );

            //db.Insert("Главный админ", "1511", "all", "2009-12-01 18:55:55");
            //db.Insert("Модератор", "qwerty", "pri", "2011-12-01 09:55:55");
            //db.Insert("Админ", "1234", "kick", "2016-12-01 12:49:55");
            //db.Insert("ВИП", null, "fly");
            //db.Insert("Пользователь", null, null);

            //for (int i = 0; i < 996; i++)
            //{
            //    db.Insert("Пользователь" + i, (i * 2).ToString() + (i * 3).ToString(), "fiugheirhgieru eriugehirugehi eirugheiruh");
            //}

            //DataBaseBinaryReader dbbr = new DataBaseBinaryReader("test.table");
            //dbbr.Load(db.Parse(), db.Increment);
            //dbbr.Write();
            #endregion

            var old_t = DateTime.Now;
            var dbr = new DataBaseBinaryReader("test.table");
            dbr.LoadFile();
            (int increment, string[][] data) = dbr.Parse();

            var db = new DBTable(data, increment);

            var time = DateTime.Now - old_t;
            Console.WriteLine(time.TotalMilliseconds);
            Console.WriteLine(db.ToString());
            Console.ReadKey();
        }
    }
}
