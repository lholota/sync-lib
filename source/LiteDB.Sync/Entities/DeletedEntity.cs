namespace LiteDB.Sync.Entities
{
    public class DeletedEntity
    {
        public string CollectionName { get; set; }

        public BsonValue EntityId { get; set; }
    }
}