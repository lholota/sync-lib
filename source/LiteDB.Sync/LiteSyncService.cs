using System;
using System.Threading;
using System.Threading.Tasks;
using LiteDB.Sync.Internal;

namespace LiteDB.Sync
{
    public class LiteSyncService
    {
        private Task syncInProgressTask;
        private CancellationTokenSource syncInProgressTokenSource;

        private readonly object syncControlLock = new object();
        private readonly ICloudClient cloudClient;
        private readonly LiteDatabase innerDb;
        private readonly LiteSyncConfiguration config;

        public LiteSyncService(LiteDatabase innerDb, LiteSyncConfiguration config)
            : this(innerDb, config, new Factory())
        {
        }

        internal LiteSyncService(LiteDatabase innerDb, LiteSyncConfiguration config, IFactory factory)
        {
            this.cloudClient = factory.CreateCloudClient(config.CloudProvider);
            this.innerDb = innerDb;
            this.config = config;
        }

        public event EventHandler SyncStarted;
        //public event EventHandler SyncProgressChanged;
        //public event EventHandler SyncProgressFinished;
        //public event EventHandler SyncFailed;

        public void EnsureIndices()
        {
            using (var tx = this.innerDb.BeginTrans())
            {
                foreach (var collectionName in this.config.SyncedCollections)
                {
                    var collection = this.innerDb.GetCollection(collectionName);
                    collection.EnsureIndex(nameof(ILiteSyncEntity.RequiresSync));
                }

                tx.Commit();
            }
        }

        public Task Synchronize(bool forceRestart)
        {
            lock (this.syncControlLock)
            {
                if (this.syncInProgressTask != null)
                {
                    if (forceRestart)
                    {
                        this.syncInProgressTokenSource.Cancel();
                        this.syncInProgressTask.Wait();
                    }
                    else
                    {
                        return Task.FromResult(0);
                    }
                }

                this.syncInProgressTokenSource = new CancellationTokenSource();
                var ctx = new LiteSynchronizer(this.innerDb, this.config, this.cloudClient);

                this.syncInProgressTask = ctx.Synchronize(this.syncInProgressTokenSource.Token);

                return this.syncInProgressTask;
            }
        }

        protected virtual void OnSyncStarted()
        {
            this.SyncStarted?.Invoke(this, EventArgs.Empty);
        }
    }
}