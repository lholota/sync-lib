using System.Linq;
using LiteDB.Sync.Internal;
using LiteDB.Sync.Tests.TestUtils;
using NUnit.Framework;

namespace LiteDB.Sync.Tests.Core.LiteSyncCollection
{
    public partial class LiteSyncCollectionTests
    {
        public class WhenDeletingSingleItem : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldCreateDeletedEntityRecord()
            {
                var entity = new TestEntity(1);
                this.NativeCollection.Insert(entity);

                this.SyncedCollection.Delete(new BsonValue(entity.Id));

                this.VerifyDeletedEntityExists(entity.Id);
            }

            [Test]
            public void ShouldNotCreateDeletedEntityWhenOriginalEntityNotExist()
            {
                var secondDeleteResult = this.SyncedCollection.Delete(new BsonValue(1));

                Assert.IsFalse(secondDeleteResult);

                this.VerifyDeletedEntitiesEmpty();
            }
        }

        public class WhenDeletingItemsByPredicate : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldCreateDeletedEntities()
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
                Assert.AreEqual(1, found.Length);
                Assert.AreEqual(3, found[0].Id);

                this.VerifyDeletedEntityExists(1);
                this.VerifyDeletedEntityExists(2);
            }

            [Test]
            public void DeletedEntitiesShouldContainIdEvenIfDeletingByOtherField()
            {
                var entity1 = new TestEntity(1) { Text = "Hello" };
                var entity2 = new TestEntity(2) { Text = "Hello" };
                var entity3 = new TestEntity(3);

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var deletedCount = this.SyncedCollection.Delete(x => x.Text == "Hello");

                Assert.AreEqual(2, deletedCount);

                var found = this.NativeCollection.FindAll().ToArray();

                Assert.IsNotNull(found);
                Assert.AreEqual(1, found.Length);
                Assert.AreEqual(3, found[0].Id);

                this.VerifyDeletedEntityExists(1);
                this.VerifyDeletedEntityExists(2);
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
                Assert.AreEqual(3, found[0].Id);

                this.VerifyDeletedEntitiesEmpty();
            }
        }

        public class WhenDeletingItemsByQuery : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldCreateDeletedEntities()
            {
                var entity1 = new TestEntity(1);
                var entity2 = new TestEntity(2);
                var entity3 = new TestEntity(3);

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var deletedCount = this.SyncedCollection.Delete(Query.LTE("_id", 2));

                Assert.AreEqual(2, deletedCount);

                var found = this.NativeCollection.FindAll().ToArray();

                Assert.IsNotNull(found);
                Assert.AreEqual(1, found.Length);
                Assert.AreEqual(3, found[0].Id);

                this.VerifyDeletedEntityExists(1);
                this.VerifyDeletedEntityExists(2);
            }

            [Test]
            public void DeletedEntitiesShouldContainIdEvenIfDeletingByOtherField()
            {
                var entity1 = new TestEntity(1) { Text = "Hello" };
                var entity2 = new TestEntity(2) { Text = "Hello" };
                var entity3 = new TestEntity(3);

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var deletedCount = this.SyncedCollection.Delete(Query.EQ("Text", "Hello"));

                Assert.AreEqual(2, deletedCount);

                var found = this.NativeCollection.FindAll().ToArray();

                Assert.IsNotNull(found);
                Assert.AreEqual(1, found.Length);
                Assert.AreEqual(3, found[0].Id);

                this.VerifyDeletedEntityExists(1);
                this.VerifyDeletedEntityExists(2);
            }

            [Test]
            public void ShouldReturnZeroIfNoItemsMatch()
            {
                var entity3 = new TestEntity(3);
                this.NativeCollection.Insert(entity3);

                var deletedCount = this.SyncedCollection.Delete(Query.LTE("_id", 2));

                Assert.AreEqual(0, deletedCount);

                var found = this.NativeCollection.FindAll().ToArray();

                Assert.IsNotNull(found);
                Assert.AreEqual(1, found.Length);
                Assert.AreEqual(3, found[0].Id);

                this.VerifyDeletedEntitiesEmpty();
            }
        }

        protected void VerifyDeletedEntityExists(BsonValue id)
        {
            var deletedEntity = this.Db.GetDeletedEntitiesCollection().FindById(this.Db.Mapper, new EntityId(CollectionName, id));

            Assert.IsNotNull(deletedEntity, "The DeletedEntity for id {0} was not found", id);
        }

        protected void VerifyDeletedEntitiesEmpty()
        {
            var all = this.Db.GetDeletedEntitiesCollection().FindAll().ToArray();

            Assert.AreEqual(0, all.Length);
        }
    }
}