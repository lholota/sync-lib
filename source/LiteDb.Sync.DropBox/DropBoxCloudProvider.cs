using System;
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

        public Task<object> DownloadHeadFile(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task UploadHeadFile(byte[] contents, object etag, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> DownloadPatch(Guid id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task UploadPatch(byte[] contents, Guid id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}