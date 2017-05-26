namespace LiteDB.Sync
{
    using System.Threading.Tasks;

    public interface ILiteSyncProvider
    {
        Task<object> Pull();

        Task Push(object args);
    }
}