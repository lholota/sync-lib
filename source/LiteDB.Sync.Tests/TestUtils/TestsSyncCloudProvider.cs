using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LiteDB.Sync.Tests.TestUtils
{
    public class TestsSyncCloudProvider : ILiteSyncCloudProvider
    {
        public void Initialize()
        {
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