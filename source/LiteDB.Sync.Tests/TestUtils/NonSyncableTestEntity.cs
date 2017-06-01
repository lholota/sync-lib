namespace LiteDB.Sync.Tests.TestUtils {
    public class NonSyncableTestEntity
    {
        public NonSyncableTestEntity(int id)
        {
            this.Id = id;
        }

        [BsonId]
        public int Id { get; set; }

        public string Text { get; set; }
    }
}