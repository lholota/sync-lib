using System.Linq;
using LiteDB.Sync.Entities;

namespace LiteDB.Sync
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class LiteSyncDatabase : ILiteDatabase
    {
        private const string DeletedEntitiesCollection = "SyncDeleted";

        private readonly LiteDatabase db;

        public LiteSyncDatabase(string connectionString, BsonMapper mapper = null)
        {
            this.db = new LiteDatabase(connectionString, mapper);
        }

        public LiteSyncDatabase(ConnectionString connectionString, BsonMapper mapper = null)
        {
            this.db = new LiteDatabase(connectionString, mapper);
        }

        public LiteSyncDatabase(Stream stream, BsonMapper mapper = null, string password = null)
        {
            this.db = new LiteDatabase(stream, mapper, password);
        }

        public LiteSyncDatabase(IDiskService diskService, 
                            BsonMapper mapper = null, string password = null,
                            TimeSpan? timeout = null, int cacheSize = 5000, Logger log = null)
        {
            this.db = new LiteDatabase(diskService, mapper, password, timeout, cacheSize, log);
        }

        public Logger Log => this.db.Log;

        public BsonMapper Mapper => this.db.Mapper;

        public LiteEngine Engine => this.db.Engine;

        public LiteStorage FileStorage => this.db.FileStorage;

        public void InsertDeletedEntityId(string collectionName, BsonValue id)
        {
            var pointer = new DeletedEntity
            {
                CollectionName = collectionName,
                EntityId = id
            };

            this.db.GetCollection<DeletedEntity>(DeletedEntitiesCollection).Insert(pointer);
        }

        public void InsertDeletedEntityIds(string collectionName, IEnumerable<BsonValue> ids)
        {
            var pointers = ids.Select(x => new DeletedEntity
            {
                CollectionName = collectionName,
                EntityId = x
            });

            this.db.GetCollection<DeletedEntity>(DeletedEntitiesCollection).Insert(pointers);
        }

        public ILiteTransaction BeginTrans()
        {
            return this.db.BeginTrans();
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
    }
}