using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace VNC.dbvn
{
    public enum ColumnDateType
    {
        None = 0,
        Date = 1,
        DateTime = 2
    }

    public class DBTable
    {
        public event Action<DBTable> TableSaved;
        public event Action<DBTable> TableDeleted;

        public static DBTable CreateTable(string name, VS source, params DBColumn[] columns)
        {
            List<DBColumn> cc = new List<DBColumn>();
            cc.Add(new DBColumn("id", false, ColumnDateType.None));
            foreach(var column in columns)
            {
                if(cc.Select(t => t.Name).Contains(column.Name))
                    throw new VDataBaseException("Не допускается хранение стобцов с одинаковым именем. Ошибка парсинга", "vdbobjs", 0xa);
                cc.Add(column);
            }
            return new DBTable(new string[][] { cc.Select(t => t.ToString()).ToArray() }, 1, name, source);
        }

        bool err = false;
        public bool Error
        {
            get
            {
                return err;
            }
        }

        public string Name { get; private set; }

        List<DBColumn> cols = new List<DBColumn>();
        List<DBRow> rows = new List<DBRow>();

        VS src;

        public int Increment { get; private set; }

        public DBTable(string[][] @base, int increment, string name, VS source)
        {
            Name = name;

            cols.Clear();
            rows.Clear();

            src = source;

            if(increment < 1) throw new VDataBaseException("Значение инкремента неверно. Ошибка парсинга", "vdbobjs", 0x2);
            if(@base.Length < 1 || @base[0].Length < 2) throw new VDataBaseException("Неверный формат таблицы. Ошибка парсинга", "vdbobjs", 0x1);

            try
            {
                Increment = increment;

                for (int i = 0; i < @base[0].Length; i++)
                {
                    cols.Add(new DBColumn(@base[0][i]) { ID = i });
                }

                for (int r = 1; r < @base.Length; r++)
                {
                    DBRow dbr;
                    (string, DBColumn)[] cells = new (string, DBColumn)[@base[r].Length];
                    for (int c = 0; c < @base[r].Length; c++)
                    {
                        if (@base[r][c] == "NULL")
                            @base[r][c] = null;
                        cells[c] = (@base[r][c], cols[c]);
                    }
                    dbr = new DBRow(cells, src);
                    dbr.ElementDeleted += RowDeleted;
                    rows.Add(dbr);
                }

            }
            catch (Exception ex)
            {
                err = true;
                throw new VDataBaseException("Неверный формат таблицы. Ошибка парсинга. \r\n Сообщение: " + ex.Message, "vdbobjs", 0x1);
            }
        }
        
        public static string GetNowDate(ColumnDateType dateType)
        {
            var now = DateTime.Now;
            switch (dateType)
            {
                case ColumnDateType.Date:
                    return now.ToString("yyyy-MM-dd");

                case ColumnDateType.DateTime:
                    return now.ToString("yyyy-MM-dd HH:mm:ss");

                default:
                    return null;
            }
        }

        public void Insert(params (int, object)[] values)
        {
            string[] row = new string[cols.Count];

            foreach (var t_value in values)
            {
                if (t_value.Item1 < 0 || t_value.Item1 >= cols.Count - 1)
                    throw new IndexOutOfRangeException("Номер столбца выходит за пределы таблицы");

                int i = t_value.Item1 + 1;

                string value = t_value.Item2 == null ? null : t_value.Item2.ToString();
                
                if (string.IsNullOrEmpty(value))
                    value = null;
                if (value == "NULL") value = null;

                row[i] = value;
            }

            row[0] = Increment.ToString();
            if (rows.Select(t => t.ID).Contains(Increment))
                throw new VDataBaseException("Елемент с таким же id уже существует", "objects");

            var result = new (string, DBColumn)[row.Length];
            for (int i = 0; i < row.Length; i++)
            {
                if (cols[i].DateType != ColumnDateType.None && row[i] == null)
                    row[i] = GetNowDate(cols[i].DateType);

                if (cols[i].Nullable && row[i] == null)
                    row[i] = "NULL";

                if (row[i] == null && !cols[i].Nullable)
                    throw new ArgumentNullException("ROW[" + i.ToString() + "]", i.ToString() + ": Столбец " + cols[i].Name + " не может быть NULL.");

                result[i] = (row[i], cols[i]);
            }

            DBRow dbr = new DBRow(result, src);
            dbr.ElementDeleted += RowDeleted;
            rows.Add(dbr);
            Increment++;
        }

        public void Insert(params (string, object)[] values)
        {
            var result = new (int, object)[values.Length];

            for (int i = 0; i < result.Length; i++)
            {
                DBColumn col = cols.FirstOrDefault(t => t.Name == values[i].Item1);
                if (col == null) throw new VDataBaseException("Столбец " + values[i].Item1 + " не существует в этой таблице", "object");

                result[i] = (col.ID - 1, values[i].Item2);
            }

            Insert(result);
        }

        public void Insert(params object[] values)
        {
            var result = new (int, object)[cols.Count - 1];

            for (int i = 0; i < result.Length; i++)
            {
                if (i < values.Length)
                    result[i] = (i, values[i]);
                else
                    result[i] = (i, null);
            }

            Insert(result);
        }

        public string[][] Parse()
        {
            List<string[]> data = new List<string[]>();

            data.Add((from t in cols select t.ToString()).ToArray());

            foreach(DBRow dbr in rows)
            {
                data.Add((from t in dbr.GetCells() select (t.ValueObject == null || t.ValueObject.ToString() == "" ? "NULL" : t.ValueObject.ToString())).ToArray());
            }

            return data.ToArray();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[");
            foreach(string[] val in Parse())
            {
                sb.AppendLine(" [" + string.Join(", ", val) + "]");
            }
            sb.AppendLine("]");
            return sb.ToString();
        }

        public void DeleteRow(int id)
        {
            rows.RemoveAll(t => t.ID == id);
        }

        public void ForEach(Action<DBRow> action)
        {
            foreach (var row in rows)
                action.Invoke(row);
        }

        private void RowDeleted(DBRow row)
        {
            DeleteRow(row.ID);
        }

        public DBRowCollection Where(Func<DBRow, bool> predicate)
        {
            return new DBRowCollection(rows.Where(predicate));
        }

        public DBRow this[int id]
        {
            get
            {
                return rows.FirstOrDefault(t => t.ID == id);
            }
        }

        public DBRowCollection Rows
        {
            get
            {
                return new DBRowCollection(rows);
            }
        }

        public void Clear()
        {
            rows.Clear();
            Increment = 0;
        }

        public void Save()
        {
            TableSaved?.Invoke(this);
        }

        public void Delete()
        {
            TableDeleted?.Invoke(this);
        }
    }

    public class DBRowCollection : IEnumerable<DBRow>
    {
        List<DBRow> rows;

        public DBRowCollection(IEnumerable<DBRow> rows)
        {
            this.rows = rows.ToList();
        }

        public DBRow this[int id]
        {
            get
            {
                return rows.FirstOrDefault(t => t.ID == id);
            }
        }

        public void DeleteAll()
        {
            rows.ForEach(t => t.Delete());
        }

        public void ForEach(Action<DBRow> action)
        {
            foreach (var row in rows)
                action.Invoke(row);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<DBRow> GetEnumerator()
        {
            return rows.GetEnumerator();
        }
    }

    public class DBColumn
    {
        bool id_changed = false;
        int id;

        public string Name { get; set; }
        public bool Nullable { get; set; }
        public ColumnDateType DateType { get; set; }

        public int ID
        {
            get { return id; }
            set
            {
                if (!id_changed)
                {
                    id = value;
                    id_changed = true;
                }
            }
        }

        public DBColumn(string column)
        {
            var re = new Regex(@"^([a-zA-Z]+?[a-zA-Z0-9_]*)\((([a-zA-Z0-9_]*(,|))*?)\)$");
            var match = re.Match(column);
            if (match.Success)
            {
                Name = match.Groups[1].Value;
                string[] args = match.Groups[2].Value.Split(',');
                switch (args.Length)
                {
                    case 1:
                        Nullable = args[0] == "NULL";
                        break;
                    case 2:
                        Nullable = args[0] == "NULL";
                        if (args[1] == "date")
                            DateType = ColumnDateType.Date;
                        else if (args[1] == "datetime")
                            DateType = ColumnDateType.DateTime;
                        else
                            DateType = ColumnDateType.None;
                        break;
                    default:
                        Nullable = true;
                        DateType = ColumnDateType.None;
                        break;
                }
            }
            else throw new VDataBaseException("Неверный формат заголовка столбца. Ошибка парсинга. " + column, "vdbobjs", 0x1);
        }

        public DBColumn(string name, bool nullable, ColumnDateType dateType=ColumnDateType.None)
        {
            Name = name;
            Nullable = nullable;
            DateType = dateType;
        }

        public override string ToString()
        {
            string dt = DateType.ToString().ToLower();
            if (dt == "none")
                dt = "";
            return Name + "(" + (Nullable ? "NULL" : "NOT_NULL") + "," + dt + ")";
        }
    }

    public class DBRow
    {
        public event Action<DBRow> ElementDeleted;

        List<DBCell> dbcells = new List<DBCell>();
        public int ID { get; private set; }

        public DBRow((string, DBColumn)[] cells, VS src)
        {
            dbcells.AddRange(from t in cells select new DBCell(t.Item1, t.Item2, this, src));
            ID = (int)dbcells[0].ValueObject;
        }

        public DBCell[] GetCells()
        {
            return dbcells.ToArray();
        }

        public DBCell this[string column]
        {
            get
            {
                return dbcells.FirstOrDefault(t => t.Column.Name == column);
            }
        }

        public DBCell this[int col_id]
        {
            get
            {
                return dbcells.FirstOrDefault(t => t.Column.ID == col_id);
            }
        }

        public void Delete()
        {
            ElementDeleted?.Invoke(this);
        }
    }

    public class DBCell
    {
        string value;

        VS src;

        public object ValueObject
        {
            get
            {
                if (int.TryParse(value, out int val))
                    return val;
                else if (bool.TryParse(value, out bool b_val))
                    return b_val;
                else
                    return value;
            }
        }

        public double ValueDouble
        {
            get
            {
                if (double.TryParse(value, out double res))
                    return res;
                else
                    return -1;
            }
        }

        public void SetValue(object value)
        {
            if (Column.Name != "id")
            {
                if (value == null)
                {
                    if (Column.Nullable)
                        this.value = null;
                }
                else
                    this.value = value.ToString();
            }
        }

        public DBRow Row { get; private set; }
        public DBColumn Column { get; private set; }

        public int iRow { get; private set; }
        public int iColumn { get; private set; }

        public DBCell(object element, DBColumn column, DBRow row, VS source)
        {
            value = element == null ? null : element.ToString();
            Column = column;
            Row = row;
        }

        public DBCell(object element, int column, int row, VS source)
        {
            value = element.ToString();
            iRow = row;
            iColumn = column;
        }
    }
}
