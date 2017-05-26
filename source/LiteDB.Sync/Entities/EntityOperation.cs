namespace LiteDB.Sync.Entities
{
    internal class EntityOperation
    {
        public EntityOperationType OperationType { get; set; }

        public object Entity { get; set; }
    }
}