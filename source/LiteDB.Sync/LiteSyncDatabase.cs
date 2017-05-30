using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB.Sync.Internal;

namespace LiteDB.Sync
{
    public class LiteSyncDatabase : ILiteDatabase
    {
        internal const string LocalHeadId = "LocalHead";
        internal const string SyncStateCollectionName = "LiteSync_State";
        internal const string DeletedEntitiesCollectionName = "LiteSync_Deleted";

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
            var nativeCollection = this.db.GetCollection<T>(name);

            return this.WrapCollectionIfRequired<T>(name, nativeCollection);
        }

        public ILiteCollection<T> GetCollection<T>()
        {
            var nativeCollection = this.db.GetCollection<T>();
            var name = BsonMapper.Global.ResolveCollectionName.Invoke(typeof(T));

            return this.WrapCollectionIfRequired(name, nativeCollection);
        }

        public ILiteCollection<BsonDocument> GetCollection(string name)
        {
            var nativeCollection = this.db.GetCollection(name);

            return this.WrapCollectionIfRequired(name, nativeCollection, false);
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

        internal ILiteCollection<DeletedEntity> GetDeletedEntitiesCollection()
        {
            return this.db.GetCollection<DeletedEntity>(DeletedEntitiesCollectionName);
        }

        private ILiteCollection<T> WrapCollectionIfRequired<T>(string name, ILiteCollection<T> nativeCollection, bool validateType = true)
        {
            if (this.syncService.SyncedCollections.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                if (validateType && !typeof(T).IsSyncEntityType())
                {
                    throw LiteSyncException.EntityDoesntImplementInterface(typeof(T));
                }

                return new LiteSyncCollection<T>(nativeCollection, this);
            }

            return nativeCollection;
        }
    }
}