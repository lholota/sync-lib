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
    public class LiteSyncDatabase : ILiteDatabase, ILiteSynchronizable, ILiteSyncDatabase
    {
        private const string ProtectedCollectionNameExceptionMessage = "The collection name {0} is used by the LiteDB.Sync and cannot be accessed directly.";

        internal const string LocalCloudStateId = "LocalHead";
        internal const string CloudStateCollectionName = "LiteSync_State";
        internal const string DeletedEntitiesCollectionName = "LiteSync_Deleted";

        private Task syncInProgressTask;

        private readonly IFactory factory;
        private readonly ICloudClient cloudClient;
        private readonly ILiteSyncConfiguration syncConfig;
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
            if (syncConfig == null)
            {
                throw new ArgumentNullException(nameof(syncConfig));
            }

            if (!syncConfig.IsValid)
            {
                throw new ArgumentException($"The provided {nameof(ILiteSyncConfiguration)} is invalid.");
            }

            this.cloudClient = factory.CreateCloudClient(syncConfig.CloudProvider);
            this.syncConfig = syncConfig;
            this.InnerDb = db;

            this.factory = factory;

            this.Mapper.RegisterCustomMappings();
        }

        public event EventHandler SyncStarted;

        public event EventHandler<SyncFinishedEventArgs> SyncFinished;

        public bool IsSyncInProgress
        {
            get
            {
                lock (this.syncControlLock)
                {
                    return this.syncInProgressTask != null
                        && !this.syncInProgressTask.IsCompleted;
                }
            }
        }

        /// <summary>
        /// Get logger class instance
        /// </summary>
        public Logger Log => this.InnerDb.Log;

        /// <summary>
        /// Get current instance of BsonMapper used in this database instance (can be BsonMapper.Global)
        /// </summary>
        public BsonMapper Mapper => this.InnerDb.Mapper;

        public LiteEngine Engine => this.InnerDb.Engine;

        /// <summary>
        /// Returns a special collection for storage files/stream inside datafile
        /// </summary>
        public LiteStorage FileStorage => this.InnerDb.FileStorage;

        internal LiteDatabase InnerDb { get; }

        /// <summary>
        /// Names of the collections which are synchronized
        /// </summary>
        public IEnumerable<string> SyncedCollectionNames => this.syncConfig.SyncedCollections;

        /// <summary>
        /// Starts synchronization to the cloud. If the synchronization is already running
        /// it will return an empty task which immediately finishes. 
        /// In case you need to cancel the synchronization, please note that you need to wait for the returned task 
        /// and that it may take up to several seconds (not all parts of the sync can be just stopped halfway through))
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns><see cref="Task"/></returns>
        public Task SynchronizeAsync(CancellationToken ct = default(CancellationToken))
        {
            lock (this.syncControlLock)
            {
                this.EnsureSyncIndices();

                if (this.syncInProgressTask != null)
                {
                    return Task.FromResult(0);
                }

                var synchronizer = this.factory.CreateSynchronizer(this, this.syncConfig, this.cloudClient);

                this.OnSyncStarting();

                this.syncInProgressTask = Task.Run(async () => await this.ExecuteSynchronizationAsync(synchronizer, ct), ct);

                return this.syncInProgressTask;
            }
        }

        public ILiteTransaction BeginTrans()
        {
            return this.InnerDb.BeginTrans();
        }

        public ILiteCollection<T> GetCollection<T>(string name)
        {
            if (this.IsCollectionNameProtected(name))
            {
                throw new ArgumentException(string.Format(ProtectedCollectionNameExceptionMessage, name));
            }

            var nativeCollection = this.InnerDb.GetCollection<T>(name);

            return this.WrapCollectionIfRequired(name, nativeCollection);
        }

        public ILiteCollection<T> GetCollection<T>()
        {
            var name = BsonMapper.Global.ResolveCollectionName.Invoke(typeof(T));

            if (this.IsCollectionNameProtected(name))
            {
                throw new ArgumentException(string.Format(ProtectedCollectionNameExceptionMessage, name));
            }

            var nativeCollection = this.InnerDb.GetCollection<T>();

            return this.WrapCollectionIfRequired(name, nativeCollection);
        }

        public ILiteCollection<BsonDocument> GetCollection(string name)
        {
            if (this.IsCollectionNameProtected(name))
            {
                throw new ArgumentException(string.Format(ProtectedCollectionNameExceptionMessage, name));
            }

            var nativeCollection = this.InnerDb.GetCollection(name);

            return this.WrapCollectionIfRequired(name, nativeCollection, false);
        }

        public IEnumerable<string> GetCollectionNames()
        {
            return this.InnerDb.GetCollectionNames().Where(x => !this.IsCollectionNameProtected(x));
        }

        public bool CollectionExists(string name)
        {
            if (this.IsCollectionNameProtected(name))
            {
                return false;
            }

            return this.InnerDb.CollectionExists(name);
        }

        public bool DropCollection(string name)
        {
            if (this.IsCollectionNameProtected(name))
            {
                throw new ArgumentException(string.Format(ProtectedCollectionNameExceptionMessage, name));
            }

            return this.InnerDb.DropCollection(name);
        }

        public bool RenameCollection(string oldName, string newName)
        {
            if (this.IsCollectionNameProtected(oldName))
            {
                throw new ArgumentException(string.Format(ProtectedCollectionNameExceptionMessage, oldName));
            }

            if (this.IsCollectionNameProtected(newName))
            {
                throw new ArgumentException(string.Format(ProtectedCollectionNameExceptionMessage, newName));
            }

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

        private bool IsCollectionNameProtected(string name)
        {
            return string.Equals(name, DeletedEntitiesCollectionName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, CloudStateCollectionName, StringComparison.OrdinalIgnoreCase);
        }

        private async Task ExecuteSynchronizationAsync(ISynchronizer synchronizer, CancellationToken ct)
        {
            this.OnSyncStarting();

            try
            {
                await synchronizer.SynchronizeAsync(ct);

                this.OnSyncFinished();   
            }
            catch (Exception ex)
            {
                this.OnSyncFinished(ex);
            }
        }

        private void OnSyncStarting()
        {
            this.SyncStarted?.Invoke(this, EventArgs.Empty);
        }

        private void OnSyncFinished(Exception ex = null)
        {
            var args = new SyncFinishedEventArgs
            {
                Error = ex
            };

            this.SyncFinished?.Invoke(this, args);
        }

        CloudState ILiteSyncDatabase.GetLocalCloudState()
        {
            return this.InnerDb.GetCollection<CloudState>(CloudStateCollectionName).FindById(LocalCloudStateId);
        }

        void ILiteSyncDatabase.SaveLocalCloudState(CloudState cloudState)
        {
            this.InnerDb.GetCollection<CloudState>(CloudStateCollectionName).Upsert(LocalCloudStateId, cloudState);
        }

        IDisposable ILiteSyncDatabase.LockExclusive()
        {
            return this.Engine.Locker.Exclusive();
        }

        Patch ILiteSyncDatabase.GetLocalChanges(CancellationToken ct)
        {
            var patch = new Patch();

            foreach (var collectionName in this.syncConfig.SyncedCollections)
            {
                var collection = this.InnerDb.GetCollection(collectionName);
                var dirtyEntities = this.FindDirtyEntities(collection);

                patch.AddUpsertChanges(collectionName, dirtyEntities);

                var delCollection = this.GetDeletedEntitiesCollection();
                var deletedEntities = delCollection.FindAll();

                patch.AddDeleteChanges(deletedEntities);

                ct.ThrowIfCancellationRequested();
            }

            return patch;
        }

        void ILiteSyncDatabase.ApplyChanges(Patch patch, CancellationToken ct)
        {
            var groupped = patch.Changes.GroupBy(x => x.EntityId.CollectionName, x => x);

            foreach (var group in groupped)
            {
                var collection = this.InnerDb.GetCollection(group.Key);

                foreach (var change in group)
                {
                    change.Apply(collection);

                    ct.ThrowIfCancellationRequested();
                }
            }
        }

        private IEnumerable<BsonDocument> FindDirtyEntities(ILiteCollection<BsonDocument> collection)
        {
            var fieldName = this.Mapper.ResolveFieldName.Invoke(nameof(ILiteSyncEntity.RequiresSync));
            var query = Query.EQ(nameof(ILiteSyncEntity.RequiresSync), new BsonValue(true));

            return collection.Find(query);
        }
    }
}