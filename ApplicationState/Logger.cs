using System;
using System.IO;
using MCMicroLauncher.Utils;

namespace MCMicroLauncher.ApplicationState
{
    internal static class Log
    {
        static Log()
        => File.Delete(Constants.LogFilePath);

        internal static void Error(string msg, Exception ex)
        => Internal("Error", msg, ex.Message, ex.StackTrace);

        internal static void Error(string msg, params string[] moreMsgs)
        => Internal("Error", msg, moreMsgs);

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
                log += " | " + moreMsgs.JoinUsing(" | ");
            }

            log += "\n";

            File.AppendAllText(Constants.LogFilePath, log);
        }
    }
}
