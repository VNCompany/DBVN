using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections;
using System.IO;

namespace VNC.dbvn
{
    public class VConfig
    {
        List<VCSection> sections;
        FileInfo file;

        public static VConfig Parse(string[] lines, string file)
        {
            var r_section = new Regex(@"^\[([A-Za-z0-9_]+)\]$");
            var r_item = new Regex(@"^([A-Za-z0-9_]+)=([^=]*)$");

            var cfg = new VConfig(new VCSection[0], new FileInfo(file));

            string section = "Default";
            cfg.Add(section);
            foreach (string line in lines)
            {
                Match m = r_section.Match(line);
                if (m.Success)
                {
                    section = m.Groups[1].Value;
                    if (!cfg.SectionExist(section))
                        cfg.Add(section);
                }else if ((m = r_item.Match(line)).Success)
                {
                    cfg.GetSection(section).Add(m.Groups[1].Value, m.Groups[2].Value);
                }
            }

            return cfg;
        }

        public static VConfig Create(string file)
        {
            var cfg = new VConfig(new VCSection[0], new FileInfo(file));
            cfg.Add("Default");
            return cfg;
        }

        private VConfig(VCSection[] sections, FileInfo fn)
        {
            this.sections = sections.ToList();
            this.sections.ForEach((item) => item.SectionDeleted += SectionDeleted);
            file = fn;
        }

        private void SectionDeleted(string section)
        {
            sections.RemoveAll(t => t.Name == section);
        }

        public bool SectionExist(string section)
        {
            return sections.Where(t => t.Name == section).Count() > 0;
        }

        public VCSection[] GetSections()
        {
            return sections.ToArray();
        }

        public VCSection GetSection(string section="Default")
        {
            return sections.FirstOrDefault(t => t.Name == section);
        }

        public VCSection Add(string section_name)
        {
            if(SectionExist(section_name))
                throw new InvalidOperationException("Секция " + section_name + " уже существует.");

            VCSection sn = new VCSection(new VCItem[0], section_name);
            sn.SectionDeleted += SectionDeleted;
            sections.Add(sn);
            return sn;
        }

        public void Save()
        {
            using(StreamWriter sw = new StreamWriter(file.FullName))
            {
                foreach(var section in sections)
                {
                    sw.WriteLine("[" + section.Name + "]");
                    foreach(var item in section)
                    {
                        sw.WriteLine(item.Name + "=" + item.Value.ToString());
                    }
                }
            }
        }
    }

    public class VCSection : IEnumerable<VCItem>
    {
        public event Action<string> SectionDeleted;

        List<VCItem> items;

        public string Name { get; set; }

        public VCSection(VCItem[] vcis, string name)
        {
            Name = name;
            items = vcis.ToList();

            items.ForEach((item) => item.VCItemDeleted += ItemDeleted);
        }

        public bool ItemExists(VCItem item)
        {
            return items.Where(t => t.Name == item.Name).Count() > 0;
        }

        public bool ItemExists(string item)
        {
            return items.Where(t => t.Name == item).Count() > 0;
        }

        public VCItem this[string item]
        {
            get
            {
                return items.FirstOrDefault(t => t.Name == item);
            }
        }

        public void Add(string name, object value)
        {
            if (ItemExists(name))
                throw new InvalidOperationException("Элемент " + name + " уже существует.");

            var item = new VCItem(name, value.ToString());
            item.VCItemDeleted += ItemDeleted;

            items.Add(item);
        }

        public void Delete()
        {
            SectionDeleted?.Invoke(this.Name);
        }

        private void ItemDeleted(string item)
        {
            items.RemoveAll(t => t.Name == item);
        }

        public IEnumerator<VCItem> GetEnumerator()
        {
            return items.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class VCItem
    {
        public event Action<string> VCItemDeleted;

        string item;
        string value;

        public string Name => item;

        public object Value
        {
            get
            {
                if (value.Split('.').Length == 2 && double.TryParse(value, out double d_result))
                    return d_result;
                else if (int.TryParse(value, out int i_result))
                    return i_result;
                else if (bool.TryParse(value, out bool b_result))
                    return b_result;
                else
                    return value;
            }
        }

        public void SetValue(object value)
        {
            this.value = value == null ? null : value.ToString();
        }

        public void Delete()
        {
            VCItemDeleted?.Invoke(this.item);
        }

        public VCItem(string item, string value)
        {
            this.item = item;
            this.value = value;
        }
    }
}
