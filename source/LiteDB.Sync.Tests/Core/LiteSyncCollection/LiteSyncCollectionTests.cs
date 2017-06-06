using System.IO;
using LiteDB.Sync.Internal;
using LiteDB.Sync.Tests.TestUtils;
using Moq;
using NUnit.Framework;

namespace LiteDB.Sync.Tests.Core.LiteSyncCollection
{
    [TestFixture]
    public partial class LiteSyncCollectionTests
    {
        private const string CollectionName = "SyncedCollection";

        protected MemoryStream DbStream;
        protected LiteSyncDatabase Db;
        protected LiteSyncConfiguration SyncConfig;
        protected ILiteCollection<TestEntity> SyncedCollection;
        protected ILiteCollection<TestEntity> NativeCollection;

        [SetUp]
        public void Setup()
        {
            this.DbStream = new MemoryStream();
            this.SyncConfig = Tools.MockExtensions.CreateMockedConfiguration(CollectionName);

            this.Db = new LiteSyncDatabase(this.SyncConfig, this.DbStream);

            this.SyncedCollection = this.Db.GetCollection<TestEntity>(CollectionName);
            this.NativeCollection = this.Db.InnerDb.GetCollection<TestEntity>(CollectionName);

            Assert.IsInstanceOf<LiteSyncCollection<TestEntity>>(this.SyncedCollection);
        }

        [TearDown]
        public void TearDown()
        {
            this.Db.Dispose();
            this.DbStream.Dispose();
        }

        protected void InsertDeletedEntity(int id)
        {
            this.Db.GetDeletedEntitiesCollection().Insert(new DeletedEntity(CollectionName, new BsonValue(id)));
        }
    }
}