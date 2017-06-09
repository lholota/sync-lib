using System;
using LiteDB.Sync.Internal;

namespace LiteDB.Sync
{
    public class LiteSyncConflict
    {
        internal LiteSyncConflict(EntityChangeBase localChange, EntityChangeBase remoteChange)
        {
            if (localChange?.EntityId != remoteChange?.EntityId)
            {
                throw new ArgumentException("The conflicting changes must be both with the same EntityId.", nameof(localChange));
            }

            if (localChange is DeleteEntityChange && remoteChange is DeleteEntityChange)
            {
                // throw new ArgumentException("Can");
            }

            this.LocalChange = localChange;
            this.RemoteChange = remoteChange;
            this.Resolution = ConflictResolution.None;
        }

        public EntityId EntityId => this.LocalChange.EntityId;

        public EntityChangeBase LocalChange { get; }

        public EntityChangeBase RemoteChange { get; }

        internal ConflictResolution Resolution { get; private set; }

        internal BsonDocument MergedEntity { get; private set; }

        public void ResolveKeepLocal()
        {
            this.Resolution = ConflictResolution.KeepLocal;
        }

        public void ResolveKeepRemote()
        {
            this.Resolution = ConflictResolution.KeepRemote;
        }

        public void ResolveMerged(BsonDocument mergedEntity)
        {
            if (mergedEntity.GetId() != this.EntityId.BsonId)
            {
                throw new ArgumentException("The merged entity must have the same id as the conflicting changes.");
            }

            this.Resolution = ConflictResolution.Merge;
            this.MergedEntity = mergedEntity;
        }

        internal bool HasDifferences()
        {
            if (this.LocalChange.GetType() != this.RemoteChange.GetType())
            {
                return true;
            }

            if (this.LocalChange is DeleteEntityChange)
            {
                return false;
            }

            var localUpsert = (UpsertEntityChange)this.LocalChange;
            var remoteUpsert = (UpsertEntityChange)this.RemoteChange;

            return localUpsert.Entity.CompareTo(remoteUpsert.Entity) != 0;
        }

        // TODO: Add mapping from bson to entity

        internal enum ConflictResolution
        {
            None,
            KeepLocal,
            KeepRemote,
            Merge
        }
    }
}