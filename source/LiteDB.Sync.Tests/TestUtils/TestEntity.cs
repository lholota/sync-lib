namespace LiteDB.Sync.Tests.TestUtils
{
    public class TestEntity : ILiteSyncEntity
    {
        public TestEntity()
        {
        }

        public TestEntity(int id)
        {
            this.Id = id;
        }

        [BsonId]
        public int Id { get; set; }

        public string Text { get; set; }

        public bool RequiresSync { get; set; }
    }
}