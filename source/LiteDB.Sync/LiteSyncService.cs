using System;
using System.Threading.Tasks;
using LiteDB.Sync.Entities;

namespace LiteDB.Sync
{
    public abstract class LiteSyncService : ILiteSyncService
    {
        private const string LocalHeadId = "LocalHead";

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

        protected abstract ILiteSyncCloudProvider GetProvider();

        protected abstract object GetConflictResolver();

        private StoreHead GetLocalHead()
        {
            throw new NotImplementedException();
            //return db.GetCollection<StoreHead>(SyncCollectionName).FindById(LocalHeadId);
        }
    }
}