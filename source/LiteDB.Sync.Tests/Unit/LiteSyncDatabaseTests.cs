using System.IO;
using System.Linq;
using LiteDB.Sync.Exceptions;
using LiteDB.Sync.Internal;
using LiteDB.Sync.Tests.TestUtils;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace LiteDB.Sync.Tests.Unit
{
    [TestFixture]
    public class LiteSyncDatabaseTests
    {
        protected MemoryStream DbStream;
        protected LiteDatabase InnerDb;
        protected LiteSyncDatabase SyncDatabase;
        protected Mock<ILiteSyncService> ServiceMock;

        // TBA: Ensure indices - lazilly on any operation

        [SetUp]
        public void Setup()
        {
            this.DbStream = new MemoryStream();
            this.InnerDb = new LiteDatabase(this.DbStream);

            this.ServiceMock = new Mock<ILiteSyncService>();

            this.SyncDatabase = new LiteSyncDatabase(this.ServiceMock.Object, this.InnerDb);
        }

        [TearDown]
        public void TearDown()
        {
            this.SyncDatabase.Dispose();
            this.DbStream.Dispose();
        }

        public class WhenGettingCollectionByType : LiteSyncDatabaseTests
        {
            [Test]
            public void ShouldReturnNativeCollectionIfNotSynced()
            {
                this.ServiceMock.SetupGet(x => x.SyncedCollections).Returns(new string[0]);

                var collection = this.SyncDatabase.GetCollection<TestEntity>();
                
                Assert.IsInstanceOf<LiteCollection<TestEntity>>(collection);
                Assert.AreEqual(collection.Name, nameof(TestEntity));
            }

            [Test]
            public void ShouldReturnSyncCollectionIfSynced()
            {
                this.ServiceMock.SetupGet(x => x.SyncedCollections).Returns(new[]{ nameof(TestEntity) });

                var collection = this.SyncDatabase.GetCollection<TestEntity>();

                Assert.IsInstanceOf<LiteSyncCollection<TestEntity>>(collection);
                Assert.AreEqual(collection.Name, nameof(TestEntity));
            }

            [Test]
            public void ShouldThrowIfRegisteredAsSyncButTypeDoesNotImplementInterface()
            {
                this.ServiceMock.SetupGet(x => x.SyncedCollections).Returns(new[] { nameof(NonSyncableTestEntity) });

                Assert.Throws<LiteSyncInvalidEntityException>(() => this.SyncDatabase.GetCollection<NonSyncableTestEntity>());
            }

            [Test]
            public void ShouldCreateSyncIndices()
            {
                this.ServiceMock.SetupGet(x => x.SyncedCollections).Returns(new[] { nameof(TestEntity) });
                var collection = this.SyncDatabase.GetCollection<TestEntity>();

                var indices = collection.GetIndexes();

                Assert.IsTrue(indices.Any(x => x.Field == nameof(ILiteSyncEntity.RequiresSync)));
            }
        }

        public class WhenGettingCollectionByTypeAndName : LiteSyncDatabaseTests
        {
            private const string CollectionName = "Explicit";

            [Test]
            public void ShouldReturnNativeCollectionIfNotSynced()
            {
                this.ServiceMock.SetupGet(x => x.SyncedCollections).Returns(new string[0]);

                var collection = this.SyncDatabase.GetCollection<TestEntity>(CollectionName);

                Assert.IsInstanceOf<LiteCollection<TestEntity>>(collection);
                Assert.AreEqual(collection.Name, CollectionName);
            }

            [Test]
            public void ShouldReturnSyncCollectionIfSynced()
            {
                this.ServiceMock.SetupGet(x => x.SyncedCollections).Returns(new[] { CollectionName });

                var collection = this.SyncDatabase.GetCollection<TestEntity>(CollectionName);

                Assert.IsInstanceOf<LiteSyncCollection<TestEntity>>(collection);
                Assert.AreEqual(collection.Name, CollectionName);
            }

            [Test]
            public void ShouldThrowIfRegisteredAsSyncButTypeDoesNotImplementInterface()
            {
                this.ServiceMock.SetupGet(x => x.SyncedCollections).Returns(new[] { CollectionName });

                Assert.Throws<LiteSyncInvalidEntityException>(() => this.SyncDatabase.GetCollection<NonSyncableTestEntity>(CollectionName));
            }
        }

        public class WhenGettingBsonDocCollection : LiteSyncDatabaseTests
        {
            private const string CollectionName = "Explicit";

            [Test]
            public void ShouldReturnNativeCollectionIfNotSynced()
            {
                this.ServiceMock.SetupGet(x => x.SyncedCollections).Returns(new string[0]);

                var collection = this.SyncDatabase.GetCollection(CollectionName);

                Assert.IsInstanceOf<LiteCollection<BsonDocument>>(collection);
                Assert.AreEqual(collection.Name, CollectionName);
            }

            [Test]
            public void ShouldReturnSyncCollectionIfSynced()
            {
                this.ServiceMock.SetupGet(x => x.SyncedCollections).Returns(new[] { CollectionName });

                var collection = this.SyncDatabase.GetCollection(CollectionName);

                Assert.IsInstanceOf<LiteSyncCollection<BsonDocument>>(collection);
                Assert.AreEqual(collection.Name, CollectionName);
            }
        }

        public class WhenGettingDeletedEntitiesCollection : LiteSyncDatabaseTests
        {
            [Test]
            public void CollectionShouldHaveCorrectName()
            {
                var collection = this.SyncDatabase.GetDeletedEntitiesCollection();

                Assert.IsInstanceOf<LiteCollection<DeletedEntity>>(collection);
                Assert.AreEqual(collection.Name, LiteSyncDatabase.DeletedEntitiesCollectionName);
            }
        }
    }
}