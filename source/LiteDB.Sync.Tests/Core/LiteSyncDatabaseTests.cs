using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        internal Mock<IFactory> FactoryMock;

        protected MemoryStream DbStream;
        protected LiteSyncDatabase SyncDatabase;
        protected Mock<ILiteSyncConfiguration> SyncConfigMock;

        [SetUp]
        public virtual void Setup()
        {
            this.DbStream = new MemoryStream();
            this.FactoryMock = new Mock<IFactory>();

            this.SyncConfigMock = new Mock<ILiteSyncConfiguration>();
            this.SyncConfigMock.SetupGet(x => x.IsValid).Returns(true);

            this.SyncDatabase = new LiteSyncDatabase(
                this.SyncConfigMock.Object,
                this.DbStream,
                this.FactoryMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            this.SyncDatabase.Dispose();
            this.DbStream.Dispose();
        }

        [TestFixture]
        public class WhenCreating
        {
            [Test]
            public void ShouldThrowIsSyncConfigNull()
            {
                using (var ms = new MemoryStream())
                {
                    Assert.Throws<ArgumentNullException>(() => new LiteSyncDatabase(null, ms));
                }
            }

            [Test]
            public void ShouldThrowIsSyncConfigInvalid()
            {
                using (var ms = new MemoryStream())
                {
                    var configMock = new Mock<ILiteSyncConfiguration>();

                    configMock.SetupGet(x => x.IsValid).Returns(false);

                    Assert.Throws<ArgumentException>(() => new LiteSyncDatabase(configMock.Object, ms));
                }
            }
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
                this.SyncConfigMock.SetupGet(x => x.SyncedCollections).Returns(new[] { nameof(TestEntity) });

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

        public class WhenSynchronizing : LiteSyncDatabaseTests
        {
            private Mock<ISynchronizer> synchronizerMock;

            public override void Setup()
            {
                base.Setup();

                this.synchronizerMock = new Mock<ISynchronizer>();

                this.FactoryMock.Setup(x => x.CreateSynchronizer(It.IsAny<ILiteSyncDatabase>(),
                                                                 It.IsAny<ILiteSyncConfiguration>(),
                                                                 It.IsAny<ICloudClient>()))
                                .Returns(this.synchronizerMock.Object);
            }

            [Test]
            public async Task ShouldStartSynchronization()
            {
                this.synchronizerMock.Setup(x => x.SynchronizeAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(0));

                await this.SyncDatabase.SynchronizeAsync();

                this.synchronizerMock.VerifyAll();
            }

            [Test]
            public void ShouldNotStartSyncIfAlreadyInProgress()
            {
                var evt = new ManualResetEvent(false);

                this.synchronizerMock
                    .Setup(x => x.SynchronizeAsync(It.IsAny<CancellationToken>()))
                    .Callback<CancellationToken>(x => evt.WaitOne())
                    .Returns(Task.FromResult(0));

                var firstSyncExecTask = this.SyncDatabase.SynchronizeAsync();
                var secondSyncExecTask = this.SyncDatabase.SynchronizeAsync();

                Assert.IsNotNull(secondSyncExecTask);

                evt.Set();

                secondSyncExecTask.Wait();
                firstSyncExecTask.Wait();

                this.synchronizerMock.Verify(x => x.SynchronizeAsync(It.IsAny<CancellationToken>()), Times.Once);
            }

            [Test]
            public async Task ShouldRaiseSyncStartedEvent()
            {
                var evtCalled = false;

                this.SyncDatabase.SyncStarted += (sender, e) => evtCalled = true;
                this.synchronizerMock.Setup(x => x.SynchronizeAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(0));

                await this.SyncDatabase.SynchronizeAsync();

                Assert.IsTrue(evtCalled);
            }

            [Test]
            public async Task ShouldRaiseSyncFinishedEvent()
            {
                var evtCalled = false;

                this.SyncDatabase.SyncFinished += (sender, e) => evtCalled = true;
                this.synchronizerMock.Setup(x => x.SynchronizeAsync(It.IsAny<CancellationToken>())).Returns(Task.FromResult(0));

                await this.SyncDatabase.SynchronizeAsync();

                Assert.IsTrue(evtCalled);
            }

            [Test]
            public async Task ShouldRaiseSyncFinishedEventWithErrorDetailsIfSyncFails()
            {
                var evtCalled = false;

                this.SyncDatabase.SyncFinished += (sender, e) =>
                {
                    evtCalled = true;

                    Assert.IsFalse(e.Successful);
                    Assert.IsNotNull(e.Error);
                };

                this.synchronizerMock
                    .Setup(x => x.SynchronizeAsync(It.IsAny<CancellationToken>()))
                    .Throws(new Exception("Dummy ex"));

                await this.SyncDatabase.SynchronizeAsync();

                Assert.IsTrue(evtCalled);
            }

            [Test]
            public async Task ShouldEnsureIndices()
            {
                this.SyncConfigMock.SetupGet(x => x.SyncedCollections).Returns(new[] { nameof(TestEntity) });

                var collection = this.SyncDatabase.InnerDb.GetCollection<TestEntity>();
                Assert.IsFalse(collection.GetIndexes().Any(x => x.Field == nameof(ILiteSyncEntity.RequiresSync)));

                await this.SyncDatabase.SynchronizeAsync();

                Assert.IsTrue(collection.GetIndexes().Any(x => x.Field == nameof(ILiteSyncEntity.RequiresSync)));
            }

            [Test]
            public void ShouldReportSyncInProgress()
            {
                Assert.IsFalse(this.SyncDatabase.IsSyncInProgress);

                var evt = new ManualResetEvent(false);

                this.synchronizerMock
                    .Setup(x => x.SynchronizeAsync(It.IsAny<CancellationToken>()))
                    .Callback<CancellationToken>(x => evt.WaitOne())
                    .Returns(Task.FromResult(0));

                var firstSyncExecTask = this.SyncDatabase.SynchronizeAsync();

                Assert.IsTrue(this.SyncDatabase.IsSyncInProgress);

                evt.Set();
                firstSyncExecTask.Wait();

                Assert.IsFalse(this.SyncDatabase.IsSyncInProgress);
            }
        }

        public class WhenGettingAndSavingLocalState : LiteSyncDatabaseTests
        {
            [Test]
            public void ShouldSaveLoad()
            {
                var syncDc = (ILiteSyncDatabase)this.SyncDatabase;
                Assert.IsNull(syncDc.GetLocalCloudState());

                var state = new CloudState("PatchId");
                syncDc.SaveLocalCloudState(state);

                var loadedState = syncDc.GetLocalCloudState();

                Assert.IsNotNull(loadedState);
                Assert.AreEqual(state.NextPatchId, loadedState.NextPatchId);
            }
        }

        public class WhenGettingLocalChanges : LiteSyncDatabaseTests
        {
            [Test]
            public void ShouldNotReturnEntitiesInNonSyncedCollections()
            {
                this.SyncConfigMock.Setup(x => x.SyncedCollections).Returns(new string[0]);

                this.SyncDatabase.GetCollection<TestEntity>("Dummy").Insert(new TestEntity(1));

                var patch = ((ILiteSyncDatabase)this.SyncDatabase).GetLocalChanges(CancellationToken.None);

                Assert.IsNotNull(patch);
                Assert.IsFalse(patch.HasChanges);
            }

            [Test]
            public void ShouldReturnEntitiesRequiringSync()
            {
                this.SyncConfigMock.Setup(x => x.SyncedCollections).Returns(new[] { nameof(TestEntity) });

                this.SyncDatabase.GetCollection<TestEntity>().Insert(new TestEntity(1));

                var patch = ((ILiteSyncDatabase)this.SyncDatabase).GetLocalChanges(CancellationToken.None);

                Assert.IsNotNull(patch);
                Assert.IsTrue(patch.HasChanges);
                Assert.AreEqual(1, patch.Changes.Count());
                Assert.IsInstanceOf<UpsertEntityChange>(patch.Changes.Single());
            }

            [Test]
            public void ShouldReturnDeletedEntities()
            {
                this.SyncConfigMock.Setup(x => x.SyncedCollections).Returns(new[] { nameof(TestEntity) });

                var collection = this.SyncDatabase.GetCollection<TestEntity>();
                collection.Insert(new TestEntity(1));
                collection.Delete(1);

                var patch = ((ILiteSyncDatabase)this.SyncDatabase).GetLocalChanges(CancellationToken.None);

                Assert.IsNotNull(patch);
                Assert.IsTrue(patch.HasChanges);
                Assert.AreEqual(1, patch.Changes.Count());
                Assert.IsInstanceOf<DeleteEntityChange>(patch.Changes.Single());
            }
        }

        public class WhenApplyingChanges : LiteSyncDatabaseTests
        {
            [Test]
            public void ShouldApplyUpsertChanges()
            {
                var doc = new BsonDocument
                {
                    ["_id"] = 1,
                    [nameof(TestEntity.Text)] = "Hello"
                };

                var change = new UpsertEntityChange(new EntityId("TestEntity", 1), doc);
                var patch = new Patch(new[] { change });

                var collection = this.SyncDatabase.GetCollection<TestEntity>();

                Assert.AreEqual(0, collection.Count());

                ((ILiteSyncDatabase)this.SyncDatabase).ApplyChanges(patch, CancellationToken.None);

                Assert.AreEqual(1, collection.Count());
            }

            [Test]
            public void ShouldApplyDeleteChanges()
            {
                var change = new DeleteEntityChange(new EntityId(nameof(TestEntity), 1));
                var patch = new Patch(new[] { change });

                var entity = new TestEntity(1);
                var collection = this.SyncDatabase.GetCollection<TestEntity>();

                collection.Insert(entity);

                ((ILiteSyncDatabase)this.SyncDatabase).ApplyChanges(patch, CancellationToken.None);

                Assert.AreEqual(0, collection.Count());
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