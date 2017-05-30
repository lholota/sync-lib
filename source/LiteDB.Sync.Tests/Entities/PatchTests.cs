using System.Linq;
using LiteDB.Sync.Contract;
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
                Assert.IsNotNull(actual.Operations);
                Assert.AreEqual(expected.Operations.Count(), actual.Operations.Count());
                Assert.AreEqual(expected.Operations.First().EntityId, actual.Operations.First().EntityId);
                Assert.AreEqual(expected.Operations.First().Entity["Text"], actual.Operations.First().Entity["Text"]);
                Assert.AreEqual(expected.Operations.First().OperationType, actual.Operations.First().OperationType);

                Assert.AreEqual(expected.Operations.Skip(1).First().EntityId, actual.Operations.Skip(1).First().EntityId);
                Assert.AreEqual(expected.Operations.Skip(1).First().OperationType, actual.Operations.Skip(1).First().OperationType);
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

                var operation = patch.Operations.Single();

                Assert.AreEqual(123, operation.EntityId.AsInt32);
                Assert.AreEqual(EntityOperationType.Upsert, operation.OperationType);
                Assert.AreEqual(document, operation.Entity);
                Assert.AreEqual(CollectionName, operation.CollectionName);
            }

            [Test]
            public void ShouldNotContainPayloadIfEntityDeleted()
            {
                var deletedEntity = new DeletedEntity(CollectionName, 123);

                var patch = new Patch();
                patch.AddDeletes(new []{ deletedEntity });

                var operation = patch.Operations.Single();

                Assert.AreEqual(123, operation.EntityId.AsInt32);
                Assert.AreEqual(EntityOperationType.Delete, operation.OperationType);
                Assert.IsNull(operation.Entity);
                Assert.AreEqual(CollectionName, operation.CollectionName);
            }
        }

        public class WhenCombiningPatches : PatchTests
        {
            [Test]
            public void ShouldContainLastChangeWhenEntityWasChangedMultipleTimes()
            {
                var patches = new[]
                {
                    this.CreatePatch(EntityOperationType.Upsert, "Value1"),
                    this.CreatePatch(EntityOperationType.Upsert, "Value2")
                };

                var combined = Patch.Combine(patches);

                Assert.AreEqual(1, combined.Operations.Count());

                var operation = combined.Operations.Single();

                Assert.AreEqual(EntityOperationType.Upsert, operation.OperationType);
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
                    this.CreatePatch(EntityOperationType.Upsert),
                    this.CreatePatch(EntityOperationType.Delete)
                };

                var combined = Patch.Combine(patches);

                Assert.AreEqual(1, combined.Operations.Count());

                var operation = combined.Operations.Single();

                Assert.AreEqual(EntityOperationType.Delete, operation.OperationType);
                Assert.AreEqual(123, operation.EntityId.AsInt32);
            }

            [Test]
            public void ShouldContainChangeIfEntityWasRecreated()
            {
                var patches = new[]
                {
                    this.CreatePatch(EntityOperationType.Delete),
                    this.CreatePatch(EntityOperationType.Upsert)
                };

                var combined = Patch.Combine(patches);

                Assert.AreEqual(1, combined.Operations.Count());

                var operation = combined.Operations.Single();

                Assert.AreEqual(EntityOperationType.Upsert, operation.OperationType);
                Assert.AreEqual(123, operation.EntityId.AsInt32);
            }

            [Test]
            public void ShouldNotMergeSameIdsInDifferentCollections()
            {
                var patches = new[]
                {
                    this.CreatePatch(EntityOperationType.Upsert, collectionName:CollectionName),
                    this.CreatePatch(EntityOperationType.Upsert, collectionName:CollectionName + "Another")
                };

                var combined = Patch.Combine(patches);

                Assert.AreEqual(2, combined.Operations.Count());
            }

            private Patch CreatePatch(EntityOperationType opType, string stringPropValue = null, string collectionName = null)
            {
                var result = new Patch();

                if (opType == EntityOperationType.Upsert)
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