using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace LiteDB.Sync.Internal
{
    public class EntityChange
    {
        public EntityChange(string collectionName, BsonValue id, EntityChangeType changeType, BsonDocument entity)
            : this(new EntityId(collectionName, id), changeType, entity)
        {
        }

        public EntityChange(EntityId entityId)
        {
            this.EntityId = entityId;
            this.ChangeType = EntityChangeType.Delete;
        }

        public EntityChange(EntityId entityId, EntityChangeType changeType, BsonDocument entity)
        {
            if (changeType == EntityChangeType.Upsert && entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null if the change type is Upsert.");
            }

            this.EntityId = entityId;
            this.ChangeType = changeType;
            this.Entity = entity;
        }

        [JsonConstructor]
        internal EntityChange(EntityId entityId, EntityChangeType changeType, IDictionary<string, object> rawValues)
        {
            if (changeType == EntityChangeType.Upsert && rawValues == null)
            {
                throw new ArgumentNullException(nameof(rawValues), "Entity cannot be null if the change type is Upsert.");
            }

            this.EntityId = entityId;
            this.ChangeType = changeType;

            if (rawValues != null)
            {
                this.Entity = new BsonDocument(rawValues.ToDictionary(x => x.Key, x => new BsonValue(x.Value)));
            }
        }

        public EntityId EntityId { get; }

        public EntityChangeType ChangeType { get; }

        [JsonIgnore]
        public BsonDocument Entity { get; internal set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal IDictionary<string, object> RawValues
        {
            get
            {
                return this.Entity?.ToDictionary(
                    x => x.Key,
                    x => x.Value.RawValue);
            }
        }

        internal void Apply(ILiteCollection<BsonDocument> collection)
        {
            if (this.ChangeType == EntityChangeType.Delete)
            {
                collection.Delete(this.EntityId.BsonId);
            }
            else
            {
                collection.Upsert(this.EntityId.BsonId, this.Entity);
            }
        }
    }
}