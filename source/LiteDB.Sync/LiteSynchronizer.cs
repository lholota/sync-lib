using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiteDB.Sync.Exceptions;
using LiteDB.Sync.Internal;

namespace LiteDB.Sync
{
    internal class LiteSynchronizer : ILiteSynchronizer
    {
        private const int MaxPushRetryCount = 5;

        private readonly ILiteDatabase db;
        private readonly ILiteSyncConfiguration config;
        private readonly ICloudClient cloudClient;

        internal LiteSynchronizer(ILiteDatabase db, ILiteSyncConfiguration config, ICloudClient cloudClient)
        {
            this.cloudClient = cloudClient;
            this.config = config;
            this.db = db;
        }

        public async Task SynchronizeAsync(CancellationToken ct)
        {
            var cloudState = this.db.GetLocalCloudState();
            ct.ThrowIfCancellationRequested();

            var pull = await this.cloudClient.Pull(cloudState, ct);

            using (this.db.Engine.Locker.Exclusive())
            {
                var localChanges = this.GetLocalChanges(ct);

                if (!pull.HasChanges && !localChanges.HasChanges)
                {
                    if (pull.CloudStateChanged)
                    {
                        this.db.SaveLocalCloudState(pull.CloudState);
                    }

                    return;
                }

                await this.DoConfictHandlingSync(ct, localChanges, pull, cloudState);
            }
        }

        private async Task DoConfictHandlingSync(CancellationToken ct, Patch localChanges, PullResult pull, CloudState cloudState)
        {
            var pushSuccessful = false;
            var retryCounter = 0;

            using (var tx = this.db.BeginTrans())
            {
                while (!pushSuccessful)
                {
                    this.ResolveConflicts(localChanges, pull.RemotePatch, ct);

                    this.ApplyChangesToLocalDb(pull.RemotePatch);

                    try
                    {
                        await this.cloudClient.Push(cloudState, localChanges, ct);

                        pushSuccessful = true;
                    }
                    catch (LiteSyncConflictOccuredException ex)
                    {
                        if (retryCounter > MaxPushRetryCount)
                        {
                            throw new LiteSyncConflictRetryCountExceededException(MaxPushRetryCount, ex);
                        }

                        pull = await this.cloudClient.Pull(cloudState, ct);
                        retryCounter++;
                    }
                }

                this.db.SaveLocalCloudState(cloudState);
                tx.Commit();
            }
        }

        private void ApplyChangesToLocalDb(Patch remoteChanges) // TODO: Add ct?
        {
            var groupped = remoteChanges.Changes.GroupBy(x => x.EntityId.CollectionName, x => x);

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

                // This should move to the Conflict logic...

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
                        localChanges.ReplaceChange(conflict.EntityId, conflict.MergedEntity);
                        localChanges.ReplaceChange(conflict.EntityId, conflict.MergedEntity);
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

                patch.AddUpsertChanges(collectionName, dirtyEntities);

                var delCollection = this.db.GetCollection<DeletedEntity>(LiteSyncDatabase.DeletedEntitiesCollectionName);
                var deletedEntities = delCollection.FindAll();

                patch.AddDeleteChanges(deletedEntities);

                ct.ThrowIfCancellationRequested();
            }

            return patch;
        }
    }
}