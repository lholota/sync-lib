namespace LiteDB.Sync
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class LiteSyncDatabase : ILiteDatabase
    {
        private long transactionCount;

        private readonly object syncTransactionLock = new object();
        private readonly LiteDatabase db;
        private readonly ILiteDbSyncController syncController;
        private LiteSyncTransaction syncTransaction;

        public LiteSyncDatabase(ILiteDbSyncController syncController, string connectionString, BsonMapper mapper = null)
        {
            this.db = new LiteDatabase(connectionString, mapper);
            this.syncController = syncController;
        }

        public LiteSyncDatabase(ILiteDbSyncController syncController, ConnectionString connectionString, BsonMapper mapper = null)
        {
            this.db = new LiteDatabase(connectionString, mapper);
            this.syncController = syncController;
        }

        public LiteSyncDatabase(ILiteDbSyncController syncController, Stream stream, BsonMapper mapper = null, string password = null)
        {
            this.db = new LiteDatabase(stream, mapper, password);
            this.syncController = syncController;
        }

        public LiteSyncDatabase(ILiteDbSyncController syncController, IDiskService diskService, 
                            BsonMapper mapper = null, string password = null,
                            TimeSpan? timeout = null, int cacheSize = 5000, Logger log = null)
        {
            this.db = new LiteDatabase(diskService, mapper, password, timeout, cacheSize, log);
            this.syncController = syncController;
        }

        public Logger Log => this.db.Log;

        public BsonMapper Mapper => this.db.Mapper;

        public LiteEngine Engine => this.db.Engine;

        public LiteStorage FileStorage => this.db.FileStorage;

        internal LiteSyncTransaction SyncTransaction
        {
            get
            {
                lock (this.syncTransactionLock)
                {
                    return syncTransaction;
                }
            }
        }

        public ILiteTransaction BeginTrans()
        {
            lock (this.syncTransactionLock)
            {
                if (this.transactionCount == 0)
                {
                    this.syncTransaction = new LiteSyncTransaction(this.db.BeginTrans(), this);

                    return this.syncTransaction;
                }

                this.transactionCount++;

                return this.db.BeginTrans();
            }
        }

        public ILiteCollection<T> GetCollection<T>(string name)
        {
            return new LiteSyncCollection<T>(this.db.GetCollection<T>(name), this);
        }

        public ILiteCollection<T> GetCollection<T>()
        {
            return new LiteSyncCollection<T>(this.db.GetCollection<T>(), this);
        }

        public ILiteCollection<BsonDocument> GetCollection(string name)
        {
            return new LiteSyncCollection<BsonDocument>(this.db.GetCollection(name), this);
        }

        public IEnumerable<string> GetCollectionNames()
        {
            return this.db.GetCollectionNames();
        }

        public bool CollectionExists(string name)
        {
            return this.db.CollectionExists(name);
        }

        public bool DropCollection(string name)
        {
            return this.db.DropCollection(name);
        }

        public bool RenameCollection(string oldName, string newName)
        {
            return this.db.RenameCollection(oldName, newName);
        }

        public long Shrink()
        {
            return this.db.Shrink();
        }

        public long Shrink(string password)
        {
            return this.db.Shrink(password);
        }

        public void Dispose()
        {
            this.db.Dispose();
        }

        internal void PopTransaction()
        {
            lock (syncTransactionLock)
            {
                this.transactionCount--;

                if (this.transactionCount == 0)
                {
                    // TODO: clear the field
                }
            }
        }
    }
}