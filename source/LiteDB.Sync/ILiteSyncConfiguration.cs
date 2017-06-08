namespace LiteDB.Sync
{
    public interface ILiteSyncConfiguration
    {
        ILiteSyncCloudProvider CloudProvider { get; }

        ILiteSyncConflictResolver ConflictResolver { get; }

        string[] SyncedCollections { get; }
    }
}