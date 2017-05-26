namespace LiteDB.Sync.Entities
{
    public class EntityOperation
    {
        public EntityOperationType OperationType { get; set; }

        public object Entity { get; set; }
    }
}