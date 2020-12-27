using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MCMicroLauncher.ApplicationState;
using MCMicroLauncher.Utils;

namespace MCMicroLauncher.Authentication
{
    internal class JavaCaller
    {
        private static readonly EnumerationOptions LibSearchOptions
            = new() { RecurseSubdirectories = true };

        private readonly StateMachine<State, Trigger> StateMachine;
        private readonly DataStore DataStore;

        internal JavaCaller(
            StateMachine<State, Trigger> stateMachine,
            DataStore dataStore)
        {
            this.StateMachine = stateMachine;
            this.DataStore = dataStore;
        }

        internal async Task LaunchMinecraftAsync()
        {
            Log.Info("Preparing MC launch");

            var arguments = await PrepareLaunchArguments(this.DataStore);

            if (string.IsNullOrWhiteSpace(arguments))
            {
                return;
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "cmd.exe",
                    Arguments = "/C javaw " + arguments
                },
                EnableRaisingEvents = true
            };

            process.Exited += (object sender, EventArgs e) =>
            {
                Log.Info("MC stopped");
                this.StateMachine.Call(Trigger.MinecraftStopped);
            };

            this.StateMachine.Call(Trigger.MinecraftLaunched);

            Log.Info("Launching MC");
            process.Start();
        }

        private static async Task<string> PrepareLaunchArguments(
            DataStore dataStore)
        {
            var (accessToken, uuid, accountName)
                = await dataStore.GetLoginInfoAsync();

            var borderlessFullscreen
                = await dataStore.GetBorderlessFullscreenAsync();

            var config = await dataStore.GetConfigAsync();

            if (string.IsNullOrWhiteSpace(accessToken)
                || string.IsNullOrWhiteSpace(uuid)
                || string.IsNullOrWhiteSpace(accountName)
                || string.IsNullOrWhiteSpace(config.Version)
                || string.IsNullOrWhiteSpace(config.ClientPath)
                || string.IsNullOrWhiteSpace(config.AssetsFolder)
                || string.IsNullOrWhiteSpace(config.BinariesFolder)
                || string.IsNullOrWhiteSpace(config.LibrariesFolder)
                || string.IsNullOrWhiteSpace(config.JavaArguments))
            {
                Log.Error("Aborting MC launch, missing config data");
                return null;
            }

            if (!TryGetLocalPath(config.AssetsFolder, out var assetsDir))
            {
                return null;
            }

            if (!TryGetLocalPath(config.BinariesFolder, out var binariesDir))
            {
                return null;
            }

            if (!TryGetLocalPath(config.LibrariesFolder, out var librariesDir))
            {
                return null;
            }

            if (!TryGetLocalPath(config.ClientPath, out var clientPath))
            {
                return null;
            }

            var libraries = Directory
                .GetFiles(librariesDir, "*.jar", LibSearchOptions)
                .JoinUsing(";");

            libraries += ";" + clientPath;

            var assetsIndexPath = Path.Combine(assetsDir, "indexes");
            var assetsIndex = Directory
                .GetFiles(assetsIndexPath, "*.json")
                .FirstOrDefault();

            if (assetsIndex == null)
            {
                Log.Error("Asset index was not found", assetsIndexPath);
                return null;
            }

            assetsIndex = Path.GetFileName(assetsIndex).Replace(".json", "");

            var arguments = new[]
            {
                "\"-Dos.name=Windows 10\"",
                "-Dos.version=10.0",
                "-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump",
                "-Djava.library.path=" + binariesDir,
                "-Dminecraft.client.jar=" + clientPath,
                "-cp " + libraries,
                config.JavaArguments,
                borderlessFullscreen ? "-Dorg.lwjgl.opengl.Window.undecorated=true" : "",
                "net.minecraft.launchwrapper.Launch",
                "--username " + accountName,
                "--version " + config.Version,
                "--gameDir " + AppDomain.CurrentDomain.BaseDirectory,
                "--assetsDir " + assetsDir,
                "--assetIndex " + assetsIndex,
                "--uuid " + uuid,
                "--accessToken " + accessToken,
                "--userType mojang",
                "--tweakClass net.minecraftforge.fml.common.launcher.FMLTweaker",
                "--versionType Forge"
            }.JoinUsing(" ");

            if (borderlessFullscreen)
            {
                var size = SystemInformation.PrimaryMonitorSize;
                arguments += $" --width {size.Width} --height {size.Height} ";
            }

            return arguments;
        }

        private static bool TryGetLocalPath(
            string relativePath,
            out string localPath)
        {
            var currentDir = AppDomain.CurrentDomain.BaseDirectory;
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
