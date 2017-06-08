using System.Linq;
using LiteDB.Sync.Tests.TestUtils;
using NUnit.Framework;

namespace LiteDB.Sync.Tests.Core.LiteSyncCollection
{
    public partial class LiteSyncCollectionTests
    {
		[TestFixture]
        public class WhenManagingIndices : LiteSyncCollectionTests
        {
            [Test]
            public void ShouldCreateAndDropIndex()
            {
                Assert.IsFalse(this.SyncedCollection.GetIndexes().Any(x => x.Field == nameof(TestEntity.Text)));

                Assert.IsTrue(this.SyncedCollection.EnsureIndex(nameof(TestEntity.Text)));
                Assert.IsTrue(this.SyncedCollection.GetIndexes().Any(x => x.Field == nameof(TestEntity.Text)));

                Assert.IsTrue(this.SyncedCollection.DropIndex(nameof(TestEntity.Text)));
                Assert.IsFalse(this.SyncedCollection.GetIndexes().Any(x => x.Field == nameof(TestEntity.Text)));
            }
        }
    }
}