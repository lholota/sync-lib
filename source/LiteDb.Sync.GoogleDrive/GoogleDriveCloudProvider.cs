using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;

namespace LiteDb.Sync.GoogleDrive
{
    public class GoogleDriveCloudProvider
    {
        private const string InitFileName = "LiteSync.init";

        private static readonly IEnumerable<string> Scopes = new[] { "" };

        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _appName;

        private DriveService driveService;

        /*
         * Has local state -> knows what file id is next -> can check if it exists directly
         * Doesn't have local state 
         *      -> doesn't know if it's connecting to existing repository or to a new one
         */
        public GoogleDriveCloudProvider(string clientId, string clientSecret, string appName)
        {
            this._clientId = clientId;
            this._clientSecret = clientSecret;
            this._appName = appName;
        }


        public async Task InitializeRemoteStorage(CancellationToken ct)
        {
            await this.EnsureService(ct);

            var listRequest = this.driveService.Files.List();
            listRequest.Fields = "nextPageToken, files(id, name)";
            listRequest.Q = string.Format("name = '{0}'", InitFileName);

            var response = await listRequest.ExecuteAsync(ct);

            if (response.Files.Any())
            {
                // Get the file and return the next id
            }
        }

        private async Task EnsureService(CancellationToken ct)
        {
            if (this.driveService == null)
            {
                var credentials = await this.Authenticate(ct);

                this.driveService = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credentials,
                    ApplicationName = this._appName,
                });
            }
        }

        private async Task<UserCredential> Authenticate(CancellationToken ct)
        {
            var secrets = new ClientSecrets
            {
                ClientId = this._clientId,
                ClientSecret = this._clientSecret
            };

            return await GoogleWebAuthorizationBroker.AuthorizeAsync(secrets, this.Scopes, "user", ct);
        }
    }
}