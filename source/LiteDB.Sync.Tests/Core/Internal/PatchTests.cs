using System;
using System.Linq;
using LiteDB.Sync.Internal;
using LiteDB.Sync.Tests.TestUtils;
using Newtonsoft.Json;
using NUnit.Framework;

namespace LiteDB.Sync.Tests.Core.Internal
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

                var settings = new JsonSerializerSettings();
                settings.Formatting = Formatting.Indented;

                var serialized = JsonConvert.SerializeObject(expected, settings);
                Console.WriteLine(serialized);

                var actual = JsonConvert.DeserializeObject<Patch>(serialized, JsonSerialization.Settings);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.NextPatchId, actual.NextPatchId);

                var expectedChanges = expected.Changes.ToArray();
                var actualChanges = actual.Changes.ToArray();

                Assert.AreEqual(expectedChanges.Length, actualChanges.Length);

                var firstExpected = expectedChanges.First();
                var firstActual = actualChanges.First();

                Assert.AreEqual(firstExpected.EntityId, firstActual.EntityId);
                Assert.AreEqual(firstExpected.Entity["Text"], firstActual.Entity["Text"]);
                Assert.AreEqual(firstExpected.ChangeType, firstActual.ChangeType);

                var secondExpected = expectedChanges.Skip(1).First();
                var secondActual = actualChanges.Skip(1).First();

                Assert.AreEqual(secondExpected.EntityId, secondActual.EntityId);
                Assert.AreEqual(secondExpected.ChangeType, secondActual.ChangeType);
            }

            private static Patch CreateSamplePatch()
            {
                var patch = new Patch();
                patch.NextPatchId = "Next";

                var docs = new[]
                {
                    GetDocument(123, "AAA")
                    // TBA: Document with complex unknown id type
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

                var change = patch.Changes.Single();

                Assert.AreEqual(123, change.EntityId.Id);
                Assert.AreEqual(CollectionName, change.EntityId.CollectionName);

                Assert.AreEqual(EntityChangeType.Upsert, change.ChangeType);
                Assert.AreEqual(document, change.Entity);
            }

            [Test]
            public void ShouldContainAddedDeletes()
            {
                var deletedEntity = new DeletedEntity(CollectionName, 123);

                var patch = new Patch();
                patch.AddDeletes(new[] { deletedEntity });

                var change = patch.Changes.Single();

                Assert.AreEqual(123, change.EntityId.Id);
                Assert.AreEqual(CollectionName, change.EntityId.CollectionName);

                Assert.AreEqual(EntityChangeType.Delete, change.ChangeType);
                Assert.IsNull(change.Entity);
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

                var change = combined.Changes.Single();

                Assert.AreEqual(EntityChangeType.Upsert, change.ChangeType);
                Assert.AreEqual(123, change.EntityId.Id);

                BsonValue actualStringPropValue;
                change.Entity.TryGetValue(nameof(TestEntity.Text), out actualStringPropValue);

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

                var change = combined.Changes.Single();

                Assert.AreEqual(EntityChangeType.Delete, change.ChangeType);
                Assert.AreEqual(123, change.EntityId.Id);
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

                var change = combined.Changes.Single();

                Assert.AreEqual(EntityChangeType.Upsert, change.ChangeType);
                Assert.AreEqual(123, change.EntityId.Id);
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

                    result.AddChanges(collectionName ?? CollectionName, new[] { bsonDoc });
                }
                else
                {
                    var deletedEntity = new DeletedEntity(collectionName ?? CollectionName, 123);

                    result.AddDeletes(new[] { deletedEntity });
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