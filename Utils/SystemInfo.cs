using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MCMicroLauncher.Utils
{
    internal static class SystemInfo
    {
        internal static readonly string RunningDirectory
            = AppDomain.CurrentDomain.BaseDirectory is var dir
            && dir.EndsWith('\\')
                ? dir[0..^1]
                : dir;

        internal static readonly Size MonitorSize
            = SystemInformation.PrimaryMonitorSize;

        internal static readonly string DefaultMinecraftPath
            = Path.Combine(Environment
                .GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Constants.DefaultMinecraftFolderName);
    }
}
