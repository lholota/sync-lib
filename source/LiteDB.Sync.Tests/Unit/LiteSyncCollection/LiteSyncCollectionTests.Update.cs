using System.Linq;
using LiteDB.Sync.Tests.TestUtils;
using NUnit.Framework;

namespace LiteDB.Sync.Tests.Unit.LiteSyncCollection
{
    public partial class LiteSyncCollectionTests
    {
		public class WhenUpdatingSingle : LiteSyncCollectionTests
        {
		    [Test]
		    public void ShouldSetRequiresSync()
		    {
		        var entity = new TestEntity(1);
		        this.NativeCollection.Insert(entity);

		        this.SyncedCollection.Upsert(entity);

		        var found = this.NativeCollection.FindOne(Query.All());
		        Assert.AreEqual(1, found.Id);
		        Assert.IsTrue(found.RequiresSync);
		    }
        }

        public class WhenUpdatingSingleWithExplicitId : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldSetRequiresSync()
            {
                var entity = new TestEntity();
                this.NativeCollection.Insert(1, entity);

                this.SyncedCollection.Update(1, entity);

                var found = this.NativeCollection.FindOne(Query.All());
                Assert.AreEqual(1, found.Id);
                Assert.IsTrue(found.RequiresSync);
            }
        }

        public class WhenUpdatingMultiple : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldSetRequiresSync()
            {
                var entities = new[]
                {
                    new TestEntity(1),
                    new TestEntity(2)
                };

                this.NativeCollection.Insert(entities);

                entities[0].Text = "Hello";
                entities[1].Text = "Hello";

                this.SyncedCollection.Upsert(entities);

                var found = this.NativeCollection.FindAll().ToArray();
                Assert.AreEqual(2, found.Length);
                Assert.IsTrue(found.All(x => x.Text == "Hello"));
                Assert.IsTrue(found.All(x => x.RequiresSync));
            }
        }
    }
}