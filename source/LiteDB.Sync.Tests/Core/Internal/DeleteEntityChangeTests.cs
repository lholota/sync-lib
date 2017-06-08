using System;
using LiteDB.Sync.Internal;
using Moq;
using NUnit.Framework;

namespace LiteDB.Sync.Tests.Core.Internal
{
    [TestFixture]
    public class DeleteEntityChangeTests
    {
        public class WhenCreating : UpsertEntityChangeTests
        {
            [Test]
            public void ShouldThrowWithNoEntityId()
            {
                Assert.Throws<ArgumentNullException>(() => new DeleteEntityChange(null));
            }
        }

        public class WhenApplying : DeleteEntityChangeTests
        {
            [Test]
            public void DeleteChangeShouldDeleteEntity()
            {
                var id = new BsonValue(123);

                var collectionMock = new Mock<ILiteCollection<BsonDocument>>();
                collectionMock.Setup(x => x.Delete(id));

                var entityId = new EntityId("Dummy", id);
                var change = new DeleteEntityChange(entityId);
                change.Apply(collectionMock.Object);

                collectionMock.VerifyAll();
            }
        }
    }
}