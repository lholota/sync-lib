using System.Threading;
using System.Threading.Tasks;

namespace LiteDB.Sync.Internal
{
    internal interface ISynchronizer
    {
        Task SynchronizeAsync(CancellationToken ct);
    }
}