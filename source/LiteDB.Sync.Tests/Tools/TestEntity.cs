namespace LiteDB.Sync.Tests.Tools
{
    public class TestEntity : ILiteSyncEntity
    {
        [BsonId]
        public int IdProp { get; set; }

        public string StringProp { get; set; }

        public bool RequiresSync { get; set; }

        public int LastChangeTime { get; set; }

        public BsonValue BsonId => new BsonValue(this.IdProp);
    }
}