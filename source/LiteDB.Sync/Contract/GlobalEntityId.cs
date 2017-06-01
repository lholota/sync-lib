using System;

namespace LiteDB.Sync.Contract
{
    public class GlobalEntityId
    {
        public GlobalEntityId(string collectionName, BsonValue entityId)
        {
            this.CollectionName = collectionName;
            this.EntityId = entityId;
        }

        public string CollectionName { get; }

        public BsonValue EntityId { get; }

        protected bool Equals(GlobalEntityId other)
        {
            return string.Equals(this.CollectionName, other.CollectionName, StringComparison.OrdinalIgnoreCase) 
                && this.EntityId.Equals(other.EntityId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;

            return this.Equals((GlobalEntityId)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (StringComparer.OrdinalIgnoreCase.GetHashCode(this.CollectionName) * 397) ^ this.EntityId.GetHashCode();
            }
        }

        public static bool operator ==(GlobalEntityId left, GlobalEntityId right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(GlobalEntityId left, GlobalEntityId right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return string.Format("(Collection: {0}, Id: {1})", this.CollectionName, this.EntityId);
        }
    }
}