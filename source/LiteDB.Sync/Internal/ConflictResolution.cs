namespace LiteDB.Sync.Internal
{
    internal enum ConflictResolution
    {
        None,
        KeepLocal,
        KeepRemote,
        Merge
    }
}