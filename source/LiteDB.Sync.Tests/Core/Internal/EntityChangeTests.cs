using System;
using LiteDB.Sync.Internal;
using Moq;
using NUnit.Framework;

namespace LiteDB.Sync.Tests.Core.Internal
{
    [TestFixture]
    public class EntityChangeTests
    {
        public class WhenCreating : EntityChangeTests
        {
            [Test]
            public void ShouldThrowOnUpsertWithNoEntity()
            {
                var entityId = new EntityId("Dummy", 1);
                Assert.Throws<ArgumentNullException>(() => new EntityChange(entityId, EntityChangeType.Upsert, (BsonDocument)null));
            }
        }

        public class WhenApplying : EntityChangeTests
        {
            [Test]
            public void DeleteChangeShouldDeleteEntity()
            {
                var id = new BsonValue(123);

                var collectionMock = new Mock<ILiteCollection<BsonDocument>>();
                collectionMock.Setup(x => x.Delete(id));

                var entityId = new EntityId("Dummy", id);
                var change = new EntityChange(entityId);
                change.Apply(collectionMock.Object);

                collectionMock.VerifyAll();
            }

            [Test]
            public void UpsertChangeShouldUpsertEntity()
            {
                var id = new BsonValue(123);
                var doc = new BsonDocument();

                var collectionMock = new Mock<ILiteCollection<BsonDocument>>();
                collectionMock.Setup(x => x.Upsert(id, doc));

                var entityId = new EntityId("Dummy", id);
                var change = new EntityChange(entityId, EntityChangeType.Upsert, doc);
                change.Apply(collectionMock.Object);

                collectionMock.VerifyAll();
            }
        }
    }
}