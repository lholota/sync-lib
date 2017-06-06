using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiteDB.Sync.Exceptions;
using LiteDB.Sync.Internal;

namespace LiteDB.Sync
{
    public class LiteSyncDatabase : ILiteDatabase, ILiteSynchronizable
    {
        internal const string LocalCloudStateId = "LocalHead";
        internal const string SyncStateCollectionName = "LiteSync_State";
        internal const string DeletedEntitiesCollectionName = "LiteSync_Deleted";

        private Task syncInProgressTask;

        private readonly LiteDatabase db;
        private readonly ILiteSyncConfiguration syncConfig;
        private readonly ICloudClient cloudClient;
        private readonly object syncControlLock = new object();

        /// <summary>
        /// Starts LiteDB database using a connection string for file system database
        /// </summary>
        public LiteSyncDatabase(ILiteSyncConfiguration syncConfig, string connectionString, BsonMapper mapper = null)
            : this(syncConfig, new LiteDatabase(connectionString, mapper), new Factory())
        {
        }

        /// <summary>
        /// Starts LiteDB database using a connection string for file system database
        /// </summary>
        public LiteSyncDatabase(ILiteSyncConfiguration syncConfig, ConnectionString connectionString, BsonMapper mapper = null)
            : this(syncConfig, new LiteDatabase(connectionString, mapper), new Factory())
        {
        }

        /// <summary>
        /// Starts LiteDB database using a Stream disk
        /// </summary>
        public LiteSyncDatabase(ILiteSyncConfiguration syncConfig, Stream stream, BsonMapper mapper = null, string password = null)
            : this(syncConfig, new LiteDatabase(stream, mapper, password), new Factory())
        {
        }

        /// <summary>
        /// Starts LiteDB database using a custom IDiskService with all parameters available
        /// </summary>
        /// <param name="syncConfig">Synchronization configuration</param>
        /// <param name="diskService">Custom implementation of persist data layer</param>
        /// <param name="mapper">Instance of BsonMapper that map poco classes to document</param>
        /// <param name="password">Password to encrypt you datafile</param>
        /// <param name="timeout">Locker timeout for concurrent access</param>
        /// <param name="cacheSize">Max memory pages used before flush data in Journal file (when available)</param>
        /// <param name="log">Custom log implementation</param>
        public LiteSyncDatabase(ILiteSyncConfiguration syncConfig, IDiskService diskService, BsonMapper mapper = null, string password = null, TimeSpan? timeout = null, int cacheSize = 5000, Logger log = null)
            : this(syncConfig, new LiteDatabase(diskService, mapper, password, timeout, cacheSize, log), new Factory())
        {
        }

        internal LiteSyncDatabase(ILiteSyncConfiguration syncConfig, Stream stream, IFactory factory)
            : this(syncConfig, new LiteDatabase(stream), factory)
        {
        }

        private LiteSyncDatabase(ILiteSyncConfiguration syncConfig, LiteDatabase db, IFactory factory)
        {
            this.cloudClient = factory.CreateCloudClient(syncConfig.CloudProvider);
            this.syncConfig = syncConfig;
            this.db = db;
        }

        public Logger Log => this.db.Log;

        public BsonMapper Mapper => this.db.Mapper;

        public LiteEngine Engine => this.db.Engine;

        public LiteStorage FileStorage => this.db.FileStorage;

        public IEnumerable<string> SyncedCollectionNames => this.syncConfig.SyncedCollections;

        public Task SynchronizeAsync(CancellationToken ct)
        {
            lock (this.syncControlLock)
            {
                this.EnsureSyncIndices();

                if (this.syncInProgressTask != null)
                {
                    return Task.FromResult(0);
                }

                var ctx = new LiteSynchronizer(this.db, this.syncConfig, this.cloudClient);

                this.syncInProgressTask = ctx.Synchronize(ct);

                return this.syncInProgressTask;
            }
        }

        public ILiteTransaction BeginTrans()
        {
            return this.db.BeginTrans();
        }

        public ILiteCollection<T> GetCollection<T>(string name)
        {
            var nativeCollection = this.db.GetCollection<T>(name);

            return this.WrapCollectionIfRequired(name, nativeCollection);
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

            // TBA: Dispose worker if it's created
        }

        internal ILiteCollection<DeletedEntity> GetDeletedEntitiesCollection()
        {
            return this.db.GetCollection<DeletedEntity>(DeletedEntitiesCollectionName);
        }

        private ILiteCollection<T> WrapCollectionIfRequired<T>(string name, ILiteCollection<T> nativeCollection, bool validateType = true)
        {
            if (this.SyncedCollectionNames.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                if (validateType && !typeof(T).IsSyncEntityType())
                {
                    throw new LiteSyncInvalidEntityException(typeof(T));
                }

                return new LiteSyncCollection<T>(nativeCollection, this);
            }

            return nativeCollection;
        }

        private void EnsureSyncIndices()
        {
            using (var tx = this.db.BeginTrans())
            {
                foreach (var collectionName in this.SyncedCollectionNames)
                {
                    var collection = this.db.GetCollection(collectionName);
                    collection.EnsureIndex(nameof(ILiteSyncEntity.RequiresSync));
                }

                tx.Commit();
            }
        }
    }
}