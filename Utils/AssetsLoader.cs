using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MCMicroLauncher.ApplicationState;

namespace MCMicroLauncher.Utils
{
    internal class AssetsLoader
    {
        private const string AssetsUrl
            = "https://resources.download.minecraft.net/";

        private static readonly HttpClient client = new();

        private readonly DataStore dataStore;

        public AssetsLoader(
            DataStore dataStore)
        {
            this.dataStore = dataStore;
        }

        public async Task<bool> PrepareAssetsAsync()
        {
            var config = await this.dataStore.GetConfigAsync();

            if (string.IsNullOrWhiteSpace(config.AssetsFolder)
                || !FileSystemUtils.TryGetLocalPath(config.AssetsFolder, out var assetsDir))
            {
                Log.Error("Missing assets file");
                return false;
            }

            var assetsIndexPath = Path.Combine(assetsDir, "indexes");
            var assetsIndex = Directory
                .GetFiles(assetsIndexPath, "*.json")
                .FirstOrDefault();

            if (assetsIndex == null)
            {
                Log.Error("Asset index was not found", assetsIndexPath);
                return false;
            }

            using var indexFile = File.OpenRead(assetsIndex);
            using var data = JsonDocument.Parse(indexFile);

            var objectsElements = data.RootElement
                .GetProperty("objects")
                .EnumerateObject();

            var paralelization = Environment.ProcessorCount;
            var i = 0;
            var hashes = new List<string>[paralelization];
            foreach (var fileElement in objectsElements)
            {
                (hashes[i] ??= new List<string>())
                    .Add(fileElement.Value.GetProperty("hash").ToString());

                i = (i + 1) % paralelization;
            }

            var tokenSource = new CancellationTokenSource();
            var assetsObjectsDir = Path.Combine(assetsDir, "objects");
            var results = await Task.WhenAny(
                hashes.Select(l => Worker(l, assetsObjectsDir, tokenSource.Token)));

            if (!results.Result)
            {
                tokenSource.Cancel();
                return false;
            }

            await this.dataStore.SetAssetsPreparedAsync(true);
            return true;

            static async Task<bool> Worker(
                List<string> hashes,
                string assetsObjectsDir,
                CancellationToken token)
            {
                foreach (var hash in hashes)
                {
                    if (token.IsCancellationRequested)
                    {
                        return false;
                    }

                    using var res = await client.GetAsync(
                        AssetsUrl + hash[0..2] + "/" + hash,
                        token);

                    if (!res.IsSuccessStatusCode)
                    {
                        var errorContent = await res.Content.ReadAsStringAsync(token);
                        Log.Error("Asset download failed", hash, errorContent);
                        return false;
                    }

                    var content = await res.Content.ReadAsByteArrayAsync(token);
                    var hashPath = Path.Combine(assetsObjectsDir, hash[0..2]);
                    Directory.CreateDirectory(hashPath);
                    var hashFile = Path.Combine(hashPath, hash);
                    File.WriteAllBytes(hashFile, content);
                }

                return true;
            }
        }
    }
}
