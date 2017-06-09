using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using LiteDB.Sync.Exceptions;
using LiteDB.Sync.Internal;
using LiteDB.Sync.Tests.TestUtils;
using Moq;
using NUnit.Framework;

namespace LiteDB.Sync.Tests.Core.Internal
{
    [TestFixture]
    public class SynchronizerTests
    {
        internal Synchronizer Synchronizer;
        internal Mock<ICloudClient> CloudClientMock;
        internal Mock<ILiteCollection<TestEntity>> TestEntityCollection;
        internal Mock<ILiteSyncDatabase> DbMock;

        protected Mock<ILiteSyncConfiguration> configMock;
        protected Mock<ILiteTransaction> transactionMock;
        protected Mock<ILiteSyncConflictResolver> conflictResolverMock;

        [SetUp]
        public void Setup()
        {
            this.DbMock = new Mock<ILiteSyncDatabase>();
            this.configMock = new Mock<ILiteSyncConfiguration>();
            this.TestEntityCollection = new Mock<ILiteCollection<TestEntity>>();
            this.transactionMock = new Mock<ILiteTransaction>();
            this.CloudClientMock = new Mock<ICloudClient>();
            this.conflictResolverMock = new Mock<ILiteSyncConflictResolver>();

            this.configMock.Setup(x => x.SyncedCollections).Returns(new[] { nameof(TestEntity) });
            this.configMock.Setup(x => x.ConflictResolver).Returns(this.conflictResolverMock.Object);

            this.Synchronizer = new Synchronizer(
                this.DbMock.Object,
                this.configMock.Object,
                this.CloudClientMock.Object);
        }

        public class WhenRunningSuccessfully : SynchronizerTests
        {
            [Test]
            public async Task ShouldSaveCloudStateWhenStartingWithNone()
            {
                this.SetupGetCloudState(null);
                this.SetupGetLocalChanges(false);
                this.SetupPull("NextPatchId", false);
                this.SetupSaveCloudState("NextPatchId");

                await this.Synchronizer.SynchronizeAsync(CancellationToken.None);

                this.VerifyAllMocks();
            }

            [Test]
            public async Task ShouldSaveCloudStateWhenStartingWithExistingState()
            {
                this.SetupGetCloudState("Patch1");
                this.SetupGetLocalChanges(false);
                this.SetupPull("Patch2", false);
                this.SetupSaveCloudState("Patch2");

                await this.Synchronizer.SynchronizeAsync(CancellationToken.None);

                this.VerifyAllMocks();
            }

            [Test]
            public async Task ShouldNotSaveCloudStateIfNoUpdateIsAvailable()
            {
                this.SetupGetCloudState("Patch1");
                this.SetupGetLocalChanges(false);
                this.SetupPull("Patch1", false);

                await this.Synchronizer.SynchronizeAsync(CancellationToken.None);

                this.VerifyAllMocks();
            }

            [Test]
            public async Task ShouldApplyAllRemotePatchesWhenStartingWithNoState()
            {
                this.SetupGetCloudState(null);
                this.SetupGetLocalChanges(false);
                this.SetupPullWithMultiplePatches("Patch3", "LatestText");
                this.SetupSaveCloudState("Patch3");
                this.SetupApplyChanges("LatestText");
                this.SetupBeginDbTransaction();
                this.SetupDbTransactionCommitDispose();

                await this.Synchronizer.SynchronizeAsync(CancellationToken.None);

                this.VerifyAllMocks();
            }

            [Test]
            public async Task ShouldPushLocalChangesToCloud()
            {
                this.SetupGetCloudState("Patch1");
                this.SetupGetLocalChanges(true);
                this.SetupPull("Patch1", false);
                this.SetupPush("Patch2", "Hello-Local");
                this.SetupSaveCloudState("Patch2");
                this.SetupBeginDbTransaction();
                this.SetupDbTransactionCommitDispose();

                await this.Synchronizer.SynchronizeAsync(CancellationToken.None);

                this.VerifyAllMocks();
            }
        }

        public class WhenHandlingErrors : SynchronizerTests
        {
            /*
             * Should wrap exceptions (?)
             * 
             * 
             * Remote errors
             * - Pull
             * - Push
             * 
             * Local errors
             * - Save/transaction commit (Mock transaction)
             * - Loading changes
             */
        }

