using System;
using System.Threading.Tasks;
using LiteDB.Sync.Contract;

namespace LiteDB.Sync
{
    using System.Collections.Generic;

    public class LiteSyncService : ILiteSyncService
    {
        private readonly Func<FileMode, LiteDatabase> dbFactoryFunc;

        public LiteSyncService(ILiteSyncCloudProvider cloudProvider, Func<FileMode, LiteDatabase> dbFactoryFunc, IEnumerable<string> syncedCollections)
        {
            this.CloudStorageProvider = cloudProvider;
            this.SyncedCollections = syncedCollections;
            this.dbFactoryFunc = dbFactoryFunc;
        }

        public IEnumerable<string> SyncedCollections { get; }

        public ILiteSyncCloudProvider CloudStorageProvider { get; }

        public void StartSyncWorker()
        {
            throw new System.NotImplementedException();
        }

        public void StopSyncWorker()
        {
            throw new System.NotImplementedException();
        }

        public void EnsureIndices()
        {
            using (var db = this.dbFactoryFunc.Invoke(FileMode.Exclusive))
            using (var tx = db.BeginTrans())
            {
                foreach (var collectionName in this.SyncedCollections)
                {
                    var collection = db.GetCollection(collectionName);
                    collection.EnsureIndex(nameof(ILiteSyncEntity.SyncState));
                }

                tx.Commit();
            }
        }

        public Task SyncNow()
        {
            using (var db = this.dbFactoryFunc.Invoke(FileMode.Exclusive))
            {
                using (var tx = db.BeginTrans())
                {
                    var localHead = db.GetSyncHead();

                    var remoteChanges = this.CloudStorageProvider.Pull(localHead.TransactionId);
                    var localChanges = this.GetLocalChanges(db);

                    /*
                     * Get state
                     * Pull remote changes
                     * Flatten the remote changes
                     * Check local changes (need them for conflict detection...)
                     * Apply changes, resolve conflicts if they happen
                     * Push local changes + resolutions to the cloud
                     */

                    throw new System.NotImplementedException();
                }
            }
        }

        private Patch GetLocalChanges(LiteDatabase db)
        {
            var pushTransaction = new Patch();

            foreach (var collectionName in this.SyncedCollections)
            {
                var collection = db.GetCollection(collectionName);
                var dirtyEntities = collection.FindDirtyEntities();

                pushTransaction.AddChanges(collectionName, dirtyEntities);
            }

            return pushTransaction;
        }
    }
}