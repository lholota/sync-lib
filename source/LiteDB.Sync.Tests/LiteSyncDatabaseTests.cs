using System.IO;
using LiteDB.Sync.Tests.Tools;
using Moq;
using NUnit.Framework;

namespace LiteDB.Sync.Tests
{
    [TestFixture]
    public class LiteSyncDatabaseTests
    {
        protected MemoryStream NativeDbStream;
        protected LiteDatabase NativeDb;
        protected LiteSyncDatabase Db;
        protected Mock<ILiteSyncService> SyncServiceMock;

        [SetUp]
        public void Setup()
        {
            this.NativeDbStream = new MemoryStream();
            this.NativeDb = new LiteDatabase(this.NativeDbStream);

            this.SyncServiceMock = new Mock<ILiteSyncService>();

            this.Db = new LiteSyncDatabase(
                this.SyncServiceMock.Object,
                this.NativeDb);
        }

        [TearDown]
        public void TearDown()
        {
            this.Db.Dispose();
            this.NativeDbStream.Dispose();
        }

        public class WhenGettingTypedCollectionWithName : LiteSyncDatabaseTests
        {
            [Test]
            public void ShouldReturnNativeCollectionForNonSynced()
            {
                this.SyncServiceMock.SetupGet(x => x.SyncedCollections).Returns(new string[0]);

                var collection = this.Db.GetCollection<TestEntity>("Dummy");

                Assert.IsInstanceOf<LiteCollection<TestEntity>>(collection);
            }

            [Test]
            public void ShouldReturnSyncCollectionForSynced()
            {
                this.SyncServiceMock.SetupGet(x => x.SyncedCollections).Returns(new[]{ "Dummy" });

                var collection = this.Db.GetCollection<TestEntity>("Dummy");

                Assert.IsInstanceOf<LiteSyncCollection<TestEntity>>(collection);
            }

            [Test]
            public void ShouldThrowWhenGettingNonSyncedEntityTypeOnSyncedCollection()
            {
                this.SyncServiceMock.SetupGet(x => x.SyncedCollections).Returns(new[] { "Dummy" });

                var ex = Assert.Throws<LiteSyncException>(() => this.Db.GetCollection<NonSyncedTestEntity>("Dummy"));

                Assert.AreEqual(LiteSyncException.ErrorCodes.EntityDoesntImplementInterfaceErrorCode, ex.ErrorCode);
            }
        }

        public class WhenGettingTypedCollectionWithAutoName : LiteSyncDatabaseTests
        {
            [Test]
            public void ShouldReturnNativeCollectionForNonSynced()
            {
                this.SyncServiceMock.SetupGet(x => x.SyncedCollections).Returns(new string[0]);

                var collection = this.Db.GetCollection<TestEntity>();

                Assert.IsInstanceOf<LiteCollection<TestEntity>>(collection);
            }

            [Test]
            public void ShouldReturnSyncCollectionForSynced()
            {
                this.SyncServiceMock.SetupGet(x => x.SyncedCollections).Returns(new[] { nameof(TestEntity) });

                var collection = this.Db.GetCollection<TestEntity>();

                Assert.IsInstanceOf<LiteSyncCollection<TestEntity>>(collection);
            }

            [Test]
            public void ShouldThrowWhenGettingNonSyncedEntityTypeOnSyncedCollection()
            {
                this.SyncServiceMock.SetupGet(x => x.SyncedCollections).Returns(new[] { nameof(NonSyncedTestEntity) });

                var ex = Assert.Throws<LiteSyncException>(() => this.Db.GetCollection<NonSyncedTestEntity>());

                Assert.AreEqual(LiteSyncException.ErrorCodes.EntityDoesntImplementInterfaceErrorCode, ex.ErrorCode);
            }
        }

        public class WhenGettingBsonCollection : LiteSyncDatabaseTests
        {
            [Test]
            public void ShouldReturnNativeCollectionForNonSynced()
            {
                this.SyncServiceMock.SetupGet(x => x.SyncedCollections).Returns(new string[0]);

                var collection = this.Db.GetCollection("Dummy");

                Assert.IsInstanceOf<LiteCollection<BsonDocument>>(collection);
            }

            [Test]
            public void ShouldReturnSyncCollectionForSynced()
            {
                this.SyncServiceMock.SetupGet(x => x.SyncedCollections).Returns(new[] { "Dummy" });

                var collection = this.Db.GetCollection("Dummy");

                Assert.IsInstanceOf<LiteSyncCollection<BsonDocument>>(collection);
            }
        }
    }
}