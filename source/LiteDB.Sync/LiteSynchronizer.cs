using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiteDB.Sync.Exceptions;
using LiteDB.Sync.Internal;

namespace LiteDB.Sync
{
    internal class LiteSynchronizer
    {
        private readonly LiteDatabase db;
        private readonly LiteSyncConfiguration config;
        private readonly ICloudClient cloudClient;

        internal LiteSynchronizer(LiteDatabase db, LiteSyncConfiguration config, ICloudClient cloudClient)
        {
            this.cloudClient = cloudClient;
            this.config = config;
            this.db = db;
        }

        internal async Task Synchronize(CancellationToken ct)
        {
            this.config.CloudProvider.Initialize();

            var localHead = this.db.GetSyncHead();
            ct.ThrowIfCancellationRequested();

            var pull = await this.cloudClient.Pull(localHead, ct);

            using (this.db.Engine.Locker.Exclusive())
            {
                var localChanges = this.GetLocalChanges(ct);

                if (!pull.RemoteChanges.HasChanges && !localChanges.HasChanges)
                {
                    return;
                }

                this.ResolveConflicts(localChanges, pull.RemoteChanges, ct);

                using (var tx = this.db.BeginTrans())
                {
                    this.ApplyChangesToLocalDb(pull.RemoteChanges);

                    // new head identifier should be generated -> create a push request which would create the file payloads etc. automatically?
                    await this.cloudClient.Push(localChanges, pull.Etag, ct);

                    tx.Commit();
                }
            }
        }

        private void ApplyChangesToLocalDb(Patch remoteChanges)
        {
            var groupped = remoteChanges.GroupBy(x => x.GlobalId.CollectionName, x => x);

            foreach (var group in groupped)
            {
                var collection = this.db.GetCollection(group.Key);

                foreach (var change in group)
                {
                    change.Apply(collection);
                }
            }
        }

        private void ResolveConflicts(Patch localChanges, Patch remoteChanges, CancellationToken ct)
        {
            var conflicts = Patch.GetConflicts(localChanges, remoteChanges);

            foreach (var conflict in conflicts)
            {
                ct.ThrowIfCancellationRequested();

                this.config.ConflictResolver.Resolve(conflict);


                switch (conflict.Resolution)
                {
                    case LiteSyncConflict.ConflictResolution.None:
                        throw new LiteSyncConflictNotResolvedException(conflict.EntityId);

                    case LiteSyncConflict.ConflictResolution.KeepLocal:
                        remoteChanges.RemoveChange(conflict.EntityId);
                        break;

                    case LiteSyncConflict.ConflictResolution.KeepRemote:
                        localChanges.RemoveChange(conflict.EntityId);
                        break;

                    case LiteSyncConflict.ConflictResolution.Merge:
                        localChanges.ReplaceEntity(conflict.EntityId, conflict.MergedEntity);
                        localChanges.ReplaceEntity(conflict.EntityId, conflict.MergedEntity);
                        break;
                }
            }
        }

        private Patch GetLocalChanges(CancellationToken ct)
        {
            var patch = new Patch();

            foreach (var collectionName in this.config.SyncedCollections)
            {
                var collection = this.db.GetCollection(collectionName);
                var dirtyEntities = collection.FindDirtyEntities();

                patch.AddChanges(collectionName, dirtyEntities);

                var delCollection = this.db.GetCollection<DeletedEntity>(LiteSyncDatabase.DeletedEntitiesCollectionName);
                var deletedEntities = delCollection.FindAll();

                patch.AddDeletes(deletedEntities);

                ct.ThrowIfCancellationRequested();
            }

            return patch;
        }
    }
}