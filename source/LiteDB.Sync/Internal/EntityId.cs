using System;

namespace LiteDB.Sync.Internal
{
    public class EntityId
    {
        public EntityId(string collectionName, BsonValue id)
        {
            this.CollectionName = collectionName;
            this.Id = id;
        }

        public string CollectionName { get; }

        public BsonValue Id { get; }

        protected bool Equals(EntityId other)
        {
            return string.Equals(this.CollectionName, other.CollectionName, StringComparison.OrdinalIgnoreCase) 
                && this.Id.Equals(other.Id);
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
                return (StringComparer.OrdinalIgnoreCase.GetHashCode(this.CollectionName) * 397) ^ this.Id.GetHashCode();
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
            return string.Format("(Collection: {0}, Id: {1})", this.CollectionName, this.Id);
        }
    }
}