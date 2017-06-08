using System.Threading;
using System.Threading.Tasks;

namespace LiteDB.Sync
{
    internal interface ILiteSynchronizer
    {
        Task SynchronizeAsync(CancellationToken ct);
    }
}