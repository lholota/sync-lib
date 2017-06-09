using System;
using Newtonsoft.Json;

namespace LiteDB.Sync.Internal
{
    public class EntityId
    { 
        internal static EntityId FromString(string str)
        {
            return JsonSerialization.DeserializeFromString<EntityId>(str);
        }

        internal static string GetEntityIdString(string collectionName, BsonValue id)
        {
            return JsonSerialization.SerializeToString(new EntityId(collectionName, id));
        }

        // For BsonMapper
        //public EntityId()
        //{
        //}

        public EntityId(string collectionName, BsonValue id)
        {
            this.Validate(collectionName, id);

            this.CollectionName = collectionName;
            this.BsonId = id;
        }

        [JsonConstructor]
        public EntityId(string collectionName, object id)
        {
            this.CollectionName = collectionName;
            this.BsonId = new BsonValue(id);
        }

        public string CollectionName { get; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
        public object Id => this.BsonId.RawValue;

        [JsonIgnore]
        public BsonValue BsonId { get; }

        protected bool Equals(EntityId other)
        {
            return string.Equals(this.CollectionName, other.CollectionName, StringComparison.OrdinalIgnoreCase) 
                && this.BsonId == other.BsonId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;

            return this.Equals((EntityId)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (StringComparer.OrdinalIgnoreCase.GetHashCode(this.CollectionName) * 397) ^ this.BsonId.GetHashCode();
            }
        }

        public static bool operator ==(EntityId left, EntityId right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EntityId left, EntityId right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return JsonSerialization.SerializeToString(this);
        }

        private void Validate(string collectionName, BsonValue id)
        {
            if (string.IsNullOrEmpty(collectionName))
            {
                throw new ArgumentNullException(nameof(collectionName));
            }

            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (id.Type == BsonType.Null)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "The id cannot be BsonValue.Null.");
            }

            if (id.Type == BsonType.MinValue || id.Type == BsonType.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "The id cannot be BsonValue.MinValue/MaxValue");
            }
        }
    }
}