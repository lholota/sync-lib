using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LiteDB.Sync
{
    public class LiteSyncCollection<T> : ILiteCollection<T>
    {
        private readonly LiteSyncDatabase database;

        internal LiteSyncCollection(ILiteCollection<T> underlyingCollection, LiteSyncDatabase database)
        {
            this.UnderlyingCollection = underlyingCollection.UnderlyingCollection;
            this.database = database;
        }

        public LiteCollection<T> UnderlyingCollection { get; }

        public string Name => this.UnderlyingCollection.Name;

        public int Count()
        {
            return this.UnderlyingCollection.Count();
        }

        public int Count(Query query)
        {
            return this.UnderlyingCollection.Count(query);
        }

        public int Count(Expression<Func<T, bool>> predicate)
        {
            return this.UnderlyingCollection.Count(predicate);
        }

        public int Delete(Query query)
        {
            return this.UnderlyingCollection.Delete(query);
        }

        public int Delete(Expression<Func<T, bool>> predicate)
        {
            return this.UnderlyingCollection.Delete(predicate);
        }

        public bool Delete(BsonValue id)
        {
            return this.UnderlyingCollection.Delete(id);
        }

        public bool DropIndex(string field)
        {
            return this.UnderlyingCollection.Delete(field);
        }

        public bool EnsureIndex(string field, bool unique = false)
        {
            return this.UnderlyingCollection.EnsureIndex(field, unique);
        }

        public bool EnsureIndex<K>(Expression<Func<T, K>> property, bool unique = false)
        {
            return this.UnderlyingCollection.EnsureIndex(property, unique);
        }

        public bool Exists(Query query)
        {
            return this.UnderlyingCollection.Exists(query);
        }

        public bool Exists(Expression<Func<T, bool>> predicate)
        {
            return this.UnderlyingCollection.Exists(predicate);
        }

        public IEnumerable<T> Find(Query query, int skip = 0, int limit = int.MaxValue)
        {
            return this.UnderlyingCollection.Find(query, skip, limit);
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate, int skip = 0, int limit = int.MaxValue)
        {
            return this.UnderlyingCollection.Find(predicate, skip, limit);
        }

        public IEnumerable<T> FindAll()
        {
            return this.UnderlyingCollection.FindAll();
        }

        public T FindById(BsonValue id)
        {
            return this.UnderlyingCollection.FindById(id);
        }

        public T FindOne(Query query)
        {
            return this.UnderlyingCollection.FindOne(query);
        }

        public T FindOne(Expression<Func<T, bool>> predicate)
        {
            return this.UnderlyingCollection.FindOne(predicate);
        }

        public IEnumerable<IndexInfo> GetIndexes()
        {
            return this.UnderlyingCollection.GetIndexes();
        }

        public LiteCollection<T> Include<K>(Expression<Func<T, K>> dbref)
        {
            return this.UnderlyingCollection.Include(dbref);
        }

        public LiteCollection<T> Include(string path)
        {
            return this.UnderlyingCollection.Include(path);
        }

        public BsonValue Insert(T document)
        {
            return this.UnderlyingCollection.Insert(document);
        }

        public void Insert(BsonValue id, T document)
        {
            this.UnderlyingCollection.Insert(id, document);
        }

        public int Insert(IEnumerable<T> docs)
        {
            return this.UnderlyingCollection.Insert(docs);
        }

        public int InsertBulk(IEnumerable<T> docs, int batchSize = 5000)
        {
            throw new NotSupportedException("This doesn't support transactions...");
        }

        public long LongCount()
        {
            return this.UnderlyingCollection.LongCount();
        }

        public long LongCount(Query query)
        {
            return this.UnderlyingCollection.LongCount(query);
        }

        public long LongCount(Expression<Func<T, bool>> predicate)
        {
            return this.UnderlyingCollection.LongCount(predicate);
        }

        public BsonValue Max(string field)
        {
            return this.UnderlyingCollection.Max(field);
        }

        public BsonValue Max()
        {
            return this.UnderlyingCollection.Max();
        }

        public BsonValue Max<K>(Expression<Func<T, K>> property)
        {
            return this.UnderlyingCollection.Max(property);
        }

        public BsonValue Min(string field)
        {
            return this.UnderlyingCollection.Min(field);
        }

        public BsonValue Min()
        {
            return this.UnderlyingCollection.Min();
        }

        public BsonValue Min<K>(Expression<Func<T, K>> property)
        {
            return this.UnderlyingCollection.Min(property);
        }

        public bool Update(T entity)
        {
            bool result;

            using (var trans = this.database.BeginTrans())
            {
                result = this.UnderlyingCollection.Update(entity);

                var document = this.database.Mapper.ToDocument(entity);

                if (result)
                {
                    this.database.SyncTransaction.AddDirtyEntity(this.Name, document["_id"]);
                }

                trans.Commit();
            }

            return result;
        }

        public bool Update(BsonValue id, T document)
        {
            bool result;

            using (var trans = this.database.BeginTrans())
            {
                result = this.UnderlyingCollection.Update(id, document);

                if (result)
                {
                    this.database.SyncTransaction.AddDirtyEntity(this.Name, id);
                }

                trans.Commit();
            }

            return result;
        }

        public int Update(IEnumerable<T> entities)
        {
            var result = 0;

            using (var trans = this.database.BeginTrans())
            {
                foreach (var entity in entities)
                {
                    var document = this.database.Mapper.ToDocument(entity);

                    if (this.UnderlyingCollection.Update(entity))
                    {
                        this.database.SyncTransaction.AddDirtyEntity(this.Name, document["_id"]);
                    }
                }

                trans.Commit();
            }

            return result;
        }

        public bool Upsert(T entity)
        {
            bool result;

            using (var trans = this.database.BeginTrans())
            {
                result = this.UnderlyingCollection.Upsert(entity);

                var document = this.database.Mapper.ToDocument(entity);

                if (result)
                {
                    this.database.SyncTransaction.AddDirtyEntity(this.Name, document["_id"]);
                }

                trans.Commit();
            }

            return result;
        }

        public bool Upsert(BsonValue id, T document)
        {
            bool result;

            using (var trans = this.database.BeginTrans())
            {
                result = this.UnderlyingCollection.Upsert(document);

                if (result)
                {
                    this.database.SyncTransaction.AddDirtyEntity(this.Name, id);
                }

                trans.Commit();
            }

            return result;
        }

        public int Upsert(IEnumerable<T> entities)
        {
            var result = 0;

            using (var trans = this.database.BeginTrans())
            {
                foreach (var entity in entities)
                {
                    var document = this.database.Mapper.ToDocument(entity);

                    if (this.UnderlyingCollection.Upsert(entity))
                    {
                        this.database.SyncTransaction.AddDirtyEntity(this.Name, document["_id"]);
                    }
                }

                trans.Commit();
            }

            return result;
        }
    }
}