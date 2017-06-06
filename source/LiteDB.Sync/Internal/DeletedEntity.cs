namespace LiteDB.Sync.Internal
{
    internal class DeletedEntity
    {
        public DeletedEntity(string collectionName, BsonValue id)
        {
            this.EntityId = new EntityId(collectionName, id);
        }

        [BsonId]
        public EntityId EntityId { get; }
    }
}