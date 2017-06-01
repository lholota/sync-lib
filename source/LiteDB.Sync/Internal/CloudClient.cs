using System;
using System.Threading;
using System.Threading.Tasks;

namespace LiteDB.Sync.Internal
{
    internal class CloudClient : ICloudClient
    {
        private readonly ILiteSyncCloudProvider provider;

        public CloudClient(ILiteSyncCloudProvider provider)
        {
            this.provider = provider;
        }

        public Task<PullResult> Pull(Head localHead, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task Push(Patch patch, string etag, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
