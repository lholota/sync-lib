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

        public bool IsValid => this.CloudProvider != null
                               && this.ConflictResolver != null
                               && this.SyncedCollections != null;
    }
}