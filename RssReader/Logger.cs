using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace RssReader
{
    public static class Logger
    {
        private const string LogDirName = "logs";

        public static void Info(string message, [CallerFilePath]string sender = "", [CallerMemberName]string method = "")
        {
            Write(LogLevel.Info, message, $"{Path.GetFileName(sender)} {method}");
        }

        public static void Warning(string message, [CallerFilePath]string sender = "", [CallerMemberName]string method = "")
        {
            Write(LogLevel.Warning, message, $"{Path.GetFileName(sender)} {method}");
        }

        public static void Error(Exception ex, string addMessage, [CallerFilePath]string sender = "", [CallerMemberName]string method = "")
        {
            Write(LogLevel.Error, $"{addMessage} [{ex.Message}]", $"{Path.GetFileName(sender)} {method}");
        }

        private static void Write(LogLevel level, string message, string sender)
        {
            try
            {
                if (!Directory.Exists(LogDirName))
                    Directory.CreateDirectory(LogDirName);

                var filename = Path.Combine(LogDirName, $"{DateTime.Now:d}.log");
                var logText = $"{DateTime.Now:G} | {level} | {sender} | {message}\r\n";

                File.AppendAllText(filename, logText);
            }
            catch
            {
                // Перехватываем все и ничего не делаем
            }
        }
    }

    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }
}