namespace LiteDB.Sync
{
    using System.Threading.Tasks;

    public interface ILiteDbSyncController
    {
        void StartSyncWorker();

        void StopSyncWorker();

        Task SyncNow();
    }
}