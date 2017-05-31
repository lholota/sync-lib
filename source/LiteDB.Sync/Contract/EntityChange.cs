using System;

namespace LiteDB.Sync.Contract
{
    public class EntityChange
    {
        public EntityChange(string collectionName, BsonValue id, EntityChangeType changeType, BsonDocument entity)
        {
            if (changeType == EntityChangeType.Upsert && entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null if the change type is Upsert.");
            }

            this.GlobalEntityId = new GlobalEntityId(collectionName, id);
            this.ChangeType = changeType;
            this.Entity = entity;
        }

        internal GlobalEntityId GlobalEntityId { get; }

        public EntityChangeType ChangeType { get; }

        public BsonDocument Entity { get; internal set; }
    }
}