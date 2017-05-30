using System.Linq;
using LiteDB.Sync.Tests.Tools;
using NUnit.Framework;

namespace LiteDB.Sync.Tests
{
    public partial class LiteSyncCollectionTests
    {
        public class WhenDeletingSingleItem : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldDeleteSingleItemWhenExists()
            {
                var entity = new TestEntity(1);

                this.SyncedCollection.Insert(entity);
                this.SyncedCollection.Delete(new BsonValue(entity.Id));

                this.VerifyExistsSoftDeleted(entity);
            }

            [Test]
            public void ShouldReturnFalseOnDeletionWhenAlreadySoftDeleted()
            {
                var entity = new TestEntity(1);

                this.SyncedCollection.Insert(entity);

                this.SyncedCollection.Delete(new BsonValue(entity.Id));
                var secondDeleteResult = this.SyncedCollection.Delete(new BsonValue(entity.Id));

                Assert.IsFalse(secondDeleteResult);

                this.VerifyExistsSoftDeleted(entity);
            }

            [Test]
            public void ShouldReturnFalseOnDeletionWhenItemDoesntExist()
            {
                var entity = new TestEntity(1);

                var deleteResult = this.SyncedCollection.Delete(new BsonValue(entity.Id));

                Assert.IsFalse(deleteResult);
            }

            private void VerifyExistsSoftDeleted(TestEntity entity)
            {
                var found = this.NativeCollection.FindById(entity.Id);

                Assert.IsNotNull(found, $"The item with id {entity.Id} could not be found.");
                Assert.AreEqual(EntitySyncState.RequiresSyncDeleted, found.SyncState);
            }
        }

        public class WhenDeletingItemsByPredicate : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldDeleteItems()
            {
                var entity1 = new TestEntity(1);
                var entity2 = new TestEntity(2);
                var entity3 = new TestEntity(3);

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var deletedCount = this.SyncedCollection.Delete(x => x.Id <= 2);

                Assert.AreEqual(2, deletedCount);

                var found = this.NativeCollection.FindAll().ToArray();

                Assert.IsNotNull(found);
                Assert.AreEqual(3, found.Length);
                Assert.AreEqual(EntitySyncState.RequiresSyncDeleted, found[0].SyncState);
                Assert.AreEqual(EntitySyncState.RequiresSyncDeleted, found[1].SyncState);
                Assert.AreEqual(EntitySyncState.None, found[2].SyncState);
            }

            [Test]
            public void ShouldIgnoreSoftDeletedItems()
            {
                var entity1 = new TestEntity(1);
                entity1.SyncState = EntitySyncState.RequiresSyncDeleted;

                var entity2 = new TestEntity(2);
                entity2.SyncState = EntitySyncState.RequiresSyncDeleted;

                var entity3 = new TestEntity(3);

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var deletedCount = this.SyncedCollection.Delete(x => x.Id <= 3);

                Assert.AreEqual(1, deletedCount);

                var found = this.NativeCollection.FindAll().ToArray();

                Assert.IsNotNull(found);
                Assert.AreEqual(3, found.Length);
                Assert.AreEqual(EntitySyncState.RequiresSyncDeleted, found[0].SyncState);
                Assert.AreEqual(EntitySyncState.RequiresSyncDeleted, found[1].SyncState);
                Assert.AreEqual(EntitySyncState.RequiresSyncDeleted, found[2].SyncState);
            }

            [Test]
            public void ShouldReturnZeroIfNoItemsMatch()
            {
                var entity3 = new TestEntity(3);
                this.NativeCollection.Insert(entity3);

                var deletedCount = this.SyncedCollection.Delete(x => x.Id <= 2);

                Assert.AreEqual(0, deletedCount);

                var found = this.NativeCollection.FindAll().ToArray();

                Assert.IsNotNull(found);
                Assert.AreEqual(1, found.Length);
                Assert.AreEqual(EntitySyncState.None, found[0].SyncState);
            }
        }

        public class WhenDeletingItemsByQuery : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldDeleteItems()
            {
                var entity1 = new TestEntity(1);
                var entity2 = new TestEntity(2);
                var entity3 = new TestEntity(3);

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var deletedCount = this.SyncedCollection.Delete(Query.LTE("_id", new BsonValue(2)));

                Assert.AreEqual(2, deletedCount);

                var found = this.NativeCollection.FindAll().ToArray();

                Assert.IsNotNull(found);
                Assert.AreEqual(3, found.Length);
                Assert.AreEqual(EntitySyncState.RequiresSyncDeleted, found[0].SyncState);
                Assert.AreEqual(EntitySyncState.RequiresSyncDeleted, found[1].SyncState);
                Assert.AreEqual(EntitySyncState.None, found[2].SyncState);
            }

            [Test]
            public void ShouldIgnoreSoftDeletedItems()
            {
                var entity1 = new TestEntity(1);
                entity1.SyncState = EntitySyncState.RequiresSyncDeleted;

                var entity2 = new TestEntity(2);
                entity2.SyncState = EntitySyncState.RequiresSyncDeleted;

                var entity3 = new TestEntity(3);

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var deletedCount = this.SyncedCollection.Delete(Query.LTE("_id", new BsonValue(3)));

                Assert.AreEqual(1, deletedCount);

                var found = this.NativeCollection.FindAll().ToArray();

                Assert.IsNotNull(found);
                Assert.AreEqual(3, found.Length);
                Assert.AreEqual(EntitySyncState.RequiresSyncDeleted, found[0].SyncState);
                Assert.AreEqual(EntitySyncState.RequiresSyncDeleted, found[1].SyncState);
                Assert.AreEqual(EntitySyncState.RequiresSyncDeleted, found[2].SyncState);
            }

            [Test]
            public void ShouldReturnZeroIfNoItemsMatch()
            {
                var entity3 = new TestEntity(3);
                this.NativeCollection.Insert(entity3);

                var deletedCount = this.SyncedCollection.Delete(Query.LTE("_id", new BsonValue(2)));

                Assert.AreEqual(0, deletedCount);

                var found = this.NativeCollection.FindAll().ToArray();

                Assert.IsNotNull(found);
                Assert.AreEqual(1, found.Length);
                Assert.AreEqual(EntitySyncState.None, found[0].SyncState);
            }
        }
    }
}