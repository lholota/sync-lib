using System.Linq;
using LiteDB.Sync.Contract;
using LiteDB.Sync.Tests.Tools;
using NUnit.Framework;

namespace LiteDB.Sync.Tests
{
    public partial class LiteSyncCollectionTests
    {
        public class WhenFindingAll : LiteSyncCollectionTests
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

                var all = this.SyncedCollection.FindAll().ToArray();

                Assert.AreEqual(2, all.Length);
                Assert.AreEqual(1, all[0].Id);
                Assert.AreEqual(3, all[1].Id);
            }
        }

        public class WhenFindingOneByQuery : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldNotReturnSoftDeletedItems()
            {
                var entity1 = new TestEntity(1) { Text = "Hello", SyncState = SyncState.RequiresSyncDeleted };
                var entity2 = new TestEntity(2) { Text = "Hello", SyncState = SyncState.RequiresSyncDeleted };
                var entity3 = new TestEntity(3) { Text = "Hello" };

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var item = this.SyncedCollection.FindOne(Query.EQ(nameof(TestEntity.Text), new BsonValue("Hello")));

                Assert.IsNotNull(item);
                Assert.AreEqual(3, item.Id);
            }

            [Test]
            public void ShouldNotReturnSoftDeletedItemsEvenIfNoOtherAvailable()
            {
                var entity1 = new TestEntity(1) { Text = "Hello", SyncState = SyncState.RequiresSyncDeleted };
                var entity2 = new TestEntity(2) { Text = "Hello", SyncState = SyncState.RequiresSyncDeleted };

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);

                var item = this.SyncedCollection.FindOne(Query.EQ(nameof(TestEntity.Text), new BsonValue("Hello")));

                Assert.IsNull(item);
            }

            [Test]
            public void ShouldReturnMatchingItem()
            {
                var entity3 = new TestEntity(3) { Text = "Hello" };

                this.NativeCollection.Insert(entity3);

                var item = this.SyncedCollection.FindOne(Query.EQ(nameof(TestEntity.Text), new BsonValue("Hello")));

                Assert.IsNotNull(item);
                Assert.AreEqual(3, item.Id);
            }
        }

        public class WhenFindingOneByPredicate : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldNotReturnSoftDeletedItems()
            {
                var entity1 = new TestEntity(1) { Text = "Hello", SyncState = SyncState.RequiresSyncDeleted };
                var entity2 = new TestEntity(2) { Text = "Hello", SyncState = SyncState.RequiresSyncDeleted };
                var entity3 = new TestEntity(3) { Text = "Hello" };

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var item = this.SyncedCollection.FindOne(x => x.Text == "Hello");

                Assert.IsNotNull(item);
                Assert.AreEqual(3, item.Id);
            }

            [Test]
            public void ShouldNotReturnSoftDeletedItemsEvenIfNoOtherAvailable()
            {
                var entity1 = new TestEntity(1) { Text = "Hello", SyncState = SyncState.RequiresSyncDeleted };
                var entity2 = new TestEntity(2) { Text = "Hello", SyncState = SyncState.RequiresSyncDeleted };

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);

                var item = this.SyncedCollection.FindOne(x => x.Text == "Hello");

                Assert.IsNull(item);
            }

            [Test]
            public void ShouldReturnMatchingItem()
            {
                var entity3 = new TestEntity(3) { Text = "Hello" };

                this.NativeCollection.Insert(entity3);

                var item = this.SyncedCollection.FindOne(x => x.Text == "Hello");

                Assert.IsNotNull(item);
                Assert.AreEqual(3, item.Id);
            }
        }

        public class WhenFindingById : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldIgnoreSoftDeletedItems()
            {
                var entity1 = new TestEntity(1) { SyncState = SyncState.RequiresSyncDeleted };

                this.NativeCollection.Insert(entity1);

                var item = this.SyncedCollection.FindById(new BsonValue(1));

                Assert.IsNull(item);
            }

            [Test]
            public void ShouldReturnItemIfFound()
            {
                var entity1 = new TestEntity(1) { SyncState = SyncState.RequiresSync };

                this.NativeCollection.Insert(entity1);

                var item = this.SyncedCollection.FindById(new BsonValue(1));

                Assert.IsNotNull(item);
                Assert.AreEqual(1, item.Id);
            }
        }

        public class WhenFindingMultipleByPredicate : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldNotReturnSoftDeletedItems()
            {
                var entity1 = new TestEntity(1) { Text = "Hello", SyncState = SyncState.RequiresSyncDeleted };
                var entity2 = new TestEntity(2) { Text = "Hello", SyncState = SyncState.RequiresSyncDeleted };
                var entity3 = new TestEntity(3) { Text = "Hello" };
                var entity4 = new TestEntity(4) { Text = "Hello" };

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);
                this.NativeCollection.Insert(entity4);

                var items = this.SyncedCollection.Find(x => x.Text == "Hello").ToArray();

                Assert.IsNotNull(items);
                Assert.AreEqual(2, items.Length);
                Assert.AreEqual(3, items[0].Id);
                Assert.AreEqual(4, items[1].Id);
            }
        }

        public class WhenFindingMultipleByQuery : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldNotReturnSoftDeletedItems()
            {
                var entity1 = new TestEntity(1) { Text = "Hello", SyncState = SyncState.RequiresSyncDeleted };
                var entity2 = new TestEntity(2) { Text = "Hello", SyncState = SyncState.RequiresSyncDeleted };
                var entity3 = new TestEntity(3) { Text = "Hello" };
                var entity4 = new TestEntity(4) { Text = "Hello" };

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);
                this.NativeCollection.Insert(entity4);

                var query = Query.EQ(nameof(TestEntity.Text), new BsonValue("Hello"));
                var items = this.SyncedCollection.Find(query).ToArray();

                Assert.IsNotNull(items);
                Assert.AreEqual(2, items.Length);
                Assert.AreEqual(3, items[0].Id);
                Assert.AreEqual(4, items[1].Id);
            }
        }
    }
}