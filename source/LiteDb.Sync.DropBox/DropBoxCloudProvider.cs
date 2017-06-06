using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LiteDB.Sync;

namespace LiteDb.Sync.DropBox
{
    public class DropBoxCloudProvider : ILiteSyncCloudProvider
    {
        private readonly string clientId;

        public DropBoxCloudProvider(string clientId)
        {
            this.clientId = clientId;
        }

        public Task<Stream> DownloadInitFile(CancellationToken ct)
        {
            throw new System.NotImplementedException();
        }

        public Task UploadInitFile(Stream contents)
        {
            throw new System.NotImplementedException();
        }

        public Task<Stream> DownloadPatchFile(string id, CancellationToken ct)
        {
            throw new System.NotImplementedException();
        }

        public Task UploadPatchFile(string id, Stream contents)
        {
            throw new System.NotImplementedException();
        }
    }
}