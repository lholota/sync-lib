namespace LiteDB.Sync
{
    using System.Threading.Tasks;
    using Entities;

    public class LiteDbSyncController : ILiteDbSyncController
    {
        private const string SyncCollectionName = "LiteSync";
        private const string LocalHeadId = "LocalHead";

        private readonly ILiteSyncProvider provider;
        private readonly ILiteDatabase db;

        public LiteDbSyncController(ILiteSyncProvider provider, ILiteDatabase db)
        {
            this.provider = provider;
            this.db = db;
        }

        public void StartSyncWorker()
        {
            throw new System.NotImplementedException();
        }

        public void StopSyncWorker()
        {
            throw new System.NotImplementedException();
        }

        public Task SyncNow()
        {
            var localHead = GetLocalHead();
            /*
             * Get state
             * Check local changes
             * Pull remote changes
             * Resolve conflicts if they happen
             * Push local changes + resolutions to the cloud
             */

            throw new System.NotImplementedException();
        }

        private StoreHead GetLocalHead()
        {
            return db.GetCollection<StoreHead>(SyncCollectionName).FindById(LocalHeadId);
        }
    }
}