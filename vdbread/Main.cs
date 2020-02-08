using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace VNC.dbvn
{
    public class DataBaseBinaryReader : IDisposable
    {
        FileInfo fi;
        byte[] data;

        public int Length
        {
            get { return data.Length; }
        }

        public DataBaseBinaryReader(string file)
        {
            fi = new FileInfo(file);
        }

        public void LoadFile()
        {
            if (!fi.Exists)
                throw new FileNotFoundException("Файл " + fi.Name + " не найден");
            else
            {
                data = new byte[fi.Length];
                using (var fileOpen = fi.OpenRead())
                {
                    fileOpen.Read(data, 0, data.Length);
                }
            }
        }

        public (int, string[][]) Parse()
        {
            List<string[]> result = new List<string[]>();

            if (data.Length == 0)
                throw new NullReferenceException("Данные не загружены.");

            string set_type = "";
            string data_type = "";
            string sign = "";

            string increment_string = "";

            List<string> string_buffer = new List<string>();
            List<byte> buffer = new List<byte>();

            for (int i = 0; i < data.Length; i++)
            {
                byte value = data[i];

                switch (value)
                {
                    case (byte)BCodes.Request:
                        set_type = "req";
                        break;

                    case (byte)BCodes.HeaderStart:
                        data_type = "string";
                        break;

                    case (byte)BCodes.HeaderEnd:
                        data_type = "";

                        if (set_type == "req" || set_type == "table")
                            result.Add(string_buffer.ToArray());
                        else if (set_type == "set")
                            increment_string = string_buffer[0];
                        string_buffer.Clear();
                        break;

                    case (byte)BCodes.TextStart:
                        sign = "text";
                        break;

                    case (byte)BCodes.TextEnd:
                        sign = "";
                        if (set_type == "req" || set_type == "set")
                            string_buffer.Add(Encoding.ASCII.GetString(buffer.ToArray()));
                        else if (set_type == "table")
                            string_buffer.Add(Encoding.UTF8.GetString(buffer.ToArray()));
                        buffer.Clear();
                        break;

                    case (byte)BCodes.Bel:
                        set_type = "set";
                        break;

                    case (byte)BCodes.Confirmation:
                        set_type = "table";
                        break;

                    default:
                        if (sign == "text" && data_type == "string")
                        {
                            buffer.Add(value);
                        }
                        break;
                }
            }

            int increment = Convert.ToInt32(increment_string.Split('=')[1]);

            return (increment, result.ToArray());
        }

        public void Load(string[][] dt, int increment)
        {
            List<byte> d = new List<byte>();
            d.AddRange(new byte[] { 0, 0, 0, 169, 154 });
            d.Add((byte)BCodes.Request);
            d.Add((byte)BCodes.HeaderStart);
            foreach(string header in dt[0])
            {
                d.Add((byte)BCodes.TextStart);
                d.AddRange(Encoding.ASCII.GetBytes(header));
                d.Add((byte)BCodes.TextEnd);
            }
            d.AddRange(new byte[] {
                (byte)BCodes.HeaderEnd,
                (byte)BCodes.NewLine,
                (byte)BCodes.Bel,
                (byte)BCodes.HeaderStart,
                (byte)BCodes.TextStart
            });
            d.AddRange(Encoding.UTF8.GetBytes("Increment=" + increment.ToString()));
            d.AddRange(new byte[] {
                (byte)BCodes.TextEnd,
                (byte)BCodes.HeaderEnd,
                (byte)BCodes.Confirmation,
                (byte)BCodes.NewLine
            });

            for (int i = 1; i < dt.Length; i++)
            {
                d.Add((byte)BCodes.HeaderStart);

                for (int j = 0; j < dt[i].Length; j++)
                {
                    d.Add((byte)BCodes.TextStart);
                    d.AddRange(Encoding.UTF8.GetBytes(dt[i][j]));
                    d.Add((byte)BCodes.TextEnd);
                }

                d.Add((byte)BCodes.HeaderEnd);
                d.Add((byte)BCodes.NewLine);
            }

            data = d.ToArray();
        }

        public void Write()
        {
            using (FileStream fs = new FileStream(fi.FullName, FileMode.Create, FileAccess.Write))
            {
                fs.Write(data, 0, data.Length);
            }
        }

        public void Dispose()
        {
            data = null;
            fi = null;
        }

        public void ReadBytes(byte[] buffer, int offset, int count=-1)
        {
            if (count == -1) count = data.Length;

            for (int i = offset; i < count; i++)
            {
                buffer[i] = data[i];
            }
        }

        private static bool CheckValidRow(string text)
        {
            return Regex.IsMatch(text, @"^([a-zA-Z]+?[0-9_]*)$");
        }
    }
}
