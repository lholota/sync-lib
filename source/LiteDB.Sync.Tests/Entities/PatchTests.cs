using System.Linq;
using LiteDB.Sync.Internal;
using LiteDB.Sync.Tests.Tools;
using NUnit.Framework;

namespace LiteDB.Sync.Tests.Entities
{
    [TestFixture]
    public class PatchTests
    {
        protected const string CollectionName = "MyCollection";

        public class WhenSerializing : PatchTests
        {
            [Test]
            public void ShouldSerializeAndDeserialize()
            {
                var expected = CreateSamplePatch();

                var serialized = BsonMapper.Global.ToDocument(expected);
                var actual = BsonMapper.Global.ToObject<Patch>(serialized);

                Assert.IsNotNull(actual);
                Assert.IsNotNull(actual.Changes);
                Assert.AreEqual(expected.Changes.Count, actual.Changes.Count);

                var firstExpected = expected.Changes.First();
                var firstActual = actual.Changes.First();

                Assert.AreEqual(firstExpected.Key.EntityId, firstActual.Key.EntityId);
                Assert.AreEqual(firstExpected.Value.Entity["Text"], firstActual.Value.Entity["Text"]);
                Assert.AreEqual(firstExpected.Value.ChangeType, firstActual.Value.ChangeType);

                var secondExpected = expected.Changes.Skip(1).First();
                var secondActual = actual.Changes.Skip(1).First();

                Assert.AreEqual(secondExpected.Key.EntityId, secondActual.Key.EntityId);
                Assert.AreEqual(secondExpected.Value.ChangeType, secondActual.Value.ChangeType);
            }

            private static Patch CreateSamplePatch()
            {
                var patch = new Patch();

                var docs = new[]
                {
                    GetDocument(123, "AAA")
                };

                patch.AddChanges(CollectionName, docs);

                var deleted = new[]
                {
                    new DeletedEntity(CollectionName, 456)
                };

                patch.AddDeletes(deleted);

                return patch;
            }
        }

        public class WhenAddingChanges : PatchTests
        {
            [Test]
            public void ShouldContainAddedChanges()
            {
                var document = GetDocument(123);

                var patch = new Patch();
                patch.AddChanges(CollectionName, new[] { document });

                var operation = patch.Changes.Single();

                Assert.AreEqual(123, operation.Key.EntityId.AsInt32);
                Assert.AreEqual(CollectionName, operation.Key.CollectionName);

                Assert.AreEqual(EntityChangeType.Upsert, operation.Value.ChangeType);
                Assert.AreEqual(document, operation.Value.Entity);
            }

            [Test]
            public void ShouldContainAddedDeletes()
            {
                var deletedEntity = new DeletedEntity(CollectionName, 123);

                var patch = new Patch();
                patch.AddDeletes(new []{ deletedEntity });

                var operation = patch.Changes.Single();

                Assert.AreEqual(123, operation.Key.EntityId.AsInt32);
                Assert.AreEqual(EntityChangeType.Delete, operation.Value.ChangeType);
                Assert.IsNull(operation.Value.Entity);
                Assert.AreEqual(CollectionName, operation.Key.CollectionName);
            }
        }

        public class WhenCombiningPatches : PatchTests
        {
            [Test]
            public void ShouldContainLastChangeWhenEntityWasChangedMultipleTimes()
            {
                var patches = new[]
                {
                    this.CreatePatch(EntityChangeType.Upsert, "Value1"),
                    this.CreatePatch(EntityChangeType.Upsert, "Value2")
                };

                var combined = Patch.Combine(patches);

                Assert.AreEqual(1, combined.Changes.Count());

                var operation = combined.Changes.Single();

                Assert.AreEqual(EntityChangeType.Upsert, operation.OperationType);
                Assert.AreEqual(123, operation.EntityId.AsInt32);

                BsonValue actualStringPropValue;
                operation.Entity.TryGetValue(nameof(TestEntity.Text), out actualStringPropValue);

                Assert.IsNotNull(actualStringPropValue);
                Assert.AreEqual("Value2", actualStringPropValue.ToString());
            }

            [Test]
            public void ShouldContainDeleteIfEntityWasLastDeleted()
            {
                var patches = new[]
                {
                    this.CreatePatch(EntityChangeType.Upsert),
                    this.CreatePatch(EntityChangeType.Delete)
                };

                var combined = Patch.Combine(patches);

                Assert.AreEqual(1, combined.Changes.Count());

                var operation = combined.Changes.Single();

                Assert.AreEqual(EntityChangeType.Delete, operation.OperationType);
                Assert.AreEqual(123, operation.EntityId.AsInt32);
            }

            [Test]
            public void ShouldContainChangeIfEntityWasRecreated()
            {
                var patches = new[]
                {
                    this.CreatePatch(EntityChangeType.Delete),
                    this.CreatePatch(EntityChangeType.Upsert)
                };

                var combined = Patch.Combine(patches);

                Assert.AreEqual(1, combined.Changes.Count());

                var operation = combined.Changes.Single();

                Assert.AreEqual(EntityChangeType.Upsert, operation.OperationType);
                Assert.AreEqual(123, operation.EntityId.AsInt32);
            }

            [Test]
            public void ShouldNotMergeSameIdsInDifferentCollections()
            {
                var patches = new[]
                {
                    this.CreatePatch(EntityChangeType.Upsert, collectionName:CollectionName),
                    this.CreatePatch(EntityChangeType.Upsert, collectionName:CollectionName + "Another")
                };

                var combined = Patch.Combine(patches);

                Assert.AreEqual(2, combined.Changes.Count());
            }

            private Patch CreatePatch(EntityChangeType opType, string stringPropValue = null, string collectionName = null)
            {
                var result = new Patch();

                if (opType == EntityChangeType.Upsert)
                {
                    var entity = new TestEntity(123)
                    {
                        Text = stringPropValue
                    };

                    var bsonDoc = BsonMapper.Global.ToDocument(entity);

                    result.AddChanges(collectionName ?? CollectionName, new []{ bsonDoc });
                }
                else
                {
                    var deletedEntity = new DeletedEntity(collectionName ?? CollectionName, 123);

                    result.AddDeletes(new []{deletedEntity});
                }

                return result;
            }
        }

        protected static BsonDocument GetDocument(int id, string stringPropValue = null, bool requiresSync = true)
        {
            return BsonMapper.Global.ToDocument(new TestEntity
            {
                Id = id,
                Text = stringPropValue,
                RequiresSync = requiresSync
            });
        }
    }
}