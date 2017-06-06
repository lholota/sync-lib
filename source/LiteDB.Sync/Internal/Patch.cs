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
            var resultChanges = new Dictionary<EntityId, EntityChange>();

            foreach (var patch in patches)
            {
                foreach (var operation in patch)
                {
                    resultChanges[operation.EntityId] = operation;
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

        private readonly Dictionary<EntityId, EntityChange> changes;

        public Patch(IEnumerable<EntityChange> initialChanges)
        {
            this.changes = initialChanges.ToDictionary(
                x => x.EntityId,
                x => x);
        }

        internal Patch()
        {
            this.changes = new Dictionary<EntityId, EntityChange>();
        }

        private Patch(Dictionary<EntityId, EntityChange> initialChanges)
        {
            this.changes = initialChanges;
        }

        public bool HasChanges => this.changes.Count > 0;

        public string NextPatchId { get; set; }

        public IEnumerator<EntityChange> GetEnumerator()
        {
            return this.changes.Select(x => x.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        internal void RemoveChange(EntityId id)
        {
            this.changes.Remove(id);
        }

        internal void ReplaceEntity(EntityId id, BsonDocument doc)
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
                this.changes.Add(change.EntityId, change);
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
                this.changes.Add(change.EntityId, change);
            }
        }
    }
}