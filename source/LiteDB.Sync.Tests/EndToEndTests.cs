namespace LiteDB.Sync.Tests
{
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
            this.DeviceOne = new DeviceContext();
            this.DeviceTwo = new DeviceContext();
        }

        public class WhenEntityCreated : EndToEndTests
        {
            [Test]
            public async Task ShouldAppearInSecondDevice()
            {
                var savedEntity = new TestEntity { Text = "Hello" };
                var dbOne = this.DeviceOne.CreateLiteDatabase();
                dbOne.GetCollection<TestEntity>().Insert(savedEntity);

                await this.DeviceOne.Controller.SyncNow();
                await this.DeviceTwo.Controller.SyncNow();

                var dbTwo = this.DeviceTwo.CreateLiteDatabase();
                var actual = dbTwo.GetCollection<TestEntity>().FindOne(x => true);

                Assert.IsNotNull(actual);
                Assert.AreEqual(actual.Text, savedEntity.Text);
            }
        }

        // TODO: Conflicts resolution etc.
    }
}