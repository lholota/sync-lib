namespace LiteDB.Sync.Tests
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Tools;

    [TestFixture]
    public class EndToEndTests
    {
        protected DeviceContext DeviceOne;
        protected DeviceContext DeviceTwo;

        [SetUp]
        public void Setup()
        {
            //this.DeviceOne = new DeviceContext();
            //this.DeviceTwo = new DeviceContext();
        }

        public class WhenEntityCreated : EndToEndTests
        {
            [Test]
            public async Task ShouldAppearInSecondDevice()
            {
                var savedEntity = new TestEntity { IdProp = 123 };
                var dbOne = this.DeviceOne.CreateLiteDatabase();
                dbOne.GetCollection<TestEntity>().Insert(savedEntity);

                await this.DeviceOne.Service.SyncNow();
                await this.DeviceTwo.Service.SyncNow();

                var dbTwo = this.DeviceTwo.CreateLiteDatabase();
                var actual = dbTwo.GetCollection<TestEntity>().FindOne(x => true);

                Assert.IsNotNull(actual);
                Assert.AreEqual(actual.IdProp, savedEntity.IdProp);
            }
        }

        // TODO: Conflicts resolution etc.
    }
}