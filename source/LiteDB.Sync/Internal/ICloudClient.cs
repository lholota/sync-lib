using System.Threading;
using System.Threading.Tasks;

namespace LiteDB.Sync.Internal
{
    internal interface ICloudClient
    {
        Task<PullResult> Pull(CloudState localCloudState, CancellationToken ct);

        Task Push(Patch patch, string etag, CancellationToken ct);
    }
}