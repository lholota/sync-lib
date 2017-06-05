using System.IO;
using System.Net.Http.Headers;
using System.Threading;
using LiteDB.Sync.Exceptions;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace LiteDb.Sync.OneDrive
{
    using System;
    using System.Threading.Tasks;
    using LiteDB.Sync;

    public class OneDriveCloudProvider : ILiteSyncCloudProvider
    {
        private static readonly string[] Scopes = { "onedrive.appfolder" };

        private const string HeadFileName = "Sync.head";
        private const string PatchFileNameFormat = "Changes/{0:N}.patch";

        private string userToken;
        private DateTimeOffset userTokenExpiration;
        private GraphServiceClient graphClient;

        private readonly PublicClientApplication clientApp;

        public OneDriveCloudProvider(string clientId)
        {
            this.clientApp = new PublicClientApplication(clientId);
        }

        // TODO: Wrap exceptions in individual methods
        public void EnsureClient()
        {
            this.graphClient = new GraphServiceClient(
                "https://graph.microsoft.com/v1.0",
                new DelegateAuthenticationProvider(
                    async requestMessage =>
                    {
                        var token = await this.GetUserToken();
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
                    }));
        }

        public async Task<Stream> DownloadInitFile(CancellationToken ct)
        {
            this.EnsureClient();

            var driveItem = await this.graphClient.Me.Drive.Special.AppRoot
                .ItemWithPath(HeadFileName)
                .Request()
                .GetAsync(ct);

            return driveItem.Content;
        }

        public async Task UploadInitFile(Stream contents)
        {
            this.EnsureClient();

            var driveItem = new DriveItem();
            driveItem.Content = contents;

            await this.graphClient.Me.Drive.Special.AppRoot
                      .ItemWithPath(HeadFileName)
                      .Request()
                      .UpdateAsync(driveItem);
        }

        public async Task<Stream> DownloadPatchFile(string id, CancellationToken ct)
        {
            this.EnsureClient();

            var fileName = string.Format(PatchFileNameFormat, id);

            var driveItem = await this.graphClient.Me.Drive.Special.AppRoot
                                      .ItemWithPath(fileName)
                                      .Request()
                                      .GetAsync(ct);

            return driveItem.Content;
        }

        public async Task UploadPatchFile(string id, Stream contents)
        {
            this.EnsureClient();

            var fileName = string.Format(PatchFileNameFormat, id);

            var driveItem = new DriveItem();
            driveItem.Content = contents;

            await this.graphClient.Me.Drive.Special.AppRoot
                      .ItemWithPath(fileName)
                      .Request()
                      .UpdateAsync(driveItem);
        }

        private async Task<string> GetUserToken()
        {
            try
            {
                if (this.userToken == null || this.userTokenExpiration <= DateTimeOffset.UtcNow.AddMinutes(5))
                {
                    var authResult = await this.clientApp.AcquireTokenAsync(Scopes);

                    this.userToken = authResult.AccessToken;
                    this.userTokenExpiration = authResult.ExpiresOn;
                }

                return this.userToken;
            }
            catch (Exception ex)
            {
                throw new LiteSyncCloudAuthFailedException(this.GetType(), ex);
            }
        }
    }
}