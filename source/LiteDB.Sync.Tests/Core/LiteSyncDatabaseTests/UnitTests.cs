using System;
using System.IO;
using System.Linq;
using LiteDB.Sync.Exceptions;
using LiteDB.Sync.Internal;
using LiteDB.Sync.Tests.TestUtils;
using LiteDB.Sync.Tests.Tools.Entities;
using Moq;
using NUnit.Framework;

namespace LiteDB.Sync.Tests.Core.LiteSyncDatabaseTests
{
    [TestFixture]
    public class UnitTests
    {
        protected MemoryStream DbStream;
        protected LiteSyncDatabase SyncDatabase;
        protected Mock<ILiteSyncConfiguration> SyncConfigMock;

        // TBA: Ensure indices - lazilly on any operation

        [SetUp]
        public void Setup()
        {
            this.DbStream = new MemoryStream();
            this.SyncConfigMock = new Mock<ILiteSyncConfiguration>();

            this.SyncDatabase = new LiteSyncDatabase(this.SyncConfigMock.Object, this.DbStream);
        }

        [TearDown]
        public void TearDown()
        {
            this.SyncDatabase.Dispose();
            this.DbStream.Dispose();
        }

        public class WhenGettingCollectionByType : UnitTests
        {
            [Test]
            public void ShouldReturnNativeCollectionIfNotSynced()
            {
                this.SyncConfigMock.SetupGet(x => x.SyncedCollections).Returns(new string[0]);

                var collection = this.SyncDatabase.GetCollection<TestEntity>();
                
                Assert.IsInstanceOf<LiteCollection<TestEntity>>(collection);
                Assert.AreEqual(collection.Name, nameof(TestEntity));
            }

            [Test]
            public void ShouldReturnSyncCollectionIfSynced()
            {
                this.SyncConfigMock.SetupGet(x => x.SyncedCollections).Returns(new[]{ nameof(TestEntity) });

                var collection = this.SyncDatabase.GetCollection<TestEntity>();

                Assert.IsInstanceOf<LiteSyncCollection<TestEntity>>(collection);
                Assert.AreEqual(collection.Name, nameof(TestEntity));
            }

            [Test]
            public void ShouldThrowIfRegisteredAsSyncButTypeDoesNotImplementInterface()
            {
                this.SyncConfigMock.SetupGet(x => x.SyncedCollections).Returns(new[] { nameof(NonSyncTestEntity) });

                Assert.Throws<LiteSyncInvalidEntityException>(() => this.SyncDatabase.GetCollection<NonSyncTestEntity>());
            }
        }

        public class WhenGettingCollectionByTypeAndName : UnitTests
        {
            private const string CollectionName = "Explicit";

            [Test]
            public void ShouldReturnNativeCollectionIfNotSynced()
            {
                this.SyncConfigMock.SetupGet(x => x.SyncedCollections).Returns(new string[0]);

                var collection = this.SyncDatabase.GetCollection<TestEntity>(CollectionName);

                Assert.IsInstanceOf<LiteCollection<TestEntity>>(collection);
                Assert.AreEqual(collection.Name, CollectionName);
            }

            [Test]
            public void ShouldReturnSyncCollectionIfSynced()
            {
                this.SyncConfigMock.SetupGet(x => x.SyncedCollections).Returns(new[] { CollectionName });

                var collection = this.SyncDatabase.GetCollection<TestEntity>(CollectionName);

                Assert.IsInstanceOf<LiteSyncCollection<TestEntity>>(collection);
                Assert.AreEqual(collection.Name, CollectionName);
            }

            [Test]
            public void ShouldThrowIfRegisteredAsSyncButTypeDoesNotImplementInterface()
            {
                this.SyncConfigMock.SetupGet(x => x.SyncedCollections).Returns(new[] { CollectionName });

                Assert.Throws<LiteSyncInvalidEntityException>(() => this.SyncDatabase.GetCollection<NonSyncTestEntity>(CollectionName));
            }
        }

        public class WhenGettingBsonDocCollection : UnitTests
        {
            private const string CollectionName = "Explicit";

            [Test]
            public void ShouldReturnNativeCollectionIfNotSynced()
            {
                this.SyncConfigMock.SetupGet(x => x.SyncedCollections).Returns(new string[0]);

                var collection = this.SyncDatabase.GetCollection(CollectionName);

                Assert.IsInstanceOf<LiteCollection<BsonDocument>>(collection);
                Assert.AreEqual(collection.Name, CollectionName);
            }

            [Test]
            public void ShouldReturnSyncCollectionIfSynced()
            {
                this.SyncConfigMock.SetupGet(x => x.SyncedCollections).Returns(new[] { CollectionName });

                var collection = this.SyncDatabase.GetCollection(CollectionName);

                Assert.IsInstanceOf<LiteSyncCollection<BsonDocument>>(collection);
                Assert.AreEqual(collection.Name, CollectionName);
            }
        }

        public class WhenGettingDeletedEntitiesCollection : UnitTests
        {
            [Test]
            public void CollectionShouldHaveCorrectName()
            {
                var collection = this.SyncDatabase.GetDeletedEntitiesCollection();

                Assert.IsInstanceOf<LiteCollection<DeletedEntity>>(collection);
                Assert.AreEqual(collection.Name, LiteSyncDatabase.DeletedEntitiesCollectionName);
            }
        }

        public class WhenSynchronizing : UnitTests
        {
            [Test]
            public void Placeholder()
            {
                throw new NotImplementedException();
            }
        }
    }
}