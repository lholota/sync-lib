using System;

namespace LiteDB.Sync.Internal
{
    public class EntityChange
    {
        public EntityChange(string collectionName, BsonValue id, EntityChangeType changeType, BsonDocument entity)
        {
            if (changeType == EntityChangeType.Upsert && entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null if the change type is Upsert.");
            }

            this.EntityId = new EntityId(collectionName, id);
            this.ChangeType = changeType;
            this.Entity = entity;
        }

        internal EntityId EntityId { get; }

        public EntityChangeType ChangeType { get; }

        public BsonDocument Entity { get; internal set; }

        internal void Apply(ILiteCollection<BsonDocument> collection)
        {
            if (this.ChangeType == EntityChangeType.Delete)
            {
                collection.Delete(this.EntityId.Id);
            }
            else
            {
                collection.Upsert(this.EntityId.Id, this.Entity);
            }
        }
    }
}