using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using LiteDB.Sync;
using LiteDB.Sync.Exceptions;
using File = Google.Apis.Drive.v3.Data.File;

namespace LiteDb.Sync.GoogleDrive
{
    public class GoogleDriveCloudProvider : ILiteSyncCloudProvider, ILiteSyncPatchIdGenerator
    {
        private const string InitFileName = "LiteSync.init";

        private static readonly IEnumerable<string> Scopes = new[] { DriveService.Scope.DriveAppdata };

        private readonly string appName;
        private readonly string clientId;
        private readonly string clientSecret;

        private DriveService driveService;

        public GoogleDriveCloudProvider(string clientId, string clientSecret, string appName)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.appName = appName;
        }

        public async Task<Stream> DownloadInitFile(CancellationToken ct)
        {
            await this.EnsureService(ct);

            var initFileId = await this.GetInitFileId(ct);

            if (string.IsNullOrEmpty(initFileId))
            {
                return null;
            }

            return await this.DownloadFile(initFileId, ct);
        }

        public async Task UploadInitFile(Stream contents)
        {
            await this.EnsureService(CancellationToken.None);

            var body = new File();
            body.Name = InitFileName;
            body.Spaces = new[] { "appDataFolder" };
            body.MimeType = "text/plain";

            var createRequest = this.driveService.Files.Create(body, contents, "text/plain");
            await createRequest.UploadAsync();

            var validInitFileId = await this.GetInitFileId(CancellationToken.None);
            var uploadedInitFileId = createRequest.ResponseBody.Id;

            if (!string.Equals(validInitFileId, uploadedInitFileId, StringComparison.OrdinalIgnoreCase))
            {
                throw new LiteSyncConflictOccuredException();
            }
        }

        public async Task<Stream> DownloadPatchFile(string patchId, CancellationToken ct)
        {
            await this.EnsureService(ct);

            return await this.DownloadFile(patchId, ct);
        }

        public async Task UploadPatchFile(string patchId, Stream contents)
        {
            await this.EnsureService(CancellationToken.None);

            var body = new File();
            body.Id = patchId;
            body.Spaces = new[] { "appDataFolder" };
            body.MimeType = "text/plain";

            var createRequest = this.driveService.Files.Create(body, contents, "text/plain");
            await createRequest.UploadAsync();
        }

        public async Task<string> GeneratePatchId(CancellationToken ct)
        {
            await this.EnsureService(ct);

            var request = this.driveService.Files.GenerateIds();
            request.Count = 1;
            request.Space = "appDataFolder";

            var fileIds = await request.ExecuteAsync(ct);

            return fileIds.Ids.Single();
        }

        private async Task<Stream> DownloadFile(string fileId, CancellationToken ct)
        {
            var request = this.driveService.Files.Get(fileId);
            return await request.ExecuteAsStreamAsync(ct);
        }

        private async Task<string> GetInitFileId(CancellationToken ct)
        {
            var listRequest = this.driveService.Files.List();
            listRequest.Fields = "nextPageToken, files(id, name)";
            listRequest.Q = string.Format("name = '{0}'", InitFileName);

            var response = await listRequest.ExecuteAsync(ct);

            var initFile = response.Files
                .OrderBy(x => x.CreatedTime)
                .FirstOrDefault(x => x.Name == InitFileName);

            return initFile?.Id;
        }

        private async Task EnsureService(CancellationToken ct)
        {
            if (this.driveService == null)
            {
                var credentials = await this.Authenticate(ct);

                this.driveService = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credentials,
                    ApplicationName = this.appName,
                });
            }
        }

        private async Task<UserCredential> Authenticate(CancellationToken ct)
        {
            try
            {
                var secrets = new ClientSecrets
                {
                    ClientId = this.clientId,
                    ClientSecret = this.clientSecret
                };

                return await GoogleWebAuthorizationBroker.AuthorizeAsync(secrets, Scopes, "user", ct);
            }
            catch (Exception ex)
            { 
                throw new LiteSyncCloudAuthFailedException(this.GetType(), ex);
            }
        }
    }
}