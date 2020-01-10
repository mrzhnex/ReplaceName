using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ReplaceName
{
    class Program
    {
        static readonly string version = "4.0.1";
        static void Main(string[] args)
        {
            string Output = "";
            Console.OutputEncoding = Encoding.GetEncoding("Windows-1251");
            Console.WriteLine("Логирование началось: ");
            Output = Output + "Логирование началось: " + "\n";
            Console.WriteLine("Версия программы: '" + version + "'");
            Output = Output + "Версия программы: '" + version + "'" + "\n";
            XmlSerializer formatter = new XmlSerializer(typeof(Settings));
            Settings setting = new Settings(true, true);
            if (!File.Exists("settings.xml"))
            {
                using (FileStream fs = new FileStream(Global.SettingsFile, FileMode.OpenOrCreate))
                {
                    formatter.Serialize(fs, setting);
                    Console.WriteLine("Файл настроек создан" + "\n");
                    Output = Output + "Файл настроек создан" + "\n";
                }
            }
            Console.Write("Введите путь от исполняемого файла программы до папки с .fb2 файлами, требующими изменения: ");
            string folder = Console.ReadLine();
            Output = Output + "Введите путь от исполняемого файла программы до папки с .fb2 файлами, требующими изменения: " + folder + "\n";
            Console.WriteLine("\n");
            if (!Directory.Exists(folder))
            {
                Console.WriteLine("Путь '" + folder + "' не существует. Нажмите любую клавишу, чтобы выйти из программы...");
                Console.ReadKey();
                Environment.Exit(0);
            }
            string[] files = Directory.GetFiles(folder);

            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Contains(Global.OldExpansion))
                {

                    if (setting.Output)
                        Console.WriteLine("Чтение данных с файла: " + files[i]);
                    Output = Output + "Чтение данных с файла: " + files[i] + "\n";

                    if (Global.Debug)
                    {
                        Console.WriteLine("directory: '" + directory + "'");
                    }
                    if (!Directory.Exists(@directory))
                    {
                        Console.WriteLine("directory2: '" + directory + "'");
                        Directory.CreateDirectory(@directory);
                    }

                    if (File.Exists(Path.Combine(directory, GetNewName(last, first, middle, book, year))))
                    {
                        if (setting.Output)
                            Console.WriteLine("Файл '" + GetNewName(last, first, middle, book, year) + "' уже существует" + "\n");
                        Output = Output + "Файл '" + GetNewName(last, first, middle, book, year) + "' уже существует" + "\n";
                        continue;
                    }

                    try
                    {
                        if (Path.Combine(directory, GetNewName(last, first, middle, book, year)).Length > Global.MaxLenghtFile)
                        {
                            book = book.Substring(0, book.Length - (Path.Combine(directory, GetNewName(last, first, middle, book, year)).Length - Global.MaxLenghtFile));
                        }
                        File.Move(Path.GetFullPath(files[i]), Path.GetFullPath(Path.Combine(directory, GetNewName(last, first, middle, book, year))));
                    }
                    catch (DirectoryNotFoundException)
                    {
                        if (setting.ErrorOutput)
                            Console.WriteLine("Ошибка переименовывания файла (ошибка директории) '" + files[i] + "'" + "\n");
                        continue;
                    }
                    if (setting.Output)
                        Console.WriteLine("Файл успешно изменен: '" + Path.Combine(directory, GetNewName(last, first, middle, book, year)) + "'\n");
                    Output = Output + "Файл успешно изменен: '" + Path.Combine(directory, GetNewName(last, first, middle, book, year)) + "'\n" + "\n";
                }
            }

            Console.WriteLine("Готово. Консоль может быть закрыта.");
            Output = Output + "Готово. Консоль может быть закрыта." + "\n";
            if (CreateOutput(Output))
            {
                if (setting.Output)
                    Console.WriteLine("Файл логирования создан");
            }
            else
            {
                if (setting.Output)
                    Console.WriteLine("Произошла ошибка при создании файла логирования");
            }
            Console.ReadKey();
        }

        public static string ClearFromWrongSymbols(string source)
        {
            foreach (KeyValuePair<string, string> kvp in Global.WrongAndGoodSymbols)
            {
                if (source.Contains(kvp.Key))
                {
                    source = source.Replace(kvp.Key, kvp.Value);
                    if (Global.Debug)
                        Console.WriteLine("clear: " + kvp.Key);
                }
            }
            foreach (char c in new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()))
            {
                source = source.Replace(c.ToString(), "");
            }
            if (source.Contains(" "))
            {
                if (source.IndexOf(" ") == source.Length - 1)
                {
                    source.Remove(source.Length - 1);
                }
            }
            return source;
        }

        public static string GetNewName(string last, string first, string middle, string book, string year)
        {
            return last + " " + first + " " + middle + Global.symbol + book + " " + Global.left_bracket + year + Global.right_bracket + Global.OldExpansion;
        }

        public static string GetBetween(string all, string first, string last)
        {
            all = all.Remove(0, all.IndexOf(first) + first.Length);
            all = all.Remove(all.IndexOf(last));
            return all.Replace(first, "").Replace(last, "").Replace(" ", string.Empty);
        }

        public static string GetBetweenWithoutRemovedEmpty(string all, string first, string last)
        {
            all = all.Remove(0, all.IndexOf(first) + first.Length);
            all = all.Remove(all.IndexOf(last));
            string temp = all.Replace(first, "").Replace(last, "");
            while (temp.IndexOf(" ") == 0)
            {
                temp = temp.Substring(1);
            }
            if (temp.LastIndexOf(" ") == temp.Length)
            {
                temp = temp.Substring(temp.Length - 1);
            }
            return temp;
        }
        public static bool CreateOutput(string text)
        {
            if (!Directory.Exists(Global.OutputFolder))
            {
                Directory.CreateDirectory(Global.OutputFolder);
            }
            string[] files = Directory.GetFiles(Global.OutputFolder);
            if (files.Length > 0)
            {
                if (int.TryParse(files[files.Length - 1].Remove(files[files.Length - 1].Length - 4).Remove(0, Global.OutputFolder.Length + 1), out int index))
                {
                    index++;
                    StreamWriter sw = new StreamWriter(Path.Combine(@Global.OutputFolder + @"/" + @index + @".txt"));
                    sw.WriteLine(text);
                    sw.Close();
                    return true;
                }
                else
                {
                    Console.WriteLine(files[files.Length - 1].Remove(files[files.Length - 1].Length - 4).Remove(0, Global.OutputFolder.Length + 1));
                    return false;
                }
            }
            else
            {
                StreamWriter sw = new StreamWriter(Path.Combine(@Global.OutputFolder + @"/" + @"1.txt"));
                sw.WriteLine(text);
                sw.Close();
                return true;
            }
        }
    }
    public static class Global
    {
        public static readonly int MaxLenghtFile = 259;
        public static readonly string OutputFolder = "Output";
        public static readonly string SettingsFile = "settings.xml";
        public static readonly bool Debug = false;
        public static readonly string symbol = "=";
        public static readonly string left_bracket = "(";
        public static readonly string right_bracket = ")";
        public static readonly string OldExpansion = ".fb2";

        public static readonly Dictionary<string, string> WrongAndGoodSymbols = new Dictionary<string, string>()
        {
            { ".", "," },
            { "|", "-" },
            { "*", "-" },
            { ":", "," },
            { "?", "," },
            { "!", "," },
            { "%", "проценты" },
            { "/", "-" },
            { @"\", "-" },
            { "\n", "-" }

        };
    }

    [Serializable]
    public class Settings
    {
        public bool ErrorOutput { get; set; }
        public bool Output { get; set; }
        public Settings() { }

        public Settings(bool output, bool errorOuput)
        {
            this.Output = output;
            this.ErrorOutput = errorOuput;
        }
    }
}
