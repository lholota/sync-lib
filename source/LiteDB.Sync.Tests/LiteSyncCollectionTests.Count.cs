using LiteDB.Sync.Contract;
using LiteDB.Sync.Tests.Tools;
using NUnit.Framework;

namespace LiteDB.Sync.Tests
{
    public partial class LiteSyncCollectionTests
    {
        public class WhenCountingAll : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldIgnoreSoftDeletedItems()
            {
                var entity1 = new TestEntity(1) { SyncState = SyncState.RequiresSyncDeleted };
                var entity2 = new TestEntity(2) { SyncState = SyncState.RequiresSyncDeleted };
                var entity3 = new TestEntity(3);

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var count = this.SyncedCollection.Count();

                Assert.AreEqual(1, count);
            }
        }

        public class WhenLongCountingAll : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldIgnoreSoftDeletedItems()
            {
                var entity1 = new TestEntity(1) { SyncState = SyncState.RequiresSyncDeleted };
                var entity2 = new TestEntity(2) { SyncState = SyncState.RequiresSyncDeleted };
                var entity3 = new TestEntity(3);

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var count = this.SyncedCollection.LongCount();

                Assert.AreEqual(1, count);
            }
        }

        public class WhenCountingWithPredicate : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldIgnoreSoftDeletedItems()
            {
                var entity1 = new TestEntity(1);
                var entity2 = new TestEntity(2) { SyncState = SyncState.RequiresSyncDeleted };
                var entity3 = new TestEntity(3);

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var count = this.SyncedCollection.Count(x => x.Id >= 2);

                Assert.AreEqual(1, count);
            }
        }

        public class WhenLongCountingWithPredicate : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldIgnoreSoftDeletedItems()
            {
                var entity1 = new TestEntity(1);
                var entity2 = new TestEntity(2) { SyncState = SyncState.RequiresSyncDeleted };
                var entity3 = new TestEntity(3);

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var count = this.SyncedCollection.LongCount(x => x.Id >= 2);

                Assert.AreEqual(1, count);
            }
        }

        public class WhenCountingWithQuery : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldIgnoreSoftDeletedItems()
            {
                var entity1 = new TestEntity(1);
                var entity2 = new TestEntity(2) { SyncState = SyncState.RequiresSyncDeleted };
                var entity3 = new TestEntity(3);

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var count = this.SyncedCollection.Count(Query.GTE("_id", new BsonValue(2)));

                Assert.AreEqual(1, count);
            }
        }

        public class WhenLongCountingWithQuery : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldIgnoreSoftDeletedItems()
            {
                var entity1 = new TestEntity(1);
                var entity2 = new TestEntity(2) { SyncState = SyncState.RequiresSyncDeleted };
                var entity3 = new TestEntity(3);

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var count = this.SyncedCollection.LongCount(Query.GTE("_id", new BsonValue(2)));

                Assert.AreEqual(1, count);
            }
        }
    }
}