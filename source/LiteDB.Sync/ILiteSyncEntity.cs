namespace LiteDB.Sync
{
    public interface ILiteSyncEntity
    {
        int SyncState { get; set; }
    }
}