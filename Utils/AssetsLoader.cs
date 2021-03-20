using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
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

        public async Task<(int count, IAsyncEnumerable<bool> progress)> PrepareAssetsAsync()
        {
            var config = await this.dataStore.GetConfigAsync();

            if (string.IsNullOrWhiteSpace(config.AssetsFolder)
                || !FileSystemUtils.TryGetLocalPath(config.AssetsFolder, out var assetsDir))
            {
                Log.Error("Missing assets file");
                return (-1, null);
            }

            var assetsIndexPath = Path.Combine(assetsDir, "indexes");
            var assetsIndex = Directory
                .GetFiles(assetsIndexPath, "*.json")
                .FirstOrDefault();

            if (assetsIndex == null)
            {
                Log.Error("Asset index was not found", assetsIndexPath);
                return (-1, null);
            }

            using var indexFile = File.OpenRead(assetsIndex);
            using var data = JsonDocument.Parse(indexFile);

            var objectsElements = data.RootElement
                .GetProperty("objects")
                .EnumerateObject();

            var paralelization = Math.Min(1, Environment.ProcessorCount - 1);
            var i = 0;
            var count = 0;
            var hashes = new List<string>[paralelization];
            foreach (var fileElement in objectsElements)
            {
                (hashes[i] ??= new List<string>())
                    .Add(fileElement.Value.GetProperty("hash").ToString());

                i = (i + 1) % paralelization;
                count++;
            }

            var channel = Channel.CreateUnbounded<bool>();
            var reader = channel.Reader;

            var tokenSource = new CancellationTokenSource();
            var assetsObjectsDir = Path.Combine(assetsDir, "objects");
            var tasks = hashes
                .Select(l => Worker(l, assetsObjectsDir, channel.Writer, tokenSource))
                .ToList();

            _ = Monitor(tasks, channel.Writer);

            return (count, reader.ReadAllAsync());

            static async Task Monitor(
                List<Task> tasks,
                ChannelWriter<bool> channelWriter)
            {
                await Task.WhenAll(tasks);
                channelWriter.TryComplete();
            }

            static async Task Worker(
                List<string> hashes,
                string assetsObjectsDir,
                ChannelWriter<bool> channelWriter,
                CancellationTokenSource tokenSource)
            {
                foreach (var hash in hashes)
                {
                    if (tokenSource.IsCancellationRequested)
                    {
                        return;
                    }

                    using var res = await client
                        .GetAsync(AssetsUrl + hash[0..2] + "/" + hash);

                    if (!res.IsSuccessStatusCode)
                    {
                        var errorContent = await res.Content.ReadAsStringAsync();
                        Log.Error("Asset download failed", hash, errorContent);

                        tokenSource.Cancel();
                        channelWriter.TryWrite(false);
                        channelWriter.TryComplete();

                        return;
                    }

                    if (tokenSource.IsCancellationRequested)
                    {
                        return;
                    }

                    var content = await res.Content.ReadAsByteArrayAsync();
                    var hashPath = Path.Combine(assetsObjectsDir, hash[0..2]);
                    Directory.CreateDirectory(hashPath);
                    var hashFile = Path.Combine(hashPath, hash);
                    File.WriteAllBytes(hashFile, content);

                    channelWriter.TryWrite(true);
                }
            }
        }
    }
}
