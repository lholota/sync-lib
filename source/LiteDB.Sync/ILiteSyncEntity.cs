namespace LiteDB.Sync
{
    public interface ILiteSyncEntity
    {
        EntitySyncState SyncState { get; set; }
    }
}