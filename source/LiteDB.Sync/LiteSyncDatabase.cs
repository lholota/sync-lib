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
        private const string ProtectedCollectionNameExceptionMessage = "The collection name {0} is used by the LiteDB.Sync and cannot be accessed directly.";

        internal const string LocalCloudStateId = "LocalHead";
        internal const string SyncStateCollectionName = "LiteSync_State";
        internal const string DeletedEntitiesCollectionName = "LiteSync_Deleted";

        private Task syncInProgressTask;
        
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
            this.InnerDb = db;
        }

        public Logger Log => this.InnerDb.Log;

        public BsonMapper Mapper => this.InnerDb.Mapper;

        public LiteEngine Engine => this.InnerDb.Engine;

        public LiteStorage FileStorage => this.InnerDb.FileStorage;

        internal LiteDatabase InnerDb { get; }

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

                var ctx = new LiteSynchronizer(this.InnerDb, this.syncConfig, this.cloudClient);

                this.syncInProgressTask = ctx.Synchronize(ct);

                return this.syncInProgressTask;
            }
        }

        public ILiteTransaction BeginTrans()
        {
            return this.InnerDb.BeginTrans();
        }

        public ILiteCollection<T> GetCollection<T>(string name)
        {
            if (this.IsDeleteEntitiesCollectionName(name))
            {
                throw new ArgumentException(string.Format(ProtectedCollectionNameExceptionMessage, name));
            }

            var nativeCollection = this.InnerDb.GetCollection<T>(name);

            return this.WrapCollectionIfRequired(name, nativeCollection);
        }

        public ILiteCollection<T> GetCollection<T>()
        {
            var name = BsonMapper.Global.ResolveCollectionName.Invoke(typeof(T));

            if (this.IsDeleteEntitiesCollectionName(name))
            {
                throw new ArgumentException(string.Format(ProtectedCollectionNameExceptionMessage, name));
            }

            var nativeCollection = this.InnerDb.GetCollection<T>();

            return this.WrapCollectionIfRequired(name, nativeCollection);
        }

        public ILiteCollection<BsonDocument> GetCollection(string name)
        {
            if (this.IsDeleteEntitiesCollectionName(name))
            {
                throw new ArgumentException(string.Format(ProtectedCollectionNameExceptionMessage, name));
            }

            var nativeCollection = this.InnerDb.GetCollection(name);

            return this.WrapCollectionIfRequired(name, nativeCollection, false);
        }

        public IEnumerable<string> GetCollectionNames()
        {
            return this.InnerDb.GetCollectionNames().Where(x => !this.IsDeleteEntitiesCollectionName(x));
        }

        public bool CollectionExists(string name)
        {
            return this.InnerDb.CollectionExists(name);
        }

        public bool DropCollection(string name)
        {
            return this.InnerDb.DropCollection(name);
        }

        public bool RenameCollection(string oldName, string newName)
        {
            return this.InnerDb.RenameCollection(oldName, newName);
        }

        public long Shrink()
        {
            return this.InnerDb.Shrink();
        }

        public long Shrink(string password)
        {
            return this.InnerDb.Shrink(password);
        }

        public void Dispose()
        {
            this.InnerDb.Dispose();

            // TBA: Dispose worker if it's created
        }

        internal ILiteCollection<DeletedEntity> GetDeletedEntitiesCollection()
        {
            return this.InnerDb.GetCollection<DeletedEntity>(DeletedEntitiesCollectionName);
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
            using (var tx = this.InnerDb.BeginTrans())
            {
                foreach (var collectionName in this.SyncedCollectionNames)
                {
                    var collection = this.InnerDb.GetCollection(collectionName);
                    collection.EnsureIndex(nameof(ILiteSyncEntity.RequiresSync));
                }

                tx.Commit();
            }
        }

        private bool IsDeleteEntitiesCollectionName(string name)
        {
            return string.Equals(name, DeletedEntitiesCollectionName, StringComparison.OrdinalIgnoreCase);
        }
    }
}