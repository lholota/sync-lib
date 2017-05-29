using System.Collections.Generic;
using System.Linq;
using LiteDB.Sync.Contract;
using LiteDB.Sync.Tests.Tools;
using NUnit.Framework;

namespace LiteDB.Sync.Tests.Entities
{
    // TODO: Change the logic, no deleted entities anymore...

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
                Assert.AreEqual(expected.Operations.Count, actual.Operations.Count);
                Assert.AreEqual(expected.Operations[0].EntityId, actual.Operations[0].EntityId);
                Assert.AreEqual(expected.Operations[0].Entity["Text"], actual.Operations[0].Entity["Text"]);
                Assert.AreEqual(expected.Operations[0].OperationType, actual.Operations[0].OperationType);

                Assert.AreEqual(expected.Operations[1].EntityId, actual.Operations[1].EntityId);
                Assert.AreEqual(expected.Operations[1].OperationType, actual.Operations[1].OperationType);
            }

            private static Patch CreateSamplePatch()
            {
                var patch = new Patch();
                patch.Operations = new List<EntityOperation>();

                patch.Operations.Add(new EntityOperation
                {
                    Entity = GetDocument(123, 1),
                    EntityId = new BsonValue(1),
                    OperationType = EntityOperationType.Upsert
                });

                patch.Operations.Add(new EntityOperation
                {
                    EntityId = new BsonValue(2),
                    OperationType = EntityOperationType.Delete
                });
                return patch;
            }
        }

        public class WhenAddingChanges : PatchTests
        {
            [Test]
            public void ShouldContainAddedChanges()
            {
                var document = GetDocument(123, 1);

                var patch = new Patch();
                patch.AddChanges(CollectionName, new[] { document });

                var operation = patch.Operations.Single();

                Assert.AreEqual(123, operation.EntityId);
                Assert.AreEqual(EntityOperationType.Upsert, operation.OperationType);
                Assert.AreEqual(document, operation.Entity);
                Assert.AreEqual(CollectionName, operation.CollectionName);
            }

            [Test]
            public void ShouldNotContainPayloadIfEntityDeleted()
            {
                var changedEntity = GetDocument(123, 1, syncState:EntitySyncState.RequiresSyncDeleted);

                var patch = new Patch();
                patch.AddChanges(CollectionName, new []{ changedEntity });

                var operation = patch.Operations.Single();

                Assert.AreEqual(123, operation.EntityId);
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

                Assert.AreEqual(1, combined.Operations.Count);
                Assert.AreEqual(EntityOperationType.Upsert, combined.Operations[0].OperationType);
                Assert.AreEqual(123, combined.Operations[0].EntityId);

                BsonValue actualStringPropValue;
                combined.Operations[0].Entity.TryGetValue(nameof(TestEntity.Text), out actualStringPropValue);

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

                Assert.AreEqual(1, combined.Operations.Count);
                Assert.AreEqual(EntityOperationType.Delete, combined.Operations[0].OperationType);
                Assert.AreEqual(123, combined.Operations[0].EntityId);
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

                Assert.AreEqual(1, combined.Operations.Count);
                Assert.AreEqual(EntityOperationType.Upsert, combined.Operations[0].OperationType);
                Assert.AreEqual(123, combined.Operations[0].EntityId);
            }

            private Patch CreatePatch(EntityOperationType opType, string stringPropValue = null)
            {
                var result = new Patch();

                result.Operations.Add(new EntityOperation
                {
                    EntityId = 123,
                    CollectionName = CollectionName,
                    OperationType = opType
                });

                if (opType == EntityOperationType.Upsert)
                {
                    result.Operations[0].Entity = GetDocument(1, 1, stringPropValue);
                }

                return result;
            }
        }

        protected static BsonDocument GetDocument(int id, int changeTime, string stringPropValue = null, EntitySyncState syncState = EntitySyncState.RequiresSync)
        {
            return BsonMapper.Global.ToDocument(new TestEntity
            {
                Id = id,
                Text = stringPropValue
            });
        }
    }
}