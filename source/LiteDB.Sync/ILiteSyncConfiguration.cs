using System.Collections.Generic;

namespace LiteDB.Sync
{
    public interface ILiteSyncConfiguration
    {
        ILiteSyncCloudProvider CloudProvider { get; }

        ILiteSyncConflictResolver ConflictResolver { get; }

        IEnumerable<string> SyncedCollections { get; }
    }
}