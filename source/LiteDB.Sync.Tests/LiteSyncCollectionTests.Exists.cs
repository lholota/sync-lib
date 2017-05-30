using LiteDB.Sync.Contract;
using LiteDB.Sync.Tests.Tools;
using NUnit.Framework;

namespace LiteDB.Sync.Tests
{
    public partial class LiteSyncCollectionTests
    {
        public class WhenCheckingExistsByQuery : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldReturnTrueIfAtLeastOneExists()
            {
                var entity1 = new TestEntity(1) { SyncState = SyncState.RequiresSyncDeleted };
                var entity2 = new TestEntity(2) { SyncState = SyncState.RequiresSyncDeleted };
                var entity3 = new TestEntity(3);

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var exists = this.SyncedCollection.Exists(Query.All());

                Assert.IsTrue(exists);
            }

            [Test]
            public void ShouldReturnFalseIfAllDeleted()
            {
                var entity1 = new TestEntity(1) { SyncState = SyncState.RequiresSyncDeleted };
                var entity2 = new TestEntity(2) { SyncState = SyncState.RequiresSyncDeleted };
                var entity3 = new TestEntity(3) { SyncState = SyncState.RequiresSyncDeleted };

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var exists = this.SyncedCollection.Exists(Query.All());

                Assert.IsFalse(exists);
            }
        }

        public class WhenCheckingExistsByPredicate : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldReturnTrueIfAtLeastOneExists()
            {
                var entity1 = new TestEntity(1) { SyncState = SyncState.RequiresSyncDeleted };
                var entity2 = new TestEntity(2) { SyncState = SyncState.RequiresSyncDeleted };
                var entity3 = new TestEntity(3);

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var exists = this.SyncedCollection.Exists(x => true);

                Assert.IsTrue(exists);
            }

            [Test]
            public void ShouldReturnFalseIfAllDeleted()
            {
                var entity1 = new TestEntity(1) { SyncState = SyncState.RequiresSyncDeleted };
                var entity2 = new TestEntity(2) { SyncState = SyncState.RequiresSyncDeleted };
                var entity3 = new TestEntity(3) { SyncState = SyncState.RequiresSyncDeleted };

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var exists = this.SyncedCollection.Exists(x => true);

                Assert.IsFalse(exists);
            }
        }
    }
}