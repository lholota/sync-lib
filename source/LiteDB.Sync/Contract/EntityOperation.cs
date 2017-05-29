namespace LiteDB.Sync.Contract
{
    public class EntityOperation
    {
        public string CollectionName { get; set; }

        public EntityOperationType OperationType { get; set; }

        public BsonValue EntityId { get; set; }

        public BsonDocument Entity { get; set; }
    }
}