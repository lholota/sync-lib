namespace LiteDB.Sync.Tests.Tools
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

        public EntitySyncState SyncState { get; set; }
    }

    public class NonSyncedTestEntity
    {
        public NonSyncedTestEntity(int id)
        {
            this.Id = id;
        }

        [BsonId]
        public int Id { get; set; }

        public string Text { get; set; }
    }
}