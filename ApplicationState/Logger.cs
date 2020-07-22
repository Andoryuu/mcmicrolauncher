using System;
using System.IO;

namespace MCMicroLauncher.ApplicationState
{
    internal static class Log
    {
        private const string LogFilePath = "./launcher.log";

        static Log()
        => File.Delete(LogFilePath);

        internal static void Error(string msg, Exception ex)
        => Internal("Error", msg, ex.Message, ex.StackTrace);

        internal static void Error(string msg)
        => Internal("Error", msg);

        internal static void Info(string msg, params string[] moreMsgs)
        => Internal("Info", msg, moreMsgs);

        internal static void Warn(string msg, params string[] moreMsgs)
        => Internal("Warn", msg, moreMsgs);

        private static void Internal(
            string type,
            string msg,
            params string[] moreMsgs)
        {
            var log = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} | {type} | {msg}";

            if (moreMsgs?.Length > 0)
            {
                log += " | " + string.Join(" | ", moreMsgs);
            }

            log += "\n";

            File.AppendAllText(LogFilePath, log);
        }
    }
}
