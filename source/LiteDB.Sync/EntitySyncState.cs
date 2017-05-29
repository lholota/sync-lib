namespace LiteDB.Sync
{
    public enum EntitySyncState
    {
        None = 0,
        RequiresSync = 1,
        RequiresSyncDeleted = 2
    }
}