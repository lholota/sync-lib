using System.Collections.Generic;
using System.Linq;
using LiteDB.Sync.Tests.TestUtils;
using NUnit.Framework;

namespace LiteDB.Sync.Tests.Core.LiteSyncCollection
{
    public partial class LiteSyncCollectionTests
    {
        public class WhenInsertingSingle : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldInsertItem()
            {
                var entity = new TestEntity(1);
                this.SyncedCollection.Insert(entity);

                var found = this.NativeCollection.FindOne(Query.All());
                Assert.AreEqual(1, found.Id);
                Assert.IsTrue(found.RequiresSync);
            }

            [Test]
            public void ShouldRemoveDeletedEntity()
            {
                this.InsertDeletedEntity(1);
                this.VerifyDeletedEntityExists(1);

                var entity = new TestEntity(1);
                this.SyncedCollection.Insert(entity);

                this.VerifyDeletedEntitiesEmpty();
                Assert.AreEqual(1, this.NativeCollection.Count());
            }
        }

        public class WhenInsertingSingleWithExplicitId : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldInsertItem()
            {
                var entity = new TestEntity();
                this.SyncedCollection.Insert(new BsonValue(2), entity);

                var found = this.NativeCollection.FindOne(Query.All());
                Assert.AreEqual(2, found.Id);
                Assert.IsTrue(found.RequiresSync);
            }

            [Test]
            public void ShouldRemoveDeletedEntity()
            {
                this.InsertDeletedEntity(2);
                this.VerifyDeletedEntityExists(2);

                var entity = new TestEntity();
                this.SyncedCollection.Insert(new BsonValue(2), entity);

                this.VerifyDeletedEntitiesEmpty();
                Assert.AreEqual(1, this.NativeCollection.Count());
            }
        }

        public class WhenInsertingMultiple : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldInsertItems()
            {
                var entities = new[]
                {
                    new TestEntity(1), 
                    new TestEntity(2)
                };

                var inserted = this.SyncedCollection.Insert(entities);
                Assert.AreEqual(2, inserted);

                var found = this.NativeCollection.FindAll().ToArray();
                Assert.AreEqual(2, found.Length);
                Assert.IsTrue(found.All(x => x.RequiresSync));
            }

            [Test]
            public void ShouldRemoveDeletedEntity()
            {
                this.InsertDeletedEntity(1);
                this.VerifyDeletedEntityExists(1);

                var entities = new[]
                {
                    new TestEntity(1),
                    new TestEntity(2)
                };

                var inserted = this.SyncedCollection.Insert(entities);
                Assert.AreEqual(2, inserted);

                this.VerifyDeletedEntitiesEmpty();
                Assert.AreEqual(2, this.NativeCollection.Count());
            }
        }

        public class WhenInsertingBulk : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldInsertItems()
            {
                var entities = this.CreateBulk(200);

                var inserted = this.SyncedCollection.InsertBulk(entities, 100);
                Assert.AreEqual(200, inserted);

                var found = this.NativeCollection.FindAll().ToArray();
                Assert.AreEqual(200, found.Length);
                Assert.IsTrue(found.All(x => x.RequiresSync));
            }

            [Test]
            public void ShouldRemoveDeletedEntities()
            {
                this.InsertDeletedEntity(1);
                this.InsertDeletedEntity(150);

                var entities = this.CreateBulk(200);

                var inserted = this.SyncedCollection.InsertBulk(entities, 100);
                Assert.AreEqual(200, inserted);

                this.VerifyDeletedEntitiesEmpty();
                Assert.AreEqual(200, this.NativeCollection.Count());
            }

            [Test]
            public void ShouldRemoveDeletedEntitiesOnPartialFailure()
            {
                // This will cause a PK conflict during the second batch
                this.NativeCollection.Insert(new TestEntity(110));

                this.InsertDeletedEntity(1);
                this.InsertDeletedEntity(115);

                var entities = this.CreateBulk(200);

                Assert.Throws<LiteException>(() => this.SyncedCollection.InsertBulk(entities, 100));

                this.VerifyDeletedEntityExists(115);
                Assert.AreEqual(100 + 1, this.NativeCollection.Count());
            }

            private IList<TestEntity> CreateBulk(int count)
            {
                var result = new List<TestEntity>();

                for (var i = 0; i < count; i++)
                {
                    result.Add(new TestEntity(i + 1));
                }

                return result;
            }
        }
    }
}