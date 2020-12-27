using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MCMicroLauncher.Utils;

namespace MCMicroLauncher.ApplicationState
{
    internal class DataStore
    {
        private static readonly JsonSerializerOptions DataSerializationOptions
            = new() { WriteIndented = true };

        private DataModel data;

        private ConfigModel config;

        internal async Task<(string accessToken, string uuid, string accountName)> GetLoginInfoAsync()
        {
            await this.EnsureData();

            return (this.data.AccessToken, this.data.Uuid, this.data.AccountName);
        }

        internal async Task SetLoginInfoAsync(
            string accessToken,
            string uuid,
            string accountName)
        {
            await this.EnsureData();

            this.data.AccessToken = accessToken;
            this.data.Uuid = uuid;
            this.data.AccountName = accountName;

            await this.PersistData();
        }

        internal async Task<bool> GetBorderlessFullscreenAsync()
        {
            await this.EnsureData();

            return this.data.BorderlessFullscreen;
        }

        internal async Task SetBorderlessFullscreenAsync(
            bool borderlessFullscreen)
        {
            await this.EnsureData();

            this.data.BorderlessFullscreen = borderlessFullscreen;

            await this.PersistData();
        }

        internal Task<ConfigModel> GetConfigAsync()
        {
            if (this.config != null)
            {
                return Task.FromResult(this.config);
            }

            if (!File.Exists(Constants.ConfigFileName))
            {
                Log.Error("Config file not found");

                return Task.FromResult(new ConfigModel());
            }

            return RunInternal();

            async Task<ConfigModel> RunInternal()
            {
                using var file = File.OpenRead(Constants.ConfigFileName);

                this.config = await JsonSerializer
                    .DeserializeAsync<ConfigModel>(file);

                return this.config ?? new ConfigModel();
            }
        }

        private Task EnsureData()
        {
            if (this.data != null)
            {
                return Task.CompletedTask;
            }

            if (!File.Exists(Constants.DataFileName))
            {
                this.data = new DataModel();

                return Task.CompletedTask;
            }

            return RunInternal();

            async Task RunInternal()
            {
                using var file = File.OpenRead(Constants.DataFileName);

                if (file.Length == 0)
                {
                    this.data = new DataModel();
                    return;
                }

                this.data = await JsonSerializer
                    .DeserializeAsync<DataModel>(file);

                this.data ??= new DataModel();
            }
        }

        private Task PersistData()
        {
            if (this.data == null)
            {
                return Task.CompletedTask;
            }

            return RunInternal();

            async Task RunInternal()
            {
                using var file = File.Create(Constants.DataFileName);

                await JsonSerializer
                    .SerializeAsync(file, this.data, DataSerializationOptions);
            }
        }

        private class DataModel
        {
            public string AccessToken { get; set; }
            public string Uuid { get; set; }
            public string AccountName { get; set; }
            public bool BorderlessFullscreen { get; set; }
        }

        internal class ConfigModel
        {
            public string ClientPath { get; set; }
            public string AssetsFolder { get; set; }
            public string BinariesFolder { get; set; }
            public string LibrariesFolder { get; set; }
            public string JavaArguments { get; set; }

            public string[] Libraries { get; set; }

            public FmlOptionsModel FmlOptions { get; set; }

            public class FmlOptionsModel
            {
                public string ForgeVersion { get; set; }
                public string McVersion { get; set; }
                public string ForgeGroup { get; set; }
                public string McpVersion { get; set; }
            }
        }
    }
}
