using System;
using LiteDB.Sync.Internal;
using NUnit.Framework;

namespace LiteDB.Sync.Tests.Unit
{
    [TestFixture]
    public class LiteSyncConflictTests
    {
        public class WhenCreating : LiteSyncConflictTests
        {
            [Test]
            public void ShouldFailIfChangesHaveDifferentEntityId()
            {
                var local = this.CreateChange(1);
                var remote = this.CreateChange(2);

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
                var local = this.CreateChange(1);
                var remote = this.CreateChange(2);

                var conflict = new LiteSyncConflict(local, remote);

                conflict.ResolveKeepLocal();

                Assert.AreEqual(LiteSyncConflict.ConflictResolution.KeepLocal, conflict.Resolution);
                Assert.IsNull(conflict.MergedEntity);
            }

            [Test]
            public void ShouldSetResolutionWhenKeepingRemote()
            {
                var local = this.CreateChange(1);
                var remote = this.CreateChange(2);

                var conflict = new LiteSyncConflict(local, remote);

                conflict.ResolveKeepRemote();

                Assert.AreEqual(LiteSyncConflict.ConflictResolution.KeepRemote, conflict.Resolution);
                Assert.IsNull(conflict.MergedEntity);
            }

            [Test]
            public void ShouldSetResolutionAndMergedEntityWhenKeepingMerged()
            {
                var local = this.CreateChange(1);
                var remote = this.CreateChange(2);

                var conflict = new LiteSyncConflict(local, remote);

                var mergedDoc = new BsonDocument();
                conflict.ResolveMerged(mergedDoc);

                Assert.AreEqual(LiteSyncConflict.ConflictResolution.Merge, conflict.Resolution);
                Assert.AreEqual(mergedDoc, conflict.MergedEntity);
            }
        }

        protected EntityChange CreateChange(int id = 1)
        {
            return new EntityChange(string.Empty, new BsonValue(id), EntityChangeType.Upsert, new BsonDocument());
        }
    }
}