using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace RssReader
{
    class BackupManager
    {
        private const string ChannelsDirName = "logs";
        private const string BackupsDirName = "backups";

        public static void CreateBackup()
        {
            if (!Directory.Exists(ChannelsDirName) ||
                Directory.GetFiles(ChannelsDirName).Length == 0)
            {
                Console.WriteLine("Нет скачанных RSS лент");
                Logger.Info("Нет скачанных RSS лент");
                return;
            }

            try
            {
                if (!Directory.Exists(BackupsDirName))
                    Directory.CreateDirectory(BackupsDirName);

                string backupFileName = $"backup_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.zip";
                string path = Path.Combine(BackupsDirName, backupFileName);
                ZipFile.CreateFromDirectory(ChannelsDirName, path);

                Console.WriteLine($"Создан бэкап {backupFileName}");
                Logger.Info($"Создан бэкап {backupFileName}");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Не удалось создать бэкап! " + ex.Message);
                Logger.Error(ex, "Не удалось создать бэкап!");
            }
        }

        public static void RestoreBackup(string backupFileName)
        {
            string path = Path.Combine(BackupsDirName, backupFileName);
            if (!File.Exists(path))
            {
                Console.WriteLine("Отсутствует файл бэкапа или имя файла введено неверно");
                Logger.Warning("Отсутствует файл бэкапа или имя файла введено неверно");
                return;
            }

            try
            {
                ZipFile.ExtractToDirectory(path, ChannelsDirName);

                Console.WriteLine($"Список RSS лент восстановлен из {backupFileName}");
                Logger.Info($"Список RSS лент восстановлен из {backupFileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось восстановить список RSS лент из {backupFileName}" + ex.Message);
                Logger.Error(ex, $"Не удалось восстановить список RSS лент из {backupFileName}");
            }
        }

        public static string[] GetBackupFileNames()
        {
            string[] backups = new string[0];
            if (!Directory.Exists(BackupsDirName) ||
                Directory.GetFiles(BackupsDirName).Length == 0)
            {
                Console.WriteLine("Нет созданных бэкапов");
                Logger.Warning("Нет созданных бэкапов");
                return backups;
            }

            try
            {
                backups = Directory.GetFiles(BackupsDirName);
                for (int i = 0; i < backups.Length; i++)
                {
                    backups[i] = Path.GetFileName(backups[i]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Не удалось получить список бэкапов! " + ex.Message);
                Logger.Error(ex, "Не удалось получить список бэкапов!");
            }

            return backups;
        }

        public static void Remove()
        {
            Console.WriteLine("Удаление всех созданных бэкапов...");

            try
            {
                Directory.Delete(BackupsDirName, true);
                Console.WriteLine("Удаление завершено.");
                Logger.Info("Удаление бэкапов завершено.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("При удалении бэкапов возникла ошибка! " + ex.Message);
                Logger.Error(ex, "При удалении бэкапов возникла ошибка!");
            }
        }

        public static void ShowCommands()
        {
            const string message = "backup create - Создать бэкап сохраненных RSS лент.\r\n" +
                                   "backup list - Показать список созданных бэкапов.\r\n" +
                                   "backup restore - Восстановить RSS из бэкапа.\r\n" +
                                   "backup remove - Восстановить RSS из бэкапа.\r\n";
            Console.WriteLine(message);
        }
    }
}
