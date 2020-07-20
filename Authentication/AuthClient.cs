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
            var (accessToken, _, _) = await this.DataStore.GetLoginInfoAsync();

            if (string.IsNullOrWhiteSpace(accessToken))
            {
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

            return res.IsSuccessStatusCode;
        }

        internal async Task<bool> RefreshAsync()
        {
            var (accessToken, uuid, accountName)
                = await this.DataStore.GetLoginInfoAsync();

            if (string.IsNullOrWhiteSpace(accessToken)
                || string.IsNullOrWhiteSpace(uuid)
                || string.IsNullOrWhiteSpace(accountName))
            {
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

            if (!res.IsSuccessStatusCode)
            {
                return false;
            }

            var json = await res.Content.ReadAsStringAsync();
            var model = JsonSerializer.Deserialize<AuthResponse>(json);

            await this.DataStore.SetLoginInfoAsync(
                model.accessToken,
                model.selectedProfile.id,
                model.selectedProfile.name);

            return true;
        }

        internal async Task<bool> AuthenticateAsync(
            string username,
            string password)
        {
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

            if (!res.IsSuccessStatusCode)
            {
                return false;
            }

            var json = await res.Content.ReadAsStringAsync();
            var model = JsonSerializer.Deserialize<AuthResponse>(json);

            await this.DataStore.SetLoginInfoAsync(
                model.accessToken,
                model.selectedProfile.id,
                model.selectedProfile.name);

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
