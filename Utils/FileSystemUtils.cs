using System.IO;
using MCMicroLauncher.ApplicationState;

namespace MCMicroLauncher.Utils
{
    internal static class FileSystemUtils
    {
        internal static bool TryGetLocalPath(
            string relativePath,
            out string localPath)
        {
            var currentDir = SystemInfo.RunningDirectory;
            localPath = Path.Combine(currentDir, relativePath);

            var exists = Directory.Exists(localPath)
                || File.Exists(localPath);

            if (!exists)
            {
                Log.Error("File or directory was not found", localPath);
            }

            return exists;
        }
    }
}
