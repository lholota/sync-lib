namespace LiteDB.Sync
{
    using System.Threading.Tasks;

    public interface ILiteSyncService
    {
        void StartSyncWorker();

        void StopSyncWorker();

        Task SyncNow();
    }
}