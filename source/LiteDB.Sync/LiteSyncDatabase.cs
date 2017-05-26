namespace LiteDB.Sync
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class LiteSyncDatabase : ILiteDatabase
    {
        private readonly LiteDatabase db;
        private readonly ILiteDbSyncController syncController;

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

        public ILiteTransaction BeginTrans() // TODO: Interface?
        {
            return this.db.BeginTrans();
        }

        public ILiteCollection<T> GetCollection<T>(string name) // TODO: Interface?
        {
            throw new NotImplementedException();
        }

        public ILiteCollection<T> GetCollection<T>()
        {
            throw new NotImplementedException();
        }

        public ILiteCollection<BsonDocument> GetCollection(string name)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetCollectionNames()
        {
            throw new NotImplementedException();
        }

        public bool CollectionExists(string name)
        {
            throw new NotImplementedException();
        }

        public bool DropCollection(string name)
        {
            throw new NotImplementedException();
        }

        public bool RenameCollection(string oldName, string newName)
        {
            throw new NotImplementedException();
        }

        public long Shrink()
        {
            throw new NotImplementedException();
        }

        public long Shrink(string password)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            this.db.Dispose();
        }
    }
}