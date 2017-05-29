namespace LiteDB.Sync.Internal
{
    internal class DeletedEntity
    {
        public string CollectionName { get; set; }

        public BsonValue EntityId { get; set; }

        public int ChangeTime { get; set; }
    }
}