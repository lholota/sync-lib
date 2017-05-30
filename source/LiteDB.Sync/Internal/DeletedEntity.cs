namespace LiteDB.Sync.Internal
{
    internal class DeletedEntity
    {
        public static BsonValue CreateId(string collectionName, BsonValue id)
        {
            return $"{collectionName}_{id}"; // TODO: Use hashing
        }

        public DeletedEntity()
        {
        }

        public DeletedEntity(string collectionName, BsonValue entityId)
        {
            this.CollectionName = collectionName;
            this.EntityId = entityId;
        }

        [BsonId]
        public string Id
        {
            get => $"{this.CollectionName}_{this.EntityId}";
        }

        public string CollectionName { get; set; }

        public BsonValue EntityId { get; set; }
    }
}