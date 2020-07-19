using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MCMicroLauncher.Authentication
{
    internal class AuthClient
    {
        private const string clientToken = "MCMicroLauncher";
        private const string baseUrl = "https://authserver.mojang.com";
        private static readonly HttpClient client = new HttpClient();

        private string accessToken;
        private string uuid;
        private string accountName;

        private readonly JavaCaller JavaCaller;

        internal AuthClient(JavaCaller javaCaller)
        {
            this.JavaCaller = javaCaller;
        }

        internal Task<bool> Validate()
        {
            if (string.IsNullOrWhiteSpace(this.accessToken))
            {
                return Task.FromResult(false);
            }

            return RunInternal();

            async Task<bool> RunInternal()
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(new
                    {
                        this.accessToken,
                        clientToken
                    }),
                    Encoding.UTF8,
                    "application/json");

                var res = await client
                    .PostAsync(baseUrl + "/validate", content);

                return res.IsSuccessStatusCode;
            }
        }

        internal Task<bool> Refresh()
        {
            if (string.IsNullOrWhiteSpace(this.accessToken)
                || string.IsNullOrWhiteSpace(this.uuid)
                || string.IsNullOrWhiteSpace(this.accountName))
            {
                return Task.FromResult(false);
            }

            return RunInternal();

            async Task<bool> RunInternal()
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(new
                    {
                        this.accessToken,
                        clientToken,
                        selectedProfile = new
                        {
                            id = this.uuid,
                            name = this.accountName
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

                this.accessToken = model.accessToken;

                return true;
            }
        }

        internal async Task<bool> Authenticate(
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

            this.accessToken = model.accessToken;
            this.uuid = model.selectedProfile.id;
            this.accountName = model.selectedProfile.name;

            return true;
        }

        internal string GetName() => this.accountName;

        internal void LaunchMinecraft(bool useBorderlessFullscreen)
        {
            this.JavaCaller.LaunchMinecraft(
                this.accountName,
                this.uuid,
                this.accessToken,
                useBorderlessFullscreen);
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
