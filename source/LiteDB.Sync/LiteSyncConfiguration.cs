using System.Collections.Generic;

namespace LiteDB.Sync
{
    public class LiteSyncConfiguration : ILiteSyncConfiguration
    {
        public LiteSyncConfiguration(
            ILiteSyncCloudProvider cloudProvider, 
            ILiteSyncConflictResolver conflictResolver, 
            IEnumerable<string> syncedCollections)
        {
            this.CloudProvider = cloudProvider;
            this.ConflictResolver = conflictResolver;
            this.SyncedCollections = syncedCollections;
        }

        public ILiteSyncCloudProvider CloudProvider { get; }

        public ILiteSyncConflictResolver ConflictResolver { get; }

        public IEnumerable<string> SyncedCollections { get; }

        // Calls before upload and after apply?
    }
}