namespace LiteDB.Sync.Entities
{
    using System;

    public class DirtyEntity
    {
        // TBA: Add bson id

        public Guid TransactionId { get; set; }

        public string CollectionName { get; set; }

        public BsonValue EntityId { get; set; }
    }
}