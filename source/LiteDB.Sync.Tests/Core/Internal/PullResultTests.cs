using System.Collections.Generic;
using System.Linq;
using LiteDB.Sync.Internal;
using NUnit.Framework;

namespace LiteDB.Sync.Tests.Core.Internal
{
    [TestFixture]
    public class PullResultTests
    {
        public class WhenCreating : PullResultTests
        {
            [Test]
            public void ShouldCombinePatches()
            {
                var first = new Patch(new[] { new EntityChange("MyColl", "ID", EntityChangeType.Upsert, new BsonDocument()) });
                var second = new Patch(new[] { new EntityChange("MyColl", "ID", EntityChangeType.Upsert, new BsonDocument()) });

                var pullResult = new PullResult(new List<Patch> { first, second }, new CloudState());

                Assert.IsNotNull(pullResult.RemoteChanges);
                Assert.AreEqual(1, pullResult.RemoteChanges.Count());
            }
        }

        public class WhenHasChanges
        {
            [Test]
            public void ShouldReturnFalseIsPatchesAreEmpty()
            {
                var patch = new Patch();
                var pullResult = new PullResult(new List<Patch> { patch }, new CloudState());

                Assert.IsFalse(pullResult.HasChanges);
            }

            [Test]
            public void ShouldReturnTrueIfPatchesHaveChanges()
            {
                var patch = new Patch(new[] { new EntityChange("MyColl", "ID", EntityChangeType.Upsert, new BsonDocument()) });
                var pullResult = new PullResult(new List<Patch> { patch }, new CloudState());

                Assert.IsTrue(pullResult.HasChanges);
            }
        }
    }
}