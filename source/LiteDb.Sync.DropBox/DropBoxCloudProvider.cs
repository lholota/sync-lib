using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dropbox.Api;
using LiteDB.Sync;

namespace LiteDb.Sync.DropBox
{
    public class DropBoxCloudProvider : ILiteSyncCloudProvider
    {
        public void Initialize()
        {
            // Dropbox.Api.DropboxClient client = new DropboxClient();
            throw new NotImplementedException();
        }

        public Task<HeadDownloadResult> DownloadHeadFile(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task UploadHeadFile(Stream contents, string etag, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> DownloadPatch(Guid id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task UploadPatch(Stream contents, Guid id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}