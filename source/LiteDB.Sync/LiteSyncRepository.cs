namespace LiteDB.Sync
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq.Expressions;

    public class LiteSyncRepository : ILiteRepository
    {
        private readonly ILiteDatabase db;

        public LiteSyncRepository(string connectionString, BsonMapper mapper = null)
        {
            this.db = new LiteSyncDatabase(connectionString, mapper);
        }

        public LiteSyncRepository(ConnectionString connectionString, BsonMapper mapper = null)
        {
            this.db = new LiteSyncDatabase(connectionString, mapper);
        }

        public LiteSyncRepository(Stream stream, BsonMapper mapper = null, string password = null)
        {
            this.db = new LiteSyncDatabase(stream, mapper, password);
        }

        #region Shortchut from Database/Engine

        /// <summary>
        /// Starts new transaction. Don't forgot execute ".Commit()" method from LiteTransaction
        /// </summary>
        public ILiteTransaction BeginTrans()
        {
            return this.db.BeginTrans();
        }

        /// <summary>
        /// Get database instance
        /// </summary>
        public ILiteDatabase Database => this.db;

        /// <summary>
        /// Get engine instance
        /// </summary>
        public LiteEngine Engine => this.db.Engine;

        /// <summary>
        /// Returns a special collection for storage files/stream inside datafile
        /// </summary>
        public LiteStorage FileStorage => this.db.FileStorage;

        #endregion

        #region Insert

        /// <summary>
        /// Insert a new document into collection. Document Id must be a new value in collection - Returns document Id
        /// </summary>
        public BsonValue Insert<T>(T entity, string collectionName = null)
        {
            return this.db.GetCollection<T>(collectionName).Insert(entity);
        }

        /// <summary>
        /// Insert an array of new documents into collection. Document Id must be a new value in collection. Can be set buffer size to commit at each N documents
        /// </summary>
        public int Insert<T>(IEnumerable<T> entities, string collectionName = null)
        {
            return this.db.GetCollection<T>(collectionName).Insert(entities);
        }

        #endregion

        #region Update

        /// <summary>
        /// Update a document into collection. Returns false if not found document in collection
        /// </summary>
        public bool Update<T>(T entity, string collectionName = null)
        {
            return this.db.GetCollection<T>(collectionName).Update(entity);
        }

        /// <summary>
        /// Update all documents
        /// </summary>
        public int Update<T>(IEnumerable<T> entities, string collectionName = null)
        {
            return this.db.GetCollection<T>(collectionName).Update(entities);
        }

        #endregion

        #region Upsert

        /// <summary>
        /// Insert or Update a document based on _id key. Returns true if insert entity or false if update entity
        /// </summary>
        public bool Upsert<T>(T entity, string collectionName = null)
        {
            return this.db.GetCollection<T>(collectionName).Upsert(entity);
        }

        /// <summary>
        /// Insert or Update all documents based on _id key. Returns entity count that was inserted
        /// </summary>
        public int Upsert<T>(IEnumerable<T> entities, string collectionName = null)
        {
            return this.db.GetCollection<T>(collectionName).Upsert(entities);
        }

        #endregion

        #region Delete

        /// <summary>
        /// Delete entity based on _id key
        /// </summary>
        public bool Delete<T>(BsonValue id, string collectionName = null)
        {
            return this.db.GetCollection<T>(collectionName).Delete(id);
        }

        /// <summary>
        /// Delete entity based on Query
        /// </summary>
        public int Delete<T>(Query query, string collectionName = null)
        {
            return this.db.GetCollection<T>(collectionName).Delete(query);
        }

        /// <summary>
        /// Delete entity based on predicate filter expression
        /// </summary>
        public int Delete<T>(Expression<Func<T, bool>> predicate, string collectionName = null)
        {
            return this.db.GetCollection<T>(collectionName).Delete(predicate);
        }

        #endregion

        #region Query

        /// <summary>
        /// Returns new instance of LiteQueryable that provides all method to query any entity inside collection. Use fluent API to apply filter/includes an than run any execute command, like ToList() or First()
        /// </summary>
        public LiteQueryable<T> Query<T>(string collectionName = null)
        {
            throw new NotImplementedException();
            // The class should later call original repository rather than the db directly, not needed for now
        }

        #endregion

        #region Shortcuts

        /// <summary>
        /// Search for a single instance of T by Id. Shortcut from Query.SingleById
        /// </summary>
        public T SingleById<T>(BsonValue id, string collectionName = null)
        {
            return this.Query<T>(collectionName).SingleById(id);
        }

        /// <summary>
        /// Execute Query[T].Where(query).ToList();
        /// </summary>
        public List<T> Fetch<T>(Query query = null, string collectionName = null)
        {
            return this.Query<T>(collectionName)
                       .Where(query ?? LiteDB.Query.All())
                       .ToList();
        }

        /// <summary>
        /// Execute Query[T].Where(query).ToList();
        /// </summary>
        public List<T> Fetch<T>(Expression<Func<T, bool>> predicate, string collectionName = null)
        {
            return this.Query<T>(collectionName)
                       .Where(predicate)
                       .ToList();
        }

        /// <summary>
        /// Execute Query[T].Where(query).First();
        /// </summary>
        public T First<T>(Query query = null, string collectionName = null)
        {
            return this.Query<T>(collectionName)
                       .Where(query ?? LiteDB.Query.All())
                       .First();
        }

        /// <summary>
        /// Execute Query[T].Where(query).First();
        /// </summary>
        public T First<T>(Expression<Func<T, bool>> predicate, string collectionName = null)
        {
            return this.Query<T>(collectionName)
                       .Where(predicate)
                       .First();
        }

        /// <summary>
        /// Execute Query[T].Where(query).FirstOrDefault();
        /// </summary>
        public T FirstOrDefault<T>(Query query = null, string collectionName = null)
        {
            return this.Query<T>(collectionName)
                       .Where(query ?? LiteDB.Query.All())
                       .FirstOrDefault();
        }

        /// <summary>
        /// Execute Query[T].Where(query).FirstOrDefault();
        /// </summary>
        public T FirstOrDefault<T>(Expression<Func<T, bool>> predicate, string collectionName = null)
        {
            return this.Query<T>(collectionName)
                       .Where(predicate)
                       .FirstOrDefault();
        }

        /// <summary>
        /// Execute Query[T].Where(query).Single();
        /// </summary>
        public T Single<T>(Query query = null, string collectionName = null)
        {
            return this.Query<T>(collectionName)
                       .Where(query ?? LiteDB.Query.All())
                       .Single();
        }

        /// <summary>
        /// Execute Query[T].Where(query).Single();
        /// </summary>
        public T Single<T>(Expression<Func<T, bool>> predicate, string collectionName = null)
        {
            return this.Query<T>(collectionName)
                       .Where(predicate)
                       .Single();
        }

        /// <summary>
        /// Execute Query[T].Where(query).SingleOrDefault();
        /// </summary>
        public T SingleOrDefault<T>(Query query = null, string collectionName = null)
        {
            return this.Query<T>(collectionName)
                       .Where(query ?? LiteDB.Query.All())
                       .SingleOrDefault();
        }

        /// <summary>
        /// Execute Query[T].Where(query).SingleOrDefault();
        /// </summary>
        public T SingleOrDefault<T>(Expression<Func<T, bool>> predicate, string collectionName = null)
        {
            return this.Query<T>(collectionName)
                       .Where(predicate)
                       .SingleOrDefault();
        }

        #endregion

        public void Dispose()
        {
            this.db.Dispose();
        }
    }
}