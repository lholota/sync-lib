using System.Linq;
using LiteDB.Sync.Tests.TestUtils;
using NUnit.Framework;

namespace LiteDB.Sync.Tests.Unit.LiteSyncCollection
{
    public partial class LiteSyncCollectionTests
    {
		public class WhenUpsertingSingle : LiteSyncCollectionTests
        {
		    [Test]
		    public void ShouldInsertNonExistingItem()
		    {
		        var entity = new TestEntity(1);
		        this.SyncedCollection.Upsert(entity);

		        var found = this.NativeCollection.FindOne(Query.All());
		        Assert.AreEqual(1, found.Id);
		        Assert.IsTrue(found.RequiresSync);
		    }

		    [Test]
		    public void ShouldUpdateExistingItem()
		    {
		        var entity = new TestEntity(1);
		        this.NativeCollection.Insert(entity);

		        entity.Text = "Hello";
		        this.SyncedCollection.Upsert(entity);

		        var found = this.NativeCollection.FindOne(Query.All());
		        Assert.AreEqual(1, found.Id);
		        Assert.AreEqual("Hello", found.Text);
		        Assert.IsTrue(found.RequiresSync);
            }

            [Test]
		    public void ShouldRemoveDeletedEntity()
		    {
		        this.InsertDeletedEntity(1);

		        var entity = new TestEntity(1);
		        this.SyncedCollection.Upsert(entity);

		        this.VerifyDeletedEntitiesEmpty();
		        Assert.AreEqual(1, this.NativeCollection.Count());
		    }
        }

        public class WhenUpsertingSingleWithExplicitId : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldInsertNonExistingItem()
            {
                var entity = new TestEntity();
                this.SyncedCollection.Upsert(1, entity);

                var found = this.NativeCollection.FindOne(Query.All());
                Assert.AreEqual(1, found.Id);
                Assert.IsTrue(found.RequiresSync);
            }

            [Test]
            public void ShouldUpdateExistingItem()
            {
                var entity = new TestEntity(1);
                this.NativeCollection.Insert(entity);

                entity.Text = "Hello";
                this.SyncedCollection.Upsert(1, entity);

                var found = this.NativeCollection.FindOne(Query.All());
                Assert.AreEqual(1, found.Id);
                Assert.AreEqual("Hello", found.Text);
                Assert.IsTrue(found.RequiresSync);
            }

            [Test]
            public void ShouldRemoveDeletedEntity()
            {
                this.InsertDeletedEntity(1);

                var entity = new TestEntity();
                this.SyncedCollection.Upsert(1, entity);

                this.VerifyDeletedEntitiesEmpty();
                Assert.AreEqual(1, this.NativeCollection.Count());
            }
        }

        public class WhenUpsertingMultiple : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldInsertNonExistingItem()
            {
                var entities = new[]
                {
                    new TestEntity(1),
                    new TestEntity(2)
                };

                this.SyncedCollection.Upsert(entities);

                var found = this.NativeCollection.FindAll().ToArray();
                Assert.AreEqual(2, found.Length);
                Assert.IsTrue(found.All(x => x.RequiresSync));
            }

            [Test]
            public void ShouldUpdateExistingItem()
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

            [Test]
            public void ShouldRemoveDeletedEntity()
            {
                this.InsertDeletedEntity(1);

                var entities = new[]
                {
                    new TestEntity(1),
                    new TestEntity(2)
                };

                this.SyncedCollection.Upsert(entities);

                this.VerifyDeletedEntitiesEmpty();
                Assert.AreEqual(2, this.NativeCollection.Count());
            }
        }
    }
}