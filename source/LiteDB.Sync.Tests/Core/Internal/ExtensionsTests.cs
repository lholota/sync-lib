using LiteDB.Sync.Internal;
using LiteDB.Sync.Tests.TestUtils;
using LiteDB.Sync.Tests.Tools.Entities;
using Moq;
using NUnit.Framework;

namespace LiteDB.Sync.Tests.Core.Internal
{
    [TestFixture]
    public class ExtensionsTests
    {
        public class WhenCheckingIsSyncEntityType : ExtensionsTests
        {
            [Test]
            public void ShouldReturnTrueIfTypeImplementsInterface()
            {
                Assert.IsTrue(typeof(TestEntity).IsSyncEntityType());
            }

            [Test]
            public void ShouldReturnFalseIfTypeDoesNotImplementInterface()
            {
                Assert.IsFalse(typeof(NonSyncTestEntity).IsSyncEntityType());
            }
        }

        //public class WhenGettingLocalCloudState : ExtensionsTests
        //{
        //    [Test]
        //    public void ShouldQueryCorrectCollectionAndId()
        //    {
        //        var dbMock = new Mock<ILiteDatabase>();
        //        var collMock = new Mock<ILiteCollection<CloudState>>();
        //        var expectedResult = new CloudState();

        //        dbMock.Setup(x => x.GetCollection<CloudState>(LiteSyncDatabase.CloudStateCollectionName)).Returns(collMock.Object);
        //        collMock.Setup(x => x.FindById(LiteSyncDatabase.LocalCloudStateId)).Returns(expectedResult);

        //        var actual = dbMock.Object.GetLocalCloudState();

        //        Assert.AreEqual(expectedResult, actual);

        //        collMock.VerifyAll();
        //        dbMock.VerifyAll();
        //    }
        //}

        //public class WhenSavingLocalCloudState : ExtensionsTests
        //{
        //    [Test]
        //    public void ShouldSaveWithCorrectCollectionAndId()
        //    {
        //        var dbMock = new Mock<ILiteDatabase>();
        //        var collMock = new Mock<ILiteCollection<CloudState>>();
        //        var state = new CloudState();

        //        dbMock.Setup(x => x.GetCollection<CloudState>(LiteSyncDatabase.CloudStateCollectionName)).Returns(collMock.Object);
        //        collMock.Setup(x => x.Upsert(LiteSyncDatabase.LocalCloudStateId, state));

        //        dbMock.Object.SaveLocalCloudState(state);

        //        collMock.VerifyAll();
        //        dbMock.VerifyAll();
        //    }
        // }
    }
}