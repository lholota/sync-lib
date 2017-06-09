using System;
using System.Collections.Generic;
using LiteDB.Sync.Internal;
using NUnit.Framework;

namespace LiteDB.Sync.Tests.Core
{
    [TestFixture]
    public class LiteSyncConflictTests
    {
        public class WhenCreating : LiteSyncConflictTests
        {
            [Test]
            public void ShouldFailIfChangesHaveDifferentEntityId()
            {
                var local = this.CreateChange(2);
                var remote = this.CreateChange(3);

                // ReSharper disable once ObjectCreationAsStatement
                var ex = Assert.Throws<ArgumentException>(() => new LiteSyncConflict(local, remote));

                Assert.IsTrue(ex.Message.Contains("EntityId"));
            }
        }

        public class WhenResolving : LiteSyncConflictTests
        {
            [Test]
            public void ShouldSetResolutionWhenKeepingLocal()
            {
                var local = this.CreateChange();
                var remote = this.CreateChange();

                var conflict = new LiteSyncConflict(local, remote);

                conflict.ResolveKeepLocal();

                Assert.AreEqual(LiteSyncConflict.ConflictResolution.KeepLocal, conflict.Resolution);
                Assert.IsNull(conflict.MergedEntity);
            }

            [Test]
            public void ShouldSetResolutionWhenKeepingRemote()
            {
                var local = this.CreateChange();
                var remote = this.CreateChange();

                var conflict = new LiteSyncConflict(local, remote);

                conflict.ResolveKeepRemote();

                Assert.AreEqual(LiteSyncConflict.ConflictResolution.KeepRemote, conflict.Resolution);
                Assert.IsNull(conflict.MergedEntity);
            }

            [Test]
            public void ShouldSetResolutionAndMergedEntityWhenKeepingMerged()
            {
                var local = this.CreateChange(2);
                var remote = this.CreateChange(2);

                var conflict = new LiteSyncConflict(local, remote);

                var mergedDoc = new BsonDocument();
                mergedDoc["_id"] = 2;

                conflict.ResolveMerged(mergedDoc);

                Assert.AreEqual(LiteSyncConflict.ConflictResolution.Merge, conflict.Resolution);
                Assert.AreEqual(mergedDoc, conflict.MergedEntity);
            }

            [Test]
            public void ShouldThrowIfMergedEntityHasDifferentId()
            {
                var localChange = this.CreateChange(2);
                var remoteChange = this.CreateChange(2);

                var conflict = new LiteSyncConflict(localChange, remoteChange);

                var doc = new BsonDocument();
                doc["_id"] = new BsonValue(99);

                Assert.Throws<ArgumentException>(() => conflict.ResolveMerged(doc));
            }
        }

        public class WhenCheckingHasDifferences : LiteSyncConflictTests
        {
            [Test]
            public void ShouldReturnFalseIfBothChangesAreEqual()
            {
                var values = new Dictionary<string, BsonValue>();
                values["Key"] = "Value";

                var localDoc = new BsonDocument(values);
                var remoteDoc = new BsonDocument(values);

                var localChange = this.CreateChange(doc: localDoc);
                var remoteChange = this.CreateChange(doc: remoteDoc);

                var conflict = new LiteSyncConflict(localChange, remoteChange);

                Assert.IsFalse(conflict.HasDifferences());
            }

            [Test]
            public void ShouldReturnFalseIfBothAreDeletes()
            {
                var localChange = new DeleteEntityChange(new EntityId("MyColl", 1));
                var remoteChange = new DeleteEntityChange(new EntityId("MyColl", 1));

                var conflict = new LiteSyncConflict(localChange, remoteChange);

                Assert.IsFalse(conflict.HasDifferences());
            }

            [Test]
            public void ShouldReturnTrueIfChangeTypesDiffer()
            {
                var localChange = new DeleteEntityChange(new EntityId("MyCollection", 1));
                var remoteChange = new UpsertEntityChange(new EntityId("MyCollection", 1), new BsonDocument());

                var conflict = new LiteSyncConflict(localChange, remoteChange);

                Assert.IsTrue(conflict.HasDifferences());
            }

            [Test]
            public void ShouldReturnTrueIfRemoteContainsNewProperty()
            {
                var localValues = new Dictionary<string, BsonValue>();
                var remoteValues = new Dictionary<string, BsonValue>();

                remoteValues["Key"] = "Value";

                var localDoc = new BsonDocument(localValues);
                var remoteDoc = new BsonDocument(remoteValues);

                var localChange = this.CreateChange(doc: localDoc);
                var remoteChange = this.CreateChange(doc: remoteDoc);

                var conflict = new LiteSyncConflict(localChange, remoteChange);

                Assert.IsTrue(conflict.HasDifferences());
            }

            [Test]
            public void ShouldReturnTrueIfRemoteContainsDifferentValue()
            {
                var localValues = new Dictionary<string, BsonValue>();
                var remoteValues = new Dictionary<string, BsonValue>();

                localValues["Key"] = "Local";
                remoteValues["Key"] = "Remote";

                var localDoc = new BsonDocument(localValues);
                var remoteDoc = new BsonDocument(remoteValues);

                var localChange = this.CreateChange(doc: localDoc);
                var remoteChange = this.CreateChange(doc: remoteDoc);

                var conflict = new LiteSyncConflict(localChange, remoteChange);

                Assert.IsTrue(conflict.HasDifferences());
            }
        }

        protected EntityChangeBase CreateChange(int id = 1, BsonDocument doc = null)
        {
            var entityId = new EntityId("MyCollection", new BsonValue(id));
            return new UpsertEntityChange(entityId, doc ?? new BsonDocument());
        }
    }
}