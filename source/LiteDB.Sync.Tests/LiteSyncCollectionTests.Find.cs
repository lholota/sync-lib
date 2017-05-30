using System.Linq;
using LiteDB.Sync.Tests.Tools;
using NUnit.Framework;

namespace LiteDB.Sync.Tests
{
    public partial class LiteSyncCollectionTests
    {
        public class WhenFindingAll : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldReturnAllItems()
            {
                var entity1 = new TestEntity(1);
                var entity2 = new TestEntity(2);
                var entity3 = new TestEntity(3);

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var all = this.SyncedCollection.FindAll().ToArray();

                Assert.AreEqual(3, all.Length);
            }
        }

        public class WhenFindingOneByQuery : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldReturnFirstItem()
            {
                var entity1 = new TestEntity(1) { Text = "Hello" };
                var entity2 = new TestEntity(2) { Text = "Hello" };
                var entity3 = new TestEntity(3) { Text = "Hello" };

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var item = this.SyncedCollection.FindOne(Query.EQ(nameof(TestEntity.Text), new BsonValue("Hello")));

                Assert.IsNotNull(item);
                Assert.AreEqual(1, item.Id);
            }
        }

        public class WhenFindingOneByPredicate : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldReturnFirstItem()
            {
                var entity1 = new TestEntity(1) { Text = "Hello" };
                var entity2 = new TestEntity(2) { Text = "Hello" };
                var entity3 = new TestEntity(3) { Text = "Hello" };

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var item = this.SyncedCollection.FindOne(x => x.Text == "Hello");

                Assert.IsNotNull(item);
                Assert.AreEqual(1, item.Id);
            }
        }

        public class WhenFindingById : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldReturnItemIfFound()
            {
                var entity1 = new TestEntity(1);

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
                var entity1 = new TestEntity(1) { Text = "Hello" };
                var entity2 = new TestEntity(2) { Text = "Hello" };
                var entity3 = new TestEntity(3) { Text = "Hello" };

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var items = this.SyncedCollection.Find(x => x.Text == "Hello").ToArray();

                Assert.IsNotNull(items);
                Assert.AreEqual(3, items.Length);
            }
        }

        public class WhenFindingMultipleByQuery : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldNotReturnSoftDeletedItems()
            {
                var entity1 = new TestEntity(1) { Text = "Hello" };
                var entity2 = new TestEntity(2) { Text = "Hello" };
                var entity3 = new TestEntity(3) { Text = "Hello" };

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var query = Query.EQ(nameof(TestEntity.Text), new BsonValue("Hello"));
                var items = this.SyncedCollection.Find(query).ToArray();

                Assert.IsNotNull(items);
                Assert.AreEqual(3, items.Length);
            }
        }
    }
}