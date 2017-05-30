namespace LiteDB.Sync
{
    public interface ILiteSyncEntity
    {
        bool RequiresSync { get; set; }
    }
}