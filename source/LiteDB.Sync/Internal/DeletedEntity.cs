namespace LiteDB.Sync.Internal
{
    internal class DeletedEntity
    {
        public DeletedEntity(string collectionName, BsonValue id)
        {
            this.EntityId = new EntityId(collectionName, id);
        }

        public DeletedEntity(EntityId entityId)
        {
            this.EntityId = entityId;
        }

        [BsonId]
        public EntityId EntityId { get; }
    }
}