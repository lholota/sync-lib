using System.Collections.Generic;

namespace LiteDB.Sync
{
    public class LiteSyncDatabase : ILiteDatabase
    {
        private readonly LiteDatabase db;
        private readonly ILiteSyncService syncService;

        public LiteSyncDatabase(ILiteSyncService syncService, LiteDatabase db)
        {
            this.syncService = syncService;
            this.db = db;
        }

        public Logger Log => this.db.Log;

        public BsonMapper Mapper => this.db.Mapper;

        public LiteEngine Engine => this.db.Engine;

        public LiteStorage FileStorage => this.db.FileStorage;

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