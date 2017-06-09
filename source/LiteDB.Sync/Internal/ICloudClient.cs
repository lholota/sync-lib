using System.Threading;
using System.Threading.Tasks;

namespace LiteDB.Sync.Internal
{
    internal interface ICloudClient
    {
        Task<PullResult> PullAsync(CloudState originalState, CancellationToken ct);

        Task<CloudState> PushAsync(CloudState localCloudState, Patch patch, CancellationToken ct);
    }
}