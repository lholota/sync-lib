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

        public Task<PullResult> Pull(CloudState localCloudState, CancellationToken ct)
        {
            if (localCloudState == null)
            {
                var localHead = this.provider.CreateLocalState();
            }

            throw new NotImplementedException();
        }

        public Task Push(Patch patch, string etag, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
