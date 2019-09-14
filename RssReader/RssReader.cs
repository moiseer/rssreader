using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;

namespace RssReader
{
    public class RssReader
    {
        private const string SettingsFileName = "settings.json";
        private const string ChannelsDirName = "channels";
        private const string CommandsString = "pull - Читает RSS ленты из файла настроек, скачивает их и сохраняет на локальный диск.\r\n" +
                                              "       Также скачивает указанные через пробел RSS ленты.\r\n" +
                                              "list [filter key_words]- Читает скачанные локально RSS ленты и отображает их элементы.\r\n" +
                                              "       Если указан фильтр отображает только RSS ленты, содержащие в названии ключевые слова.\r\n" +
                                              "remove - Удаляет все скачанные RSS ленты.\r\n" +
                                              "add - Добавляет в файл настроек RSS ленты указанные через пробел.\r\n" +
                                              "help - Список доступных команд.\r\n" +
                                              "exit - Выйти из программы.";

        public void Run()
        {
            var run = true;
            while (run)
            {
                Console.Write(">");
                var input = Console.ReadLine();
                Logger.Info($"Input: {input}");

                var command = input?.ToLower().Split(' ');
                switch (command?[0])
                {
                    case "pull":
                        Pull();
                        break;
                    case "list":
                        List(command);
                        break;
                    case "remove":
                        Remove();
                        break;
                    case "exit":
                        run = false;
                        break;
                    case "add":
                        AddChannels(command);
                        break;
                    case "backup":
                        Backup(command);
                        break;
                    case "help":
                        Console.WriteLine("Доступные команды:\r\n" + CommandsString);
                        break;
                    default:
                        UnknownCommand();
                        break;
                }
            }
        }

        

        public void StartMessage()
        {
            const string message = "RSS Reader\r\n" + CommandsString;
                                   
            Console.WriteLine(message);
            BackupManager.ShowCommands();
        }

        private static void Pull()
        {
            int loaded = 0;
            int errors = 0;

            var settings = LoadSettings();
            
            Console.WriteLine("Скачивание RSS лент...");
            Logger.Info("Скачивание RSS лент...");

            foreach (var url in settings.Channels)
            {
                if (LoadChannel(url))
                    loaded++;
                else
                    errors++;
            }

            Console.WriteLine(((loaded > 0) ? $"Успешно скачано RSS лент: {loaded}. " : "") +
                              ((errors > 0) ? $"Не удалось скачать RSS лент: {errors}." : ""));
            if (loaded>0) Logger.Info($"Успешно скачано RSS лент: {loaded}. ");
            if (errors>0) Logger.Warning($"Не удалось скачать RSS лент: {errors}.");
        }

