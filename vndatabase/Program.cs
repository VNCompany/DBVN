using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VNC.dbvn
{
    class Program
    {
        static void Main(string[] args)
        {
            #region check_args
            if (args.Length != 4 || args[0] != "-a" || args[2] != "-n")
            {
                Console.WriteLine("Правильный запуск: vndatabase.exe -a [AUTH_CODE] -n [DB_NAME]");
                return;
            }
            if (string.IsNullOrWhiteSpace(args[3]))
            {
                Console.WriteLine("Правильный запуск: vndatabase.exe -a [AUTH_CODE] -n [DB_NAME]");
                return;
            }
            if (!ushort.TryParse(args[1], out ushort auth_code))
            {
                Console.WriteLine("Неверный формат кода входа AUTH_CODE.");
                return;
            }
            string db_name = args[3];
            #endregion

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("+--------------------------------------------------------------------------+");
            Console.WriteLine("+         Приложение для работы с базой данных DBVN. (c) VNCompany         +");
            Console.WriteLine("+--------------------------------------------------------------------------+");
            Console.ResetColor();

            Console.WriteLine();

            Console.Write("dbvn> ");

            string cmd;
            while((cmd = Console.ReadLine()) != "exit;")
            {
                try
                {
                    DoWork(cmd);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: " + ex.GetType().Name + ". " + ex.Message);
                    Console.ResetColor();
                }
                finally
                {
                    Console.Write("dbvn> ");
                }
            }
        }

        static void DoWork(string cmd)
        {

        }

        static void DrawTable(string[][] table)
        {
            const int MAX_WIDTH = 40;

            int[] widths = new int[table[0].Length];

            for (int i = 0; i < widths.Length; i++)
            {
                int max = table.Max(t => t[i].Length);
                widths[i] = max <= MAX_WIDTH - 2 ? max : MAX_WIDTH;
            }

            string border = "+" + string.Join("+", widths.Select(t => "-".JoinStr(t + 2))) + "+";

            Console.WriteLine(border);
            for (int i = 0; i < table.Length; i++)
            {
                string val = "|";

                for (int j = 0; j < table[i].Length; j++)
                {
                    val += WForMain.aSym(widths[j] + 2, table[i][j], ' ') + "|";
                }
                Console.WriteLine(val);
                Console.WriteLine(border);
            }
        }
    }

    public static class WForMain
    {
        public static string JoinStr(this string value, int count)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                result.Append(value);
            }
            return result.ToString();
        }

        public static string aSym(int length, string text, char sym)
        {
            text = " " + text + " ";

            if (text.Length == length) return text;
            else if (text.Length > length)
            {
                try
                {
                    return text.Substring(0, length - 4) + "... ";
                }
                catch (ArgumentOutOfRangeException)
                {
                    return text;
                }
            }
            int a_count = length - text.Length;

            return text + sym.ToString().JoinStr(a_count);
        }
    }
}
