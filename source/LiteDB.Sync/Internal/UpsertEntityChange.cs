using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace LiteDB.Sync.Internal
{
    public class UpsertEntityChange : EntityChangeBase
    {
        public UpsertEntityChange(EntityId entityId, BsonDocument entity)
            : base(entityId)
        {
            this.Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        }

        [JsonConstructor]
        private UpsertEntityChange(EntityId entityId, IDictionary<string, object> rawValues)
            : base(entityId)
        {
            if (rawValues != null)
            {
                this.Entity = new BsonDocument(rawValues.ToDictionary(x => x.Key, x => new BsonValue(x.Value)));
            }
        }

        [JsonIgnore]
        public BsonDocument Entity { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        private IDictionary<string, object> RawValues
        {
            get
            {
                return this.Entity?.ToDictionary(
                    x => x.Key,
                    x => x.Value.RawValue);
            }
        }

        internal override void Apply(ILiteCollection<BsonDocument> collection)
        {
            collection.Upsert(this.EntityId.BsonId, this.Entity);
        }
    }
}