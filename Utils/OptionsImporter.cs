using System;
using System.IO;
using MCMicroLauncher.ApplicationState;

namespace MCMicroLauncher.Utils
{
    internal static class OptionsImporter
    {
        internal static bool HasLocalOptions()
        => Directory
            .GetFiles(SystemInfo.RunningDirectory, Constants.OptionsPattern)
            .Length > 0;

        internal static bool HasDefaultOptions()
        => Directory
            .GetFiles(SystemInfo.DefaultMinecraftPath, Constants.OptionsPattern)
            .Length > 0;

        internal static string[] GetDefaultOptionsPaths()
        => Directory.GetFiles(
            SystemInfo.DefaultMinecraftPath,
            Constants.OptionsPattern);

        internal static bool ImportOptions(params string[] fileNames)
        {
            if (fileNames.Length == 0)
            {
                return false;
            }

            var result = true;

            foreach (var file in fileNames)
            {
                try
                {
                    var newFile = Path.Combine(
                        SystemInfo.RunningDirectory,
                        Path.GetFileName(file));

                    File.Copy(file, newFile, overwrite: true);
                }
                catch (Exception ex)
                {
                    Log.Error($"Options import failed for {file}", ex);
                    result = false;
                }
            }

            return result;
        }
    }
}
