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
            var resultChanges = new Dictionary<EntityId, EntityChangeBase>();

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
                .Where(x => x.HasDifferences())
                .ToArray();
        }

        #endregion

        private readonly Dictionary<EntityId, EntityChangeBase> changes;

        internal Patch()
        {
            this.changes = new Dictionary<EntityId, EntityChangeBase>();
        }

        [JsonConstructor]
        internal Patch([JsonProperty(nameof(Changes))] IEnumerable<EntityChangeBase> initialChanges)
        {
            this.changes = initialChanges.ToDictionary(
                x => x.EntityId,
                x => x);
        }

        private Patch(Dictionary<EntityId, EntityChangeBase> initialChanges)
        {
            this.changes = initialChanges;
        }

        [JsonIgnore]
        public bool HasChanges => this.changes.Count > 0;

        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
        public IEnumerable<EntityChangeBase> Changes => this.changes.Select(x => x.Value).ToList();

        public string NextPatchId { get; set; }

        internal void RemoveChange(EntityId id)
        {
            if (!this.changes.Remove(id))
            {
                throw new KeyNotFoundException($"No change with EntityId {id} could be found.");
            }
        }

        internal void ReplaceChange(EntityId id, BsonDocument doc)
        {
            var change = new UpsertEntityChange(id, doc);

            if (!this.changes.ContainsKey(id))
            {
                throw new KeyNotFoundException($"No change with EntityId {id} could be found.");
            }

            this.changes[id] = change;
        }

        internal void AddUpsertChanges(string collectionName, IEnumerable<BsonDocument> dirtyEntities)
        {
            if (dirtyEntities == null)
            {
                throw new ArgumentNullException(nameof(dirtyEntities));
            }

            foreach (var bsonDoc in dirtyEntities)
            {
                var entityId = new EntityId(collectionName, bsonDoc["_id"]);
                var change = new UpsertEntityChange(entityId, bsonDoc);

                this.changes.Add(change.EntityId, change);
            }
        }

        internal void AddDeleteChanges(IEnumerable<DeletedEntity> deletedEntities)
        {
            if (deletedEntities == null)
            {
                throw new ArgumentNullException(nameof(deletedEntities));
            }

            foreach (var deleted in deletedEntities)
            {
                var change = new DeleteEntityChange(deleted.EntityId);
                this.changes.Add(change.EntityId, change);
            }
        }
    }
}