using System.IO;
using LiteDB.Sync.Tests.Tools;
using NUnit.Framework;
using Moq;

namespace LiteDB.Sync.Tests
{
    [TestFixture]
    public partial class LiteSyncCollectionTests
    {
        protected MemoryStream DbStream;
        protected LiteDatabase InnerDb;
        protected LiteSyncDatabase Db;
        protected ILiteCollection<TestEntity> SyncedCollection;
        protected ILiteCollection<TestEntity> NativeCollection;
        protected Mock<ILiteSyncService> SyncServiceMock;

        [SetUp]
        public void Setup()
        {
            this.DbStream = new MemoryStream();
            this.InnerDb = new LiteDatabase(this.DbStream);

            this.SyncServiceMock = new Mock<ILiteSyncService>();
            this.SyncServiceMock.Setup(x => x.SyncedCollections).Returns(new[] {"Dummy"});

            this.Db = new LiteSyncDatabase(this.SyncServiceMock.Object, this.InnerDb);
            this.SyncedCollection = this.Db.GetCollection<TestEntity>("Dummy");
            this.NativeCollection = this.InnerDb.GetCollection<TestEntity>("Dummy");

            Assert.IsInstanceOf<LiteSyncCollection<TestEntity>>(this.SyncedCollection);
        }

        [TearDown]
        public void TearDown()
        {
            this.Db.Dispose();
            this.DbStream.Dispose();
        }

        // ALL LOGIC SHOULD BE IGNORED WHEN WORKING WITH NON-SYNCED COLLECTIONS! (???)

       

        public class WhenInsertingIntoNonSyncedCollection : LiteSyncCollectionTests
        {
            
        }

        public class WhenInsertingIntoSyncedCollection : LiteSyncCollectionTests
        {
            /*
             * Switch to upsert in synced collections (the deleted item still may be in the collection)
             * Double insert should fail with id problem
             */
        }

        public class WhenFindingInNonSyncedCollection : LiteSyncCollectionTests
        {
            /*
             * Logically deleted items should not be returned !!!
             */
        }

        public class WhenFindingInSyncedCollection : LiteSyncCollectionTests
        {
            /*
             * Logically deleted items should not be returned !!!
             */
        }

        public class WhenAggregatingInNonSyncedCollection : LiteSyncCollectionTests
        {
            
        }

        public class WhenAggregatingInSyncedCollection : LiteSyncCollectionTests
        {

        }
    }
}