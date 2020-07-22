using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MCMicroLauncher.ApplicationState;

namespace MCMicroLauncher.Authentication
{
    internal class AuthClient
    {
        private const string clientToken = "MCMicroLauncher";
        private const string baseUrl = "https://authserver.mojang.com";
        private static readonly HttpClient client = new HttpClient();

        private readonly DataStore DataStore;

        internal AuthClient(
            DataStore dataStore)
        {
            this.DataStore = dataStore;
        }

        internal async Task<bool> ValidateAsync()
        {
            Log.Info("Validating token");

            var (accessToken, _, _) = await this.DataStore.GetLoginInfoAsync();

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                Log.Info("Validation skipped, no cached token");
                return false;
            }

            var content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    accessToken,
                    clientToken
                }),
                Encoding.UTF8,
                "application/json");

            var res = await client
                .PostAsync(baseUrl + "/validate", content);

            Log.Info($"Validation {(res.IsSuccessStatusCode ? "" : "un")}successful",
                    $"{(int)res.StatusCode}");

            return res.IsSuccessStatusCode;
        }

        internal async Task<bool> RefreshAsync()
        {
            Log.Info("Refreshing token");

            var (accessToken, uuid, accountName)
                = await this.DataStore.GetLoginInfoAsync();

            if (string.IsNullOrWhiteSpace(accessToken)
                || string.IsNullOrWhiteSpace(uuid)
                || string.IsNullOrWhiteSpace(accountName))
            {
                Log.Info("Refreshing skipped, no cached info");
                return false;
            }

            var content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    accessToken,
                    clientToken,
                    selectedProfile = new
                    {
                        id = uuid,
                        name = accountName
                    }
                }),
                Encoding.UTF8,
                "application/json");

            var res = await client
                .PostAsync(baseUrl + "/refresh", content);

            var response = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
            {
                Log.Info("Refresh failed",
                        $"{(int)res.StatusCode}",
                        response);

                return false;
            }

            var model = JsonSerializer.Deserialize<AuthResponse>(response);

            await this.DataStore.SetLoginInfoAsync(
                model.accessToken,
                model.selectedProfile.id,
                model.selectedProfile.name);

            Log.Info("Refresh successful");

            return true;
        }

        internal async Task<bool> AuthenticateAsync(
            string username,
            string password)
        {
            Log.Info("Authenticating");

            var content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    agent = new
                    {
                        name = "Minecraft",
                        version = 1
                    },

                    username,
                    password,
                    clientToken
                }),
                Encoding.UTF8,
                "application/json");

            var res = await client
                .PostAsync(baseUrl + "/authenticate", content);

            var response = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
            {
                Log.Warn("Authentication failed",
                        $"{(int)res.StatusCode}",
                        response);

                return false;
            }

            var model = JsonSerializer.Deserialize<AuthResponse>(response);

            await this.DataStore.SetLoginInfoAsync(
                model.accessToken,
                model.selectedProfile.id,
                model.selectedProfile.name);

            Log.Info("Authentication successful");

            return true;
        }

        private class AuthResponse
        {
            public string accessToken {get;set;}

            public SelectedProfile selectedProfile {get;set;}

            public class SelectedProfile
            {
                public string name {get;set;}

                public string id {get;set;}
            }
        }
    }
}
