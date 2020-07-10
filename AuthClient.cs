using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MCMicroLauncher
{
    public static class AuthClient
    {
        private const string clientToken = "MCMicroLauncher";
        private const string baseUrl = "https://authserver.mojang.com";
        private static readonly HttpClient client = new HttpClient();

        private static string accessToken;
        private static string uuid;
        private static string accountName;

        public static async Task<bool> Validate()
        {
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

        public static async Task Refresh()
        {
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

            var json = await res.Content.ReadAsStringAsync();
            var model = JsonSerializer.Deserialize<AuthResponse>(json);
            accessToken = model.accessToken;
        }

        public static async Task Authenticate(string username, string password)
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

            var json = await res.Content.ReadAsStringAsync();
            var model = JsonSerializer.Deserialize<AuthResponse>(json);
            accessToken = model.accessToken;
            uuid = model.selectedProfile.id;
            accountName = model.selectedProfile.name;

            JavaCaller.LaunchMinecraft(accountName, uuid, accessToken);
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
