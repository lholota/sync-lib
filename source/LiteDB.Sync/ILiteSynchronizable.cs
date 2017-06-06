using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LiteDB.Sync
{
    public interface ILiteSynchronizable
    {
        IEnumerable<string> SyncedCollectionNames { get; }
            
        Task SynchronizeAsync(CancellationToken ct);

        // TBA: Worker methods
        // TBA: Sync events
        // Repository doesn't need to implement everything, it should only pass through the Synchronize method
    }
}