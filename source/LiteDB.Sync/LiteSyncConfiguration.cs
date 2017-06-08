namespace LiteDB.Sync
{
    public class LiteSyncConfiguration : ILiteSyncConfiguration
    {
        public LiteSyncConfiguration(
            ILiteSyncCloudProvider cloudProvider, 
            ILiteSyncConflictResolver conflictResolver, 
            string[] syncedCollections)
        {
            this.CloudProvider = cloudProvider;
            this.ConflictResolver = conflictResolver;
            this.SyncedCollections = syncedCollections;
        }

        public ILiteSyncCloudProvider CloudProvider { get; }

        public ILiteSyncConflictResolver ConflictResolver { get; }

        public string[] SyncedCollections { get; }

        // Calls before upload and after apply?
    }
}