        private static void List(string[] command)
        {
            if (!Directory.Exists(ChannelsDirName) ||
                Directory.GetFiles(ChannelsDirName).Length==0)
            {
                Console.WriteLine("Нет скачанных RSS лент");
                Logger.Info("Нет скачанных RSS лент");
                return;
            }

            string filter = "";
            if (command.Length > 1)
                switch (command[1])
                {
                    case "filter":
                        if (command.Length >= 3)
                        {
                            for (int i = 2;i<command.Length-1;i++)
                                filter+=$"{command[i]} ";
                            filter+=command.Last();
                        }
                        else
                            UnknownCommand();
                        break;
                    default:
                        UnknownCommand();
                        break;
                }

            Console.WriteLine("Список скачанных RSS лент:");

            var files = Directory.GetFiles(ChannelsDirName).ToList();

            if (filter != "")
            {
                Console.WriteLine($"Фильтр: \"{filter}\"");
                for (int i = 0; i < files.Count; i++)
                {
                    var channelName = Path.GetFileNameWithoutExtension(files[i]).ToLower();
                    if (!channelName.Contains(filter))
                    {
                        files.Remove(files[i]);
                        i--;
                    }
                }
            }

            foreach (var file in files)
            {
                try
                {
                    using (var reader = XmlReader.Create(file))
                    {
                        SyndicationFeed channel = SyndicationFeed.Load(reader);
                        Console.WriteLine($"\nКанал {channel.Title.Text}");
                        foreach (var item in channel.Items)
                        {
                            Console.WriteLine($"{item.PublishDate,-16:g} | {item.Title.Text} | {item.Links[0].Uri}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Не удалось прочитать файл {file}! " + ex.Message);
                    Logger.Error(ex, $"Не удалось прочитать файл {file}!");
                }
            }
        }

        private static void Remove()
        {
            Console.WriteLine("Удаление всех скачанных RSS лент...");

            try
            {
                Directory.Delete(ChannelsDirName, true);
                Console.WriteLine("Удаление завершено.");
                Logger.Info("Удаление скачанных RSS лент завершено.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("При удалении скачанных RSS лент возникла ошибка! " + ex.Message);
                Logger.Error(ex, "При удалении скачанных RSS лент возникла ошибка!");
            }
        }

        private static void AddChannels(string[] command)
        {
            var isNeedSave = false;

            var settings = LoadSettings();

            foreach (var url in command)
            {
                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) continue;
                if (settings.Channels.Contains(url)) continue;

                settings.Channels.Add(url);
                isNeedSave = true;

                Logger.Info($"Добавлена RSS лента {url}");
            }

            if(isNeedSave)
                SaveSettings(settings);
        }

        private static void UnknownCommand()
        {
            Console.WriteLine("Введена неизвестная команда.\r\nhelp - список доступных команд");
            
            Logger.Warning("Введена неизвестная команда");
        }

        private static bool LoadChannel(string url)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) return false;
            try
            {
                using (var reader = XmlReader.Create(url))
                {
                    var channel = SyndicationFeed.Load(reader);

                    if (!Directory.Exists(ChannelsDirName))
                        Directory.CreateDirectory(ChannelsDirName);

                    string path = Path.Combine(ChannelsDirName, $"{channel.Title.Text}.xml");

                    using (var file = File.OpenWrite(path))
                    using (var writer = new XmlTextWriter(file, null))
                    {
                        writer.Formatting = System.Xml.Formatting.Indented;
                        channel.SaveAsRss20(writer);
                    }
                }
                Console.WriteLine($"Канал {url} загружен");
                Logger.Info($"Канал {url} загружен");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось загрузить канал {url}! " + ex.Message);
                Logger.Error(ex, $"Не удалось загрузить канал {url}!");
                return false;
            }
        }

        private static Settings LoadSettings()
        {
            var settings = new Settings();
            try
            {
                if (File.Exists(SettingsFileName))
                {
                    using (var reader = new StreamReader(SettingsFileName))
                    {
                        settings = JsonConvert.DeserializeObject<Settings>(reader.ReadToEnd());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Не удалось прочитать файл настроек программы! " + ex.Message);
                Logger.Error(ex, "Не удалось прочитать файл настроек программы!");
            }

            return settings;
        }

        private static void SaveSettings(Settings settings)
        {
            try
            {
                var jsonSettings = JsonConvert.SerializeObject(settings, Newtonsoft.Json.Formatting.Indented);

                using (var writer = new StreamWriter(SettingsFileName))
                {
                    writer.Write(jsonSettings);
                }
                Logger.Info("Файл настроек успешно сохранен");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Не удалось сохранить файл настроек программы! " + ex.Message);
                Logger.Error(ex, "Не удалось сохранить файл настроек программы!");
            }
        }

        private static void Backup(string[] command)
        {
            if (command.Length == 1)
            {
                BackupManager.ShowCommands();
            }

            switch (command[1])
            {
                case "create":
                    BackupManager.CreateBackup();
                    break;
                case "restore":
                    string backup = "";
                    if (command.Length == 3)
                        backup = command[2];
                    BackupManager.RestoreBackup(backup);
                    break;
                case "list":
                    var backups = BackupManager.GetBackupFileNames();
                    foreach (var backupName in backups)
                    {
                        Console.WriteLine(backupName);
                    }
                    break;
                case "remove":
                    BackupManager.Remove();
                    break;
                default:
                    BackupManager.ShowCommands();
                    break;
            }
        }
    }
}
