using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LiteDB.Sync.Internal
{
    internal class Patch : IEnumerable<EntityChange>
    {
        #region Static

        public static Patch Combine(IList<Patch> patches)
        {
            var resultChanges = new Dictionary<GlobalEntityId, EntityChange>();

            foreach (var patch in patches)
            {
                foreach (var operation in patch)
                {
                    resultChanges[operation.GlobalId] = operation;
                }
            }

            return new Patch(resultChanges);
        }

        public static IList<LiteSyncConflict> GetConflicts(Patch localChanges, Patch remoteChanges)
        {
            var conflictingIds = localChanges.changes.Keys
                .Intersect(remoteChanges.changes.Keys)
                .ToArray();

            return conflictingIds
                .Select(x => new LiteSyncConflict(localChanges.changes[x], remoteChanges.changes[x]))
                .ToArray();
        }

        #endregion

        private readonly Dictionary<GlobalEntityId, EntityChange> changes;

        public Patch(IEnumerable<EntityChange> initialChanges)
        {
            this.changes = initialChanges.ToDictionary(
                x => x.GlobalId,
                x => x);
        }

        internal Patch()
        {
            this.changes = new Dictionary<GlobalEntityId, EntityChange>();
        }

        private Patch(Dictionary<GlobalEntityId, EntityChange> initialChanges)
        {
            this.changes = initialChanges;
        }

        public bool HasChanges => this.changes.Count > 0;

        public IEnumerator<EntityChange> GetEnumerator()
        {
            return this.changes.Select(x => x.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        internal void RemoveChange(GlobalEntityId id)
        {
            this.changes.Remove(id);
        }

        internal void ReplaceEntity(GlobalEntityId id, BsonDocument doc)
        {
            this.changes[id].Entity = doc;
        }

        internal void AddChanges(string collectionName, IEnumerable<BsonDocument> dirtyEntities)
        {
            if (dirtyEntities == null)
            {
                throw new ArgumentNullException(nameof(dirtyEntities));
            }

            foreach (var bsonDoc in dirtyEntities)
            {
                var change = new EntityChange(collectionName, bsonDoc["_id"], EntityChangeType.Upsert, bsonDoc);
                this.changes.Add(change.GlobalId, change);
            }
        }

        internal void AddDeletes(IEnumerable<DeletedEntity> deletedEntities)
        {
            if (deletedEntities == null)
            {
                throw new ArgumentNullException(nameof(deletedEntities));
            }

            foreach (var deleted in deletedEntities)
            {
                var change = new EntityChange(deleted.CollectionName, deleted.EntityId, EntityChangeType.Delete, null);
                this.changes.Add(change.GlobalId, change);
            }
        }
    }
}