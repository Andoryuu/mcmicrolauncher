using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using MCMicroLauncher.ApplicationState;

namespace MCMicroLauncher.Authentication
{
    internal class JavaCaller
    {
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

            var (accessToken, uuid, accountName) = await this.DataStore.GetLoginInfoAsync();
            var borderlessFullscreen = await this.DataStore.GetBorderlessFullscreenAsync();
            var config = await this.DataStore.GetConfigAsync();

            if (string.IsNullOrWhiteSpace(accessToken)
                || string.IsNullOrWhiteSpace(uuid)
                || string.IsNullOrWhiteSpace(accountName)
                || string.IsNullOrWhiteSpace(config.AssetsDirPath)
                || string.IsNullOrWhiteSpace(config.ClientJarPath)
                || string.IsNullOrWhiteSpace(config.GameDirPath)
                || string.IsNullOrWhiteSpace(config.LibraryPath)
                || string.IsNullOrWhiteSpace(config.Version)
                || config.Libraries == null)
            {
                Log.Error("Aborting MC launch, missing config data");
                return;
            }

            var libraries = string.Join(";", config.Libraries);
            if (!config.Libraries.Any(x => x == config.ClientJarPath))
            {
                libraries += ";" + config.ClientJarPath;
            }

            var arguments = string.Join(" ", new[]
            {
                "\"-Dos.name=Windows 10\"",
                "-Dos.version=10.0",
                "-XX:HeapDumpPath=MojangTricksIntelDriversForPerformance_javaw.exe_minecraft.exe.heapdump",
                "-Djava.library.path=" + config.LibraryPath,
                "-Dminecraft.client.jar=" + config.ClientJarPath,
                "-cp " + libraries,
                "-Xms3G -Xmx3G -d64 -XX:+DisableExplicitGC",
                borderlessFullscreen ? "-Dorg.lwjgl.opengl.Window.undecorated=true" : "",
                "net.minecraft.launchwrapper.Launch",
                "--username " + accountName,
                "--version " + config.Version,
                "--gameDir " + config.GameDirPath,
                "--assetsDir " + config.AssetsDirPath,
                "--assetIndex 1.12",
                "--uuid " + uuid,
                "--accessToken " + accessToken,
                "--userType mojang",
                "--tweakClass net.minecraftforge.fml.common.launcher.FMLTweaker",
                "--versionType Forge"
            });

            if (borderlessFullscreen)
            {
                var size = SystemInformation.PrimaryMonitorSize;
                arguments += $" --width {size.Width} --height {size.Height} ";
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
    }
}
