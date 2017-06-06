namespace LiteDB.Sync.Internal
{
    internal class DeletedEntity
    {
        // Parameterless ctor is required by BsonMapper
        public DeletedEntity()
        {
        }

        public DeletedEntity(string collectionName, BsonValue id)
        {
            this.EntityId = new EntityId(collectionName, id);
        }

        // This one will have to use the serialized version

        [BsonId]
        public EntityId EntityId { get; }
    }
}