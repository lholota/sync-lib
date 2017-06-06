using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace LiteDB.Sync
{
    public class LiteSyncRepository : ILiteRepository, ILiteSynchronizable
    {
        private readonly LiteSyncDatabase db;
        private readonly LiteRepository repository;

        /// <summary>
        /// Starts LiteDB database using a connection string for file system database
        /// </summary>
        public LiteSyncRepository(LiteSyncConfiguration syncConfig, string connectionString, BsonMapper mapper = null)
            : this(new LiteSyncDatabase(syncConfig, connectionString, mapper))
        {           
        }

        /// <summary>
        /// Starts LiteDB database using a connection string for file system database
        /// </summary>
        public LiteSyncRepository(LiteSyncConfiguration syncConfig, ConnectionString connectionString, BsonMapper mapper = null)
            : this(new LiteSyncDatabase(syncConfig, connectionString, mapper))
        {
        }

        /// <summary>
        /// Starts LiteDB database using a Stream disk
        /// </summary>
        public LiteSyncRepository(LiteSyncConfiguration syncConfig, Stream stream, BsonMapper mapper = null, string password = null)
            : this(new LiteSyncDatabase(syncConfig, stream, mapper, password))
        {
        }

        private LiteSyncRepository(LiteSyncDatabase db)
        {
            this.repository = new LiteRepository(db);
            this.db = db;
        }

        public ILiteDatabase Database => this.repository.Database;

        public LiteEngine Engine => this.repository.Engine;

        public LiteStorage FileStorage => this.repository.FileStorage;

        public IEnumerable<string> SyncedCollectionNames => this.db.SyncedCollectionNames;

        public ILiteTransaction BeginTrans()
        {
            return this.repository.BeginTrans();
        }

        public BsonValue Insert<T>(T entity, string collectionName = null)
        {
            return this.repository.Insert(entity, collectionName);
        }

        public int Insert<T>(IEnumerable<T> entities, string collectionName = null)
        {
            return this.repository.Insert(entities, collectionName);
        }

        public bool Update<T>(T entity, string collectionName = null)
        {
            return this.repository.Update(entity, collectionName);
        }

        public int Update<T>(IEnumerable<T> entities, string collectionName = null)
        {
            return this.repository.Update(entities, collectionName);
        }

        public bool Upsert<T>(T entity, string collectionName = null)
        {
            return this.repository.Update(entity, collectionName);
        }

        public int Upsert<T>(IEnumerable<T> entities, string collectionName = null)
        {
            return this.repository.Upsert(entities, collectionName);
        }

        public bool Delete<T>(BsonValue id, string collectionName = null)
        {
            return this.repository.Delete<T>(id, collectionName);
        }

        public int Delete<T>(Query query, string collectionName = null)
        {
            return this.repository.Delete<T>(query, collectionName);
        }

        public int Delete<T>(Expression<Func<T, bool>> predicate, string collectionName = null)
        {
            return this.repository.Delete(predicate, collectionName);
        }

        public LiteQueryable<T> Query<T>(string collectionName = null)
        {
            return this.repository.Query<T>(collectionName);
        }

        public T SingleById<T>(BsonValue id, string collectionName = null)
        {
            return this.repository.SingleById<T>(id, collectionName);
        }

        public List<T> Fetch<T>(Query query = null, string collectionName = null)
        {
            return this.repository.Fetch<T>(query, collectionName);
        }

        public List<T> Fetch<T>(Expression<Func<T, bool>> predicate, string collectionName = null)
        {
            return this.repository.Fetch(predicate, collectionName);
        }

        public T First<T>(Query query = null, string collectionName = null)
        {
            return this.repository.First<T>(query, collectionName);
        }

        public T First<T>(Expression<Func<T, bool>> predicate, string collectionName = null)
        {
            return this.repository.First(predicate, collectionName);
        }

        public T FirstOrDefault<T>(Query query = null, string collectionName = null)
        {
            return this.repository.FirstOrDefault<T>(query, collectionName);
        }

        public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate, string collectionName = null)
        {
            return this.repository.FirstOrDefault(predicate, collectionName);
        }

        public T Single<T>(Query query = null, string collectionName = null)
        {
            return this.repository.Single<T>(query, collectionName);
        }

        public T Single<T>(Expression<Func<T, bool>> predicate, string collectionName = null)
        {
            return this.repository.Single(predicate, collectionName);
        }

        public T SingleOrDefault<T>(Query query = null, string collectionName = null)
        {
            return this.repository.SingleOrDefault<T>(query, collectionName);
        }

        public T SingleOrDefault<T>(Expression<Func<T, bool>> predicate, string collectionName = null)
        {
            return this.repository.SingleOrDefault(predicate, collectionName);
        }

        public Task SynchronizeAsync(CancellationToken ct)
        {
            return this.db.SynchronizeAsync(ct);
        }

        public void Dispose()
        {
            this.repository.Dispose();
        }
    }
}