using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LiteDB.Sync.Contract;
using LiteDB.Sync.Internal;

namespace LiteDB.Sync
{
    internal class LiteSynchronizer
    {
        private readonly LiteDatabase db;
        private readonly LiteSyncConfiguration config;

        internal LiteSynchronizer(LiteDatabase db, LiteSyncConfiguration config)
        {
            this.config = config;
            this.db = db;
        }

        internal async Task Synchronize(CancellationToken ct)
        {
            var localHead = this.db.GetSyncHead();
            ct.ThrowIfCancellationRequested();

            // TODO: Pull result should contain remote head etag or version number depending on the provider
            var remoteChanges = await this.config.CloudProvider.Pull(localHead?.PatchId, ct);

            using (this.db.Engine.Locker.Exclusive())
            {
                var localChanges = this.GetLocalChanges(ct);
                var remoteCombined = Patch.Combine(remoteChanges);

                if (!this.HasChanges(remoteChanges) && !localChanges.HasChanges)
                {
                    return;
                }

                this.ResolveConflicts(localChanges, remoteCombined, ct);

                using (var tx = this.db.BeginTrans())
                {
                    this.ApplyChangesToLocalDb(remoteCombined);

                    await this.config.CloudProvider.Push(localChanges, ct);

                    tx.Commit();
                }
            }
        }

        private void ApplyChangesToLocalDb(Patch remoteChanges)
        {
            var collections = new Dictionary<string, ILiteCollection<BsonDocument>>();

            foreach (var change in remoteChanges)
            {
                if (!collections.TryGetValue(change.GlobalEntityId.CollectionName,
                                             out ILiteCollection<BsonDocument> collection))
                {
                    collection = this.db.GetCollection(change.GlobalEntityId.CollectionName);
                    collections.Add(change.GlobalEntityId.CollectionName, collection);
                }

                // collection.Apply(change);
                throw new NotImplementedException();
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
                    case ConflictResolution.None:
                        throw LiteSyncException.ConflictNotResolved(conflict.LocalChange.GlobalEntityId);

                    case ConflictResolution.KeepLocal:
                        remoteChanges.RemoveChange(conflict.EntityId);
                        break;

                    case ConflictResolution.KeepRemote:
                        localChanges.RemoveChange(conflict.EntityId);
                        break;

                    case ConflictResolution.Merge:
                        localChanges.ReplaceEntity(conflict.EntityId, conflict.MergedEntity);
                        localChanges.ReplaceEntity(conflict.EntityId, conflict.MergedEntity);
                        break;
                }
            }
        }

        private bool HasChanges(IList<Patch> patches)
        {
            return patches != null && patches.Count > 0;
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