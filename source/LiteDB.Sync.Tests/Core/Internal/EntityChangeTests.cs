using System;
using LiteDB.Sync.Internal;
using Moq;
using NUnit.Framework;

namespace LiteDB.Sync.Tests.Core.Internal
{
    [TestFixture]
    public class UpsertEntityChangeTests
    {
        public class WhenCreating : UpsertEntityChangeTests
        {
            [Test]
            public void ShouldThrowWithNoEntity()
            {
                var entityId = new EntityId("Dummy", 1);
                Assert.Throws<ArgumentNullException>(() => new UpsertEntityChange(entityId, (BsonDocument)null));
            }

            [Test]
            public void ShouldThrowWithNoEntityId()
            {
                Assert.Throws<ArgumentNullException>(() => new UpsertEntityChange(null, new BsonDocument()));
            }
        }

        public class WhenApplying : UpsertEntityChangeTests
        {
            [Test]
            public void UpsertChangeShouldUpsertEntity()
            {
                var id = new BsonValue(123);
                var doc = new BsonDocument();

                var collectionMock = new Mock<ILiteCollection<BsonDocument>>();
                collectionMock.Setup(x => x.Upsert(id, doc));

                var entityId = new EntityId("Dummy", id);
                var change = new UpsertEntityChange(entityId, doc);
                change.Apply(collectionMock.Object);

                collectionMock.VerifyAll();
            }
        }
    }
}