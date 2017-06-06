using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace LiteDB.Sync.Internal
{
    internal class Patch
    {
        #region Static

        public static Patch Combine(IList<Patch> patches)
        {
            var resultChanges = new Dictionary<EntityId, EntityChange>();

            foreach (var patch in patches)
            {
                foreach (var operation in patch.Changes)
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

        internal Patch()
        {
            this.changes = new Dictionary<EntityId, EntityChange>();
        }

        [JsonConstructor]
        internal Patch([JsonProperty(nameof(Changes))] IEnumerable<EntityChange> initialChanges)
        {
            this.changes = initialChanges.ToDictionary(
                x => x.EntityId,
                x => x);
        }

        private Patch(Dictionary<EntityId, EntityChange> initialChanges)
        {
            this.changes = initialChanges;
        }

        [JsonIgnore]
        public bool HasChanges => this.changes.Count > 0;

        [JsonProperty]
        public IEnumerable<EntityChange> Changes => this.changes.Select(x => x.Value);

        public string NextPatchId { get; set; }

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
                var entityId = new EntityId(collectionName, bsonDoc["_id"]);
                var change = new EntityChange(entityId, EntityChangeType.Upsert, bsonDoc);
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
                var change = new EntityChange(deleted.EntityId);
                this.changes.Add(change.EntityId, change);
            }
        }
    }
}