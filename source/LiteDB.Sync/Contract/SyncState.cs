namespace LiteDB.Sync.Contract
{
    internal class SyncState
    {
        internal const int None = 0;
        internal const int RequiresSync = 1;
        internal const int RequiresSyncDeleted = 2;
    }
}