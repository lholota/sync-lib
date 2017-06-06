using System.Threading;
using System.Threading.Tasks;

namespace LiteDB.Sync.Internal
{
    internal interface ICloudClient
    {
        Task<PullResult> Pull(CloudState originalState, CancellationToken ct);

        Task Push(CloudState localCloudState, Patch patch, CancellationToken ct);
    }
}