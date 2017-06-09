namespace LiteDB.Sync
{
    public interface ILiteSyncConflictResolver
    {
        void Resolve(LiteSyncConflict conflict, BsonMapper mapper);
    }
}