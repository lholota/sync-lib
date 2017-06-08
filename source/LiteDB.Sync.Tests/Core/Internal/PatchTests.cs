using System;
using System.Collections.Generic;
using System.IO;
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
                var serializer = JsonSerialization.CreateSerializer();

                Patch actual;

                using (var ms = new MemoryStream())
                using (var writer = new StreamWriter(ms))
                using (var reader = new StreamReader(ms))
                using (var jsonReader = new JsonTextReader(reader))
                {
                    serializer.Serialize(writer, expected);

                    writer.Flush();
                    ms.Position = 0;

                    actual = serializer.Deserialize<Patch>(jsonReader);
                }

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.NextPatchId, actual.NextPatchId);

                var expectedChanges = expected.Changes.ToArray();
                var actualChanges = actual.Changes.ToArray();

                Assert.AreEqual(expectedChanges.Length, actualChanges.Length);

                var firstExpected = (UpsertEntityChange)expectedChanges.First();
                var firstActual = (UpsertEntityChange)actualChanges.First();

                Assert.AreEqual(firstExpected.EntityId, firstActual.EntityId);
                Assert.AreEqual(firstExpected.Entity["Text"], firstActual.Entity["Text"]);

                var secondExpected = (UpsertEntityChange)expectedChanges.Skip(1).First();
                var secondActual = (UpsertEntityChange)actualChanges.Skip(1).First();

                Assert.AreEqual(secondExpected.EntityId, secondActual.EntityId);
                Assert.AreEqual(secondExpected.Entity["Text"], secondActual.Entity["Text"]);

                var thirdExpected = expectedChanges.Skip(2).First();
                var thirdActual = actualChanges.Skip(2).First();

                Assert.IsInstanceOf<DeleteEntityChange>(thirdActual);
                Assert.AreEqual(thirdExpected.EntityId, thirdActual.EntityId);
            }

            private static Patch CreateSamplePatch()
            {
                var patch = new Patch();
                patch.NextPatchId = "Next";

                var docs = new[]
                {
                    GetDocument(1, "AAA"),
                    GetDocument(2, null)
                    // TBA: Document with complex unknown id type
                };

                patch.AddUpsertChanges(CollectionName, docs);

                var deleted = new[]
                {
                    new DeletedEntity(CollectionName, 3)
                };

                patch.AddDeleteChanges(deleted);

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
                patch.AddUpsertChanges(CollectionName, new[] { document });

                var change = patch.Changes.Single();

                Assert.AreEqual(123, change.EntityId.Id);
                Assert.AreEqual(CollectionName, change.EntityId.CollectionName);

                var upsertEntityChange = (UpsertEntityChange)change;
                Assert.AreEqual(document, upsertEntityChange.Entity);
            }

            [Test]
            public void ShouldContainAddedDeletes()
            {
                var deletedEntity = new DeletedEntity(CollectionName, 123);

                var patch = new Patch();
                patch.AddDeleteChanges(new[] { deletedEntity });

                var change = patch.Changes.Single();

                Assert.AreEqual(123, change.EntityId.Id);
                Assert.AreEqual(CollectionName, change.EntityId.CollectionName);

                Assert.IsInstanceOf<DeleteEntityChange>(change);
            }
        }

        public class WhenCombiningPatches : PatchTests
        {
            [Test]
            public void ShouldContainLastChangeWhenEntityWasChangedMultipleTimes()
            {
                var patches = new[]
                {
                    this.CreatePatchWithUpsert("Value1"),
                    this.CreatePatchWithUpsert("Value2")
                };

                var combined = Patch.Combine(patches);

                Assert.AreEqual(1, combined.Changes.Count());

                var change = combined.Changes.Single();

                Assert.IsInstanceOf<UpsertEntityChange>(change);
                Assert.AreEqual(123, change.EntityId.Id);

                BsonValue actualStringPropValue;
                var upsertChange = (UpsertEntityChange)change;

                upsertChange.Entity.TryGetValue(nameof(TestEntity.Text), out actualStringPropValue);

                Assert.IsNotNull(actualStringPropValue);
                Assert.AreEqual("Value2", actualStringPropValue.ToString());
            }

            [Test]
            public void ShouldContainDeleteIfEntityWasLastDeleted()
            {
                var patches = new[]
                {
                    this.CreatePatchWithUpsert(),
                    this.CreatePatchWithDelete()
                };

                var combined = Patch.Combine(patches);

                Assert.AreEqual(1, combined.Changes.Count());

                var change = combined.Changes.Single();

                Assert.IsInstanceOf<DeleteEntityChange>(change);
                Assert.AreEqual(123, change.EntityId.Id);
            }

            [Test]
            public void ShouldContainChangeIfEntityWasRecreated()
            {
                var patches = new[]
                {
                    this.CreatePatchWithDelete(),
                    this.CreatePatchWithUpsert()
                };

                var combined = Patch.Combine(patches);

                Assert.AreEqual(1, combined.Changes.Count());

                var change = combined.Changes.Single();

                Assert.IsInstanceOf<UpsertEntityChange>(change);
                Assert.AreEqual(123, change.EntityId.Id);
            }

            [Test]
            public void ShouldNotMergeSameIdsInDifferentCollections()
            {
                var patches = new[]
                {
                    this.CreatePatchWithUpsert(collectionName:CollectionName),
                    this.CreatePatchWithUpsert(collectionName:CollectionName + "Another")
                };

                var combined = Patch.Combine(patches);

                Assert.AreEqual(2, combined.Changes.Count());
            }

            private Patch CreatePatchWithUpsert(string stringPropValue = null, string collectionName = null)
            {
                var result = new Patch();

                var entity = new TestEntity(123)
                {
                    Text = stringPropValue
                };

                var bsonDoc = BsonMapper.Global.ToDocument(entity);

                result.AddUpsertChanges(collectionName ?? CollectionName, new[] { bsonDoc });

                return result;
            }

            private Patch CreatePatchWithDelete(string collectionName = null)
            {
                var result = new Patch();
                var deletedEntity = new DeletedEntity(collectionName ?? CollectionName, 123);

                result.AddDeleteChanges(new[] { deletedEntity });

                return result;
            }
        }

        public class WhenGettingHasChanges : PatchTests
        {
            [Test]
            public void ShouldReturnFalseIfHasZeroChanges()
            {
                var patch = new Patch();

                Assert.IsFalse(patch.HasChanges);
            }

            [Test]
            public void ShouldReturnTrueIfHasChanges()
            {
                var patch = new Patch();
                var doc = new BsonDocument();
                doc["_id"] = new BsonValue(1);

                patch.AddUpsertChanges("MyColl", new[] { doc });

                Assert.IsTrue(patch.HasChanges);
            }
        }

        public class WhenGettingConflicts : PatchTests
        {
            [Test]
            public void ShouldNotReturnConflictWithDeletesOnBothSides()
            {
                var localChange = new DeleteEntityChange(new EntityId("MyColl", 1));
                var remoteChange = new DeleteEntityChange(new EntityId("MyColl", 1));

                var localPatch = new Patch(new[] { localChange });
                var remotePatch = new Patch(new[] { remoteChange });

                var conflicts = Patch.GetConflicts(localPatch, remotePatch);

                Assert.AreEqual(0, conflicts.Count);
            }

            [Test]
            public void ShouldNotReturnConflictWithSameUpsertOnBothSides()
            {
                var localDoc = new BsonDocument();
                var remoteDoc = new BsonDocument();

                localDoc["key"] = "value";
                remoteDoc["key"] = "value";

                var localChange = new UpsertEntityChange(new EntityId("MyColl", 1), localDoc);
                var remoteChange = new UpsertEntityChange(new EntityId("MyColl", 1), remoteDoc);

                var localPatch = new Patch(new[] { localChange });
                var remotePatch = new Patch(new[] { remoteChange });

                var conflicts = Patch.GetConflicts(localPatch, remotePatch);

                Assert.AreEqual(0, conflicts.Count);
            }

            [Test]
            public void ShouldReturnConflictWithDifferentChangeTypes()
            {
                var localDoc = new BsonDocument();

                localDoc["key"] = "value";

                var localChange = new UpsertEntityChange(new EntityId("MyColl", 1), localDoc);
                var remoteChange = new DeleteEntityChange(new EntityId("MyColl", 1));

                var localPatch = new Patch(new[] { localChange });
                var remotePatch = new Patch(new[] { remoteChange });

                var conflicts = Patch.GetConflicts(localPatch, remotePatch);

                Assert.AreEqual(1, conflicts.Count);
                Assert.AreEqual(localChange, conflicts[0].LocalChange);
                Assert.AreEqual(remoteChange, conflicts[0].RemoteChange);
            }

            [Test]
            public void ShouldReturnConflictWithDifferentValues()
            {
                var localDoc = new BsonDocument();
                var remoteDoc = new BsonDocument();

                localDoc["key"] = "value";
                remoteDoc["key"] = "different";

                var localChange = new UpsertEntityChange(new EntityId("MyColl", 1), localDoc);
                var remoteChange = new UpsertEntityChange(new EntityId("MyColl", 1), remoteDoc);

                var localPatch = new Patch(new[] { localChange });
                var remotePatch = new Patch(new[] { remoteChange });

                var conflicts = Patch.GetConflicts(localPatch, remotePatch);

                Assert.AreEqual(1, conflicts.Count);
                Assert.AreEqual(localChange, conflicts[0].LocalChange);
                Assert.AreEqual(remoteChange, conflicts[0].RemoteChange);
            }

            [Test]
            public void ShouldNotReturnConflictWhenDifferentEntitiesChanged()
            {
                var localDoc = new BsonDocument();
                var remoteDoc = new BsonDocument();

                localDoc["key"] = "value";
                remoteDoc["key"] = "different";

                var localChange = new UpsertEntityChange(new EntityId("MyColl", 1), localDoc);
                var remoteChange = new UpsertEntityChange(new EntityId("MyColl", 55), remoteDoc);

                var localPatch = new Patch(new[] { localChange });
                var remotePatch = new Patch(new[] { remoteChange });

                var conflicts = Patch.GetConflicts(localPatch, remotePatch);

                Assert.AreEqual(0, conflicts.Count);
            }
        }

        public class WhenRemovingChange : PatchTests
        {
            [Test]
            public void ShouldRemoveTheChangeByEntityId()
            {
                var changes = new[]
                {
                    new DeleteEntityChange(new EntityId("Collection", 1)),
                };

                var patch = new Patch(changes);

                patch.RemoveChange(new EntityId("Collection", 1));

                Assert.IsFalse(patch.Changes.Any());
            }

            [Test]
            public void ShouldThrowIfEntityIdIsNotFound()
            {
                var patch = new Patch();

                Assert.Throws<KeyNotFoundException>(() => patch.RemoveChange(new EntityId("Collection", 1)));
            }
        }

        public class WhenReplacingEntity : PatchTests
        {
            [Test]
            public void ShouldReplaceChangeById()
            {
                var originalDoc = new BsonDocument();
                var replacingDoc = new BsonDocument();

                originalDoc["key"] = "original";
                replacingDoc["key"] = "different";

                var id = new EntityId(CollectionName, 1);

                var changes = new[]
                {
                    new UpsertEntityChange(id, originalDoc)
                };

                var patch = new Patch(changes);

                patch.ReplaceChange(id, replacingDoc);

                var replacingChange = patch.Changes
                    .OfType<UpsertEntityChange>()
                    .Single(x => x.EntityId == id);

                Assert.AreEqual(replacingDoc, replacingChange.Entity);
            }

            [Test]
            public void ShouldThrowIfEntityIdIsNotFound()
            {
                var patch = new Patch();

                Assert.Throws<KeyNotFoundException>(() =>
                {
                    patch.ReplaceChange(new EntityId("Collection", 1), new BsonDocument());
                });
            }
        }

        // TBA: ReplaceEntity

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