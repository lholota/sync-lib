using System.Linq;
using LiteDB.Sync.Tests.Tools;
using NUnit.Framework;

namespace LiteDB.Sync.Tests
{
    public partial class LiteSyncCollectionTests
    {
        public class WhenDeletingSingleItemFromNonSyncedCollection : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldDeleteExistingItem()
            {
                var entity = new TestEntity(1);
                entity.Text = "Hello";

                this.SyncedCollection.Insert(entity);
                this.SyncedCollection.Delete(new BsonValue(entity.Id));

                var found = this.InnerDb.GetCollection<TestEntity>().FindAll().ToArray();

                Assert.IsNotNull(found);
                Assert.AreEqual(0, found.Length);
            }

            [Test]
            public void ShouldThrowOnNonExistingItem()
            {
                var ex = Assert.Throws<LiteException>(() => this.SyncedCollection.Delete(new BsonValue(123)));
                
                Assert.AreEqual(0, ex.ErrorCode);
            }
        }

        public class WhenDeletingItemsByPredicateFromNonSyncedCollection : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldDeleteItemsByPredicate()
            {
                var entity1 = new NonSyncedTestEntity(1);
                var entity2 = new NonSyncedTestEntity(2);
                var entity3 = new NonSyncedTestEntity(3);

                this.NonSyncedCollection.Insert(entity1);
                this.NonSyncedCollection.Insert(entity2);
                this.NonSyncedCollection.Insert(entity3);

                this.SyncedCollection.Delete(x => x.Id <= 2);

                var found = this.InnerDb.GetCollection<NonSyncedTestEntity>().FindAll().ToArray();

                Assert.IsNotNull(found);
                Assert.AreEqual(1, found.Length);
            }

            [Test]
            public void ShouldReturnZeroWhenCollectionEmpty()
            {
                var count = this.SyncedCollection.Delete(x => true);

                Assert.AreEqual(0, count);
            }
        }

        public class WhenDeletingItemsByQueryFromNonSyncedCollection : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldDeleteItemsByQuery()
            {
                var entity1 = new NonSyncedTestEntity(1);
                var entity2 = new NonSyncedTestEntity(2);
                var entity3 = new NonSyncedTestEntity(3);

                this.NonSyncedCollection.Insert(entity1);
                this.NonSyncedCollection.Insert(entity2);
                this.NonSyncedCollection.Insert(entity3);

                this.NonSyncedCollection.Delete(Query.LTE(nameof(TestEntity.Id), new BsonValue(2)));

                var found = this.InnerDb.GetCollection<NonSyncedTestEntity>().FindAll().ToArray();

                Assert.IsNotNull(found);
                Assert.AreEqual(1, found.Length);
                Assert.AreEqual(3, found[0].Id);
            }

            [Test]
            public void ShouldReturnZeroWhenCollectionEmpty()
            {
                var count = this.NonSyncedCollection.Delete(Query.LTE(nameof(TestEntity.Id), new BsonValue(1000)));

                Assert.AreEqual(0, count);
            }
        }

        public class WhenDeletingSingleItemFromSyncedCollection : LiteSyncCollectionTests
        {
            /*
             * Item doesn't exist -> should throw
             * Item soft deleted - should throw (delete twice scenario)
	         * Item exists - should soft delete -> update
             */

            [Test]
            public void ShouldDeleteSingleItemLogically()
            {
                var entity = new TestEntity(1);
                entity.Text = "Hello";

                this.SyncedCollection.Insert(entity);
                this.SyncedCollection.Delete(new BsonValue(entity.Id));

                var found = this.InnerDb.GetCollection<TestEntity>().FindById(entity.Id);

                Assert.IsNotNull(found);
                Assert.AreEqual(EntitySyncState.RequiresSyncDeleted, found.SyncState);
            }
        }

        public class WhenDeletingItemsByPredicateFromSyncedCollection : LiteSyncCollectionTests
        {
            /*
             * 
             */
            [Test]
            public void ShouldDeleteItemsByPredicateLogically()
            {
                var entity1 = new TestEntity(1);
                var entity2 = new TestEntity(2);
                var entity3 = new TestEntity(3);

                this.SyncedCollection.Insert(entity1);
                this.SyncedCollection.Insert(entity2);
                this.SyncedCollection.Insert(entity3);

                this.SyncedCollection.Delete(x => x.Id <= 2);

                var found = this.InnerDb.GetCollection<TestEntity>().FindAll().ToArray();

                Assert.IsNotNull(found);
                Assert.AreEqual(3, found.Length);
                Assert.AreEqual(EntitySyncState.RequiresSyncDeleted, found[0].SyncState);
                Assert.AreEqual(EntitySyncState.RequiresSyncDeleted, found[1].SyncState);
                Assert.AreEqual(EntitySyncState.RequiresSync, found[2].SyncState);
            }
        }

        public class WhenDeletingItemsByQueryFromSyncedCollection : LiteSyncCollectionTests
        {
            /*
             * 
             */

            [Test]
            public void ShouldDeleteItemsByQueryLogically()
            {
                var entity1 = new TestEntity(1);
                var entity2 = new TestEntity(2);
                var entity3 = new TestEntity(3);

                this.SyncedCollection.Insert(entity1);
                this.SyncedCollection.Insert(entity2);
                this.SyncedCollection.Insert(entity3);

                this.SyncedCollection.Delete(Query.LTE(nameof(TestEntity.Id), new BsonValue(2)));

                var found = this.InnerDb.GetCollection<TestEntity>().FindAll().ToArray();

                Assert.IsNotNull(found);
                Assert.AreEqual(3, found.Length);
                Assert.AreEqual(EntitySyncState.RequiresSyncDeleted, found[0].SyncState);
                Assert.AreEqual(EntitySyncState.RequiresSyncDeleted, found[1].SyncState);
                Assert.AreEqual(EntitySyncState.RequiresSync, found[2].SyncState);
            }
        }
    }
}