using System;
using System.IO;
using System.Linq;
using LiteDB.Sync.Exceptions;
using LiteDB.Sync.Internal;
using LiteDB.Sync.Tests.TestUtils;
using LiteDB.Sync.Tests.Tools.Entities;
using Moq;
using NUnit.Framework;

namespace LiteDB.Sync.Tests.Core
{
    [TestFixture]
    public class LiteSyncDatabaseTests
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

        public class WhenGettingCollectionByType : LiteSyncDatabaseTests
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

            [Test]
            public void ShouldThrowIfTryingToGetDeletedEntities()
            {
                Assert.Throws<ArgumentException>(() => this.SyncDatabase.GetCollection<LiteSync_Deleted>());
            }

            // ReSharper disable once ClassNeverInstantiated.Local
            // ReSharper disable once InconsistentNaming
            private class LiteSync_Deleted { }
        }

        public class WhenGettingCollectionByTypeAndName : LiteSyncDatabaseTests
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

            [Test]
            public void ShouldThrowIfTryingToGetDeletedEntities()
            {
                Assert.Throws<ArgumentException>(() => this.SyncDatabase.GetCollection<TestEntity>(LiteSyncDatabase.DeletedEntitiesCollectionName));
            }
        }

        public class WhenGettingBsonDocCollection : LiteSyncDatabaseTests
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

            [Test]
            public void ShouldThrowIfTryingToGetDeletedEntities()
            {
                Assert.Throws<ArgumentException>(() => this.SyncDatabase.GetCollection(LiteSyncDatabase.DeletedEntitiesCollectionName));
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

        public class WhenGettingCollectionNames : LiteSyncDatabaseTests
        {
            [Test]
            public void ResultShouldNotContainDeletedEntities()
            {
                this.CreateDeletedEntitiesCollection();

                Assert.IsFalse(this.SyncDatabase.GetCollectionNames().Contains(LiteSyncDatabase.DeletedEntitiesCollectionName));
            }
        }

        public class WhenRenamingCollection : LiteSyncDatabaseTests
        {
            [Test]
            public void ShouldThrowOnDeletedEntitiesInOldName()
            {
                Assert.Throws<ArgumentException>(() => this.SyncDatabase.RenameCollection(LiteSyncDatabase.DeletedEntitiesCollectionName, "Some other"));
            }

            [Test]
            public void ShouldThrowOnDeletedEntitiesInNewName()
            {
                Assert.Throws<ArgumentException>(() => this.SyncDatabase.RenameCollection("Source", LiteSyncDatabase.DeletedEntitiesCollectionName));
            }

            [Test]
            public void ShouldRenameCollection()
            {
                this.SyncDatabase.GetCollection<TestEntity>("Name").Insert(new TestEntity(1));

                this.SyncDatabase.RenameCollection("Name", "Another");

                var cnt = this.SyncDatabase.GetCollection<TestEntity>("Another").Count();
                Assert.AreEqual(1, cnt);
            }
        }

        public class WhenGettingCollectionExists : LiteSyncDatabaseTests
        {
            [Test]
            public void ShouldReturnFalseOnDeletedEntities()
            {
                this.CreateDeletedEntitiesCollection();

                Assert.IsFalse(this.SyncDatabase.CollectionExists(LiteSyncDatabase.DeletedEntitiesCollectionName));
            }

            [Test]
            public void ShouldReturnFalseIfNotExists()
            {
                Assert.IsFalse(this.SyncDatabase.CollectionExists("NotExisting"));
            }

            [Test]
            public void ShouldReturnTrueIfExists()
            {
                this.SyncDatabase.GetCollection<TestEntity>().Insert(new TestEntity(1));

                Assert.IsTrue(this.SyncDatabase.CollectionExists(nameof(TestEntity)));
            }
        }

        public class WhenDroppingCollection : LiteSyncDatabaseTests
        {
            [Test]
            public void ShouldThrowOnDeletedEntities()
            {
                Assert.Throws<ArgumentException>(() => this.SyncDatabase.DropCollection(LiteSyncDatabase.DeletedEntitiesCollectionName));
            }

            [Test]
            public void ShouldDropCollection()
            {
                Assert.IsFalse(this.SyncDatabase.CollectionExists(nameof(TestEntity)));

                this.SyncDatabase.GetCollection<TestEntity>().Insert(new TestEntity(1));
                Assert.IsTrue(this.SyncDatabase.CollectionExists(nameof(TestEntity)));

                this.SyncDatabase.DropCollection(nameof(TestEntity));
                Assert.IsFalse(this.SyncDatabase.CollectionExists(nameof(TestEntity)));
            }
        }

        // TBA: Drop

        public class WhenSynchronizing : LiteSyncDatabaseTests
        {
            /*
             * Should not execute sync if it's already running
             * Should raise events
             * Should ensure indices on all collections
             */

            [Test]
            public void Placeholder()
            {
                throw new NotImplementedException();
            }
        }

        protected void CreateDeletedEntitiesCollection()
        {
            this.SyncConfigMock.SetupGet(x => x.SyncedCollections).Returns(new[] { nameof(TestEntity) });

            var coll = this.SyncDatabase.GetCollection<TestEntity>();
            coll.Insert(new TestEntity(1));
            coll.Delete(1);

            Assert.IsTrue(this.SyncDatabase.InnerDb.CollectionExists(LiteSyncDatabase.DeletedEntitiesCollectionName));
        }
    }
}