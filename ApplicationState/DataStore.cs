using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace MCMicroLauncher.ApplicationState
{
    internal class DataStore
    {
        private const string DataFileName = "./data.json";

        private const string ConfigFileName = "./config.json";

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

            if (!File.Exists(ConfigFileName))
            {
                Log.Error("Config file not found");

                return Task.FromResult(new ConfigModel());
            }

            return RunInternal();

            async Task<ConfigModel> RunInternal()
            {
                using var file = File.OpenRead(ConfigFileName);

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

            if (!File.Exists(DataFileName))
            {
                this.data = new DataModel();

                return Task.CompletedTask;
            }

            return RunInternal();

            async Task RunInternal()
            {
                using var file = File.OpenRead(DataFileName);

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
                using var file = File.Create(DataFileName);

                await JsonSerializer.SerializeAsync(file, this.data);
            }
        }

        private class DataModel
        {
            public string AccessToken {get;set;}
            public string Uuid {get;set;}
            public string AccountName {get;set;}
            public bool BorderlessFullscreen {get;set;}
        }

        internal class ConfigModel
        {
            public string LibraryPath {get;set;}
            public string ClientJarPath {get;set;}
            public string[] Libraries {get;set;}
            public string Version {get;set;}
            public string GameDirPath {get;set;}
            public string AssetsDirPath {get;set;}
        }
    }
}
