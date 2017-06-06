namespace LiteDB.Sync.Tests.Tools.Entities
{
    public class NonSyncTestEntity
    {
        public NonSyncTestEntity(int id)
        {
            this.Id = id;
        }

        [BsonId]
        public int Id { get; set; }

        public string Text { get; set; }
    }
}