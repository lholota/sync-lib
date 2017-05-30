namespace LiteDB.Sync.Contract
{
    public class EntityOperation
    {
        internal string MergeId => $"{this.CollectionName}_{this.EntityId}";

        public string CollectionName { get; set; }

        public EntityOperationType OperationType { get; set; }

        public BsonValue EntityId { get; set; }

        public BsonDocument Entity { get; set; }
    }
}