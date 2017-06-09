using LiteDB.Sync.Internal;
using NUnit.Framework;

namespace LiteDB.Sync.Tests.Core.Internal
{
    [TestFixture]
    public class BsonMappingsTests
    {
        protected BsonMapper Mapper;

        [SetUp]
        public void Setup()
        {
            this.Mapper = new BsonMapper();
            this.Mapper.RegisterCustomMappings();
        }

        public class WhenMappingEntityId : BsonMappingsTests
        {
            /*
             * TBA: Complex id, string id etc.
             * 
             */

            [Test]
            public void ShouldConvertToBsonAndBack()
            {
                var expected = new EntityId("Collection", 25);

                var bson = this.Mapper.ToDocument(expected);
                var actual = this.Mapper.ToObject<EntityId>(bson);

                Assert.AreEqual(expected, actual);
            }
        }

        public class WhenMappingDeletedEntity : BsonMappingsTests
        {
            [Test]
            public void ShouldConvertToBsonAndBack()
            {
                var expected = new DeletedEntity("Collection", 25);

                var bson = this.Mapper.ToDocument(expected);
                var actual = this.Mapper.ToObject<DeletedEntity>(bson);

                Assert.AreEqual(expected.EntityId, actual.EntityId);
            }
        }
    }
}