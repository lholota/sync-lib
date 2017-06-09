using System;
using System.Threading;
using System.Threading.Tasks;
using LiteDB.Sync.Exceptions;

namespace LiteDB.Sync.Internal
{
    internal class Synchronizer : ISynchronizer
    {
        private const int MaxPushRetryCount = 3;

        private readonly ILiteSyncDatabase db;
        private readonly ILiteSyncConfiguration config;
        private readonly ICloudClient cloudClient;

        internal Synchronizer(ILiteSyncDatabase db, ILiteSyncConfiguration config, ICloudClient cloudClient)
        {
            this.cloudClient = cloudClient;
            this.config = config;
            this.db = db;
        }

        // TBA: Should delete deleted entities locally when applying changes + update RequiresSync = false
        public async Task SynchronizeAsync(CancellationToken ct)
        {
            var cloudState = this.db.GetLocalCloudState();
            ct.ThrowIfCancellationRequested();

            var pull = await this.cloudClient.PullAsync(cloudState, ct);

            using (this.db.LockExclusive())
            {
                var localChanges = this.db.GetLocalChanges(ct);

                if (!pull.HasChanges && !localChanges.HasChanges)
                {
                    if (pull.CloudStateChanged)
                    {
                        this.db.SaveLocalCloudState(pull.CloudState);
                    }

                    return;
                }

                await this.PerformSynchronizationHandleConflictsAsync(ct, localChanges, pull, cloudState);
            }
        }

        private async Task PerformSynchronizationHandleConflictsAsync(CancellationToken ct, Patch localChanges, PullResult pull, CloudState cloudState)
        {
            var retryCounter = 1;
            var pushSuccessful = false;
            var cloudStateToSave = pull.CloudState;

            using (var tx = this.db.BeginTrans())
            {
                while (!pushSuccessful)
                {
                    this.ResolveConflicts(localChanges, pull.RemotePatch, ct);

                    if (pull.HasChanges)
                    {
                        this.db.ApplyChanges(pull.RemotePatch, ct);

                        // Pull brought changes or push will overwrite the state with later patchId
                        cloudStateToSave = pull.CloudState;
                    }

                    if (!localChanges.HasChanges)
                    {
                        break;
                    }

                    try
                    {
                        cloudStateToSave = await this.cloudClient.PushAsync(cloudState, localChanges, ct);
                        pushSuccessful = true;
                    }
                    catch (LiteSyncConflictOccuredException ex)
                    {
                        if (retryCounter > MaxPushRetryCount)
                        {
                            throw new LiteSyncConflictRetryCountExceededException(MaxPushRetryCount, ex);
                        }

                        pull = await this.cloudClient.PullAsync(cloudState, ct);
                        retryCounter++;
                    }
                }

                if (cloudStateToSave != null)
                {
                    this.db.SaveLocalCloudState(cloudStateToSave);
                }

                tx.Commit();
            }
        }

        private void ResolveConflicts(Patch localChanges, Patch remoteChanges, CancellationToken ct)
        {
            if (!localChanges.HasChanges || !remoteChanges.HasChanges)
            {
                return;
            }

            var conflicts = Patch.GetConflicts(localChanges, remoteChanges);

            foreach (var conflict in conflicts)
            {
                ct.ThrowIfCancellationRequested();

                this.config.ConflictResolver.Resolve(conflict, this.db.Mapper);

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
                        remoteChanges.ReplaceChange(conflict.EntityId, conflict.MergedEntity);
                        break;
                }
            }
        }
    }
}