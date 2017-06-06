using LiteDB.Sync.Tests.TestUtils;
using NUnit.Framework;

namespace LiteDB.Sync.Tests.Core.LiteSyncCollection
{
    public partial class LiteSyncCollectionTests
    {
        public class WhenCheckingExistsByQuery : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldReturnTrueIfAtLeastOneExists()
            {
                var entity1 = new TestEntity(1);
                var entity2 = new TestEntity(2) { Text = "Hello"};
                var entity3 = new TestEntity(3);

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var exists = this.SyncedCollection.Exists(Query.EQ("Text", "Hello"));

                Assert.IsTrue(exists);
            }

            [Test]
            public void ShouldReturnFalseIfNonExists()
            {
                var entity1 = new TestEntity(1);
                var entity2 = new TestEntity(2);
                var entity3 = new TestEntity(3);

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var exists = this.SyncedCollection.Exists(Query.EQ("Text", "Hello"));

                Assert.IsFalse(exists);
            }
        }

        public class WhenCheckingExistsByPredicate : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldReturnTrueIfAtLeastOneExists()
            {
                var entity1 = new TestEntity(1);
                var entity2 = new TestEntity(2) { Text = "Hello" };
                var entity3 = new TestEntity(3);

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var exists = this.SyncedCollection.Exists(x => x.Text == "Hello");

                Assert.IsTrue(exists);
            }

            [Test]
            public void ShouldReturnFalseIfNonExists()
            {
                var entity1 = new TestEntity(1);
                var entity2 = new TestEntity(2);
                var entity3 = new TestEntity(3);

                this.NativeCollection.Insert(entity1);
                this.NativeCollection.Insert(entity2);
                this.NativeCollection.Insert(entity3);

                var exists = this.SyncedCollection.Exists(x => x.Text == "Hello");

                Assert.IsFalse(exists);
            }
        }
    }
}