        public class WhenHandlingConflicts : SynchronizerTests
        {
            [Test]
            public async Task ShouldRetryIfAnotherPushHappened()
            {
                this.SetupGetCloudState("Patch1");
                this.SetupGetLocalChanges(true, 2);

                this.CloudClientMock
                    .SetupSequence(x => x.Pull(It.IsAny<CloudState>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new PullResult(new CloudState("Patch1"))) // First call is fine, nothing no changes available
                    .ReturnsAsync(new PullResult(new[] { this.CreatePatch(1, "Hello") }, new CloudState("Patch2")));

                this.CloudClientMock
                    .SetupSequence(x => x.Push(It.IsAny<CloudState>(), It.IsAny<Patch>(), It.IsAny<CancellationToken>()))
                    .Throws(new LiteSyncConflictOccuredException())
                    .ReturnsAsync(new CloudState("Patch3"));

                this.SetupApplyChanges("Hello");
                this.SetupBeginDbTransaction();
                this.SetupDbTransactionCommitDispose();

                await this.Synchronizer.SynchronizeAsync(CancellationToken.None);

                this.VerifyAllMocks();
            }

            [Test]
            public void ShouldThrowIfRetryLimitExceeded()
            {
                this.SetupGetCloudState("Patch1");
                this.SetupGetLocalChanges(true, 2);

                this.CloudClientMock
                    .SetupSequence(x => x.Pull(It.IsAny<CloudState>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new PullResult(new CloudState("Patch1"))) // First call is fine, nothing new
                    .ReturnsAsync(new PullResult(new[] {this.CreatePatch(1, "Hello2")}, new CloudState("Patch2")))
                    .ReturnsAsync(new PullResult(new[] {this.CreatePatch(1, "Hello3")}, new CloudState("Patch3")))
                    .ReturnsAsync(new PullResult(new[] {this.CreatePatch(1, "Hello4")}, new CloudState("Patch4")));

                this.CloudClientMock
                    .SetupSequence(x => x.Push(It.IsAny<CloudState>(), It.IsAny<Patch>(), It.IsAny<CancellationToken>()))
                    .Throws(new LiteSyncConflictOccuredException())
                    .Throws(new LiteSyncConflictOccuredException())
                    .Throws(new LiteSyncConflictOccuredException())
                    .Throws(new LiteSyncConflictOccuredException());

                this.SetupApplyChanges("Hello", false);
                this.SetupBeginDbTransaction();
                this.transactionMock.Setup(x => x.Dispose()); // Should not commit

                Assert.ThrowsAsync<LiteSyncConflictRetryCountExceededException>(async () => await this.Synchronizer.SynchronizeAsync(CancellationToken.None));

                this.TestEntityCollection.VerifyAll();
                this.transactionMock.VerifyAll();
                this.CloudClientMock.VerifyAll();
            }

            [Test]
            public void ShouldThrowIfConflictNotResolved()
            {
                this.SetupGetCloudState("Patch1");
                this.SetupGetLocalChanges(true);
                this.SetupPull("Patch2", true);
                this.SetupBeginDbTransaction();
                this.transactionMock.Setup(x => x.Dispose());

                this.SetupConflictResolver((conflict, mapper) => { });

                Assert.ThrowsAsync<LiteSyncConflictNotResolvedException>(async () => await this.Synchronizer.SynchronizeAsync(CancellationToken.None));

                this.VerifyAllMocks();
            }

            [Test]
            public async Task ShouldFinishSuccessfullyIfConflictResolvedWithLocal()
            {
                this.SetupGetCloudState("Patch1");
                this.SetupGetLocalChanges(true);
                this.SetupBeginDbTransaction();
                this.SetupPull("Patch2", true);
                this.SetupPush("Patch3", "Hello-Local", true);
                this.SetupSaveCloudState("Patch3");
                this.SetupDbTransactionCommitDispose();

                this.SetupConflictResolver((conflict, mapper) => conflict.ResolveKeepLocal());

                await this.Synchronizer.SynchronizeAsync(CancellationToken.None);

                this.VerifyAllMocks();
            }

            [Test]
            public async Task ShouldFinishSuccessfullyIfConflictResolvedWithRemote()
            {
                this.SetupGetCloudState("Patch1");
                this.SetupGetLocalChanges(true);
                this.SetupBeginDbTransaction();
                this.SetupPull("Patch2", true);
                this.SetupSaveCloudState("Patch2");
                this.SetupApplyChanges("Hello-Remote");
                this.SetupDbTransactionCommitDispose();

                // No push - no other changes in the queue

                this.SetupConflictResolver((conflict, mapper) => conflict.ResolveKeepRemote());

                await this.Synchronizer.SynchronizeAsync(CancellationToken.None);

                this.VerifyAllMocks();
            }

            [Test]
            public async Task ShouldFinishSuccessfullyIfConflictResolvedWithMerged()
            {
                this.SetupGetCloudState("Patch1");
                this.SetupGetLocalChanges(true);
                this.SetupBeginDbTransaction();
                this.SetupPull("Patch2", true);
                
                this.SetupDbMapper();
                this.SetupPush("Patch3", "Hello-Merged");
                this.SetupApplyChanges("Hello-Merged");
                this.SetupSaveCloudState("Patch3");
                this.SetupDbTransactionCommitDispose();

                // No push - no other changes in the queue

                this.SetupConflictResolver((conflict, mapper) =>
                {
                    var localDoc = ((UpsertEntityChange)conflict.LocalChange).Entity;
                    var local = mapper.ToObject<TestEntity>(localDoc);

                    local.Text = "Hello-Merged";

                    conflict.ResolveMerged(mapper.ToDocument(local));
                });

                await this.Synchronizer.SynchronizeAsync(CancellationToken.None);

                this.VerifyAllMocks();
            }
        }

        public class WhenGettingCancelled : SynchronizerTests
        {
            /*
             * Mock various parts via a reset event and cancell in various moments - especially around transaction commit...
             */
        }

        protected void SetupGetCloudState(string nextPatchId)
        {
            this.DbMock.Setup(x => x.GetLocalCloudState()).Returns(new CloudState(nextPatchId));
        }

        protected void SetupSaveCloudState(string expectedNextPatchId)
        {
            this.DbMock
                .Setup(x => x.SaveLocalCloudState(It.IsAny<CloudState>()))
                .Callback<CloudState>(state => Assert.AreEqual(expectedNextPatchId, state.NextPatchId));
        }

        protected void SetupGetLocalChanges(bool changesAvailable, int entityId = 1)
        {
            var dirtyDocs = new BsonDocument[0];

            if (changesAvailable)
            {
                var entityDoc = new BsonDocument();
                entityDoc["_id"] = new BsonValue(entityId);
                entityDoc[nameof(TestEntity.RequiresSync)] = true;
                entityDoc[nameof(TestEntity.Text)] = "Hello-Local";

                dirtyDocs = new[] { entityDoc };
            }

            var result = new Patch();
            result.AddUpsertChanges(nameof(TestEntity), dirtyDocs);

            this.DbMock
                .Setup(x => x.GetLocalChanges(It.IsAny<CancellationToken>()))
                .Returns(result);
        }

        protected void SetupApplyChanges(string expectedTextValue, bool assertTextValue = true)
        {
            this.DbMock
                .Setup(x => x.ApplyChanges(It.IsAny<Patch>(), It.IsAny<CancellationToken>()))
                .Callback<Patch, CancellationToken>((patch, ct) =>
                {
                    var change = patch.Changes.OfType<UpsertEntityChange>().Single();

                    if (assertTextValue)
                    {
                        Assert.AreEqual(expectedTextValue, change.Entity[nameof(TestEntity.Text)].AsString);
                    }
                });
        }

        protected void SetupPull(string nextPatchId, bool changesAvailable, int entityId = 1)
        {
            PullResult result;
            var cloudState = new CloudState(nextPatchId);

            if (changesAvailable)
            {
                var patch = this.CreatePatch(entityId, "Hello-Remote");

                result = new PullResult(new[] { patch }, cloudState);
            }
            else
            {
                result = new PullResult(cloudState);
            }

            this.CloudClientMock
                .Setup(x => x.Pull(It.IsAny<CloudState>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);
        }

        protected void SetupPullWithMultiplePatches(string nextPatchId, string lastTextPropValue)
        {
            var patch1 = this.CreatePatch(1, "Hello");
            var patch2 = this.CreatePatch(1, lastTextPropValue);

            var cloudState = new CloudState(nextPatchId);
            var result = new PullResult(new[] { patch1, patch2 }, cloudState);

            this.CloudClientMock
                .Setup(x => x.Pull(It.IsAny<CloudState>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);
        }

        protected void SetupPush(string returnedNextPatchId, string expectedTextValue = null, bool assertTextValue = true)
        {
            this.CloudClientMock
                .Setup(x => x.Push(It.IsAny<CloudState>(), It.IsAny<Patch>(), It.IsAny<CancellationToken>()))
                .Callback<CloudState, Patch, CancellationToken>((state, patch, ct) =>
                {
                    if (assertTextValue)
                    {
                        var change = patch.Changes.OfType<UpsertEntityChange>().Single();
                        Assert.AreEqual(expectedTextValue, change.Entity[nameof(TestEntity.Text)].AsString);
                    }
                })
                .ReturnsAsync(new CloudState(returnedNextPatchId));
        }

        protected void SetupBeginDbTransaction()
        {
            this.DbMock.Setup(x => x.BeginTrans()).Returns(this.transactionMock.Object);
        }

        protected void SetupDbTransactionCommitDispose()
        {
            this.transactionMock.Setup(x => x.Commit());
            this.transactionMock.Setup(x => x.Dispose());
        }

        protected void SetupDbMapper()
        {
            this.DbMock.SetupGet(x => x.Mapper).Returns(new BsonMapper());
        }

        protected void SetupConflictResolver(Action<LiteSyncConflict, BsonMapper> resolveAction)
        {
            this.conflictResolverMock
                .Setup(x => x.Resolve(It.IsAny<LiteSyncConflict>(), It.IsAny<BsonMapper>()))
                .Callback(resolveAction);
        }

        internal Patch CreatePatch(int id, string textPropValue)
        {
            var doc = new BsonDocument();

            doc[nameof(TestEntity.Text)] = textPropValue;

            return new Patch(new[]
            {
                new UpsertEntityChange(new EntityId(nameof(TestEntity), 1), doc)
            });
        }

        protected void VerifyAllMocks()
        {
            this.DbMock.VerifyAll();
            this.TestEntityCollection.VerifyAll();
            this.transactionMock.VerifyAll();
            this.CloudClientMock.VerifyAll();
            this.conflictResolverMock.VerifyAll();
        }
    }
}