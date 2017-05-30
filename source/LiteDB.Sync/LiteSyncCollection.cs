using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using LiteDB.Sync.Internal;

namespace LiteDB.Sync
{
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
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
            int result;

            using (var tx = this.database.BeginTrans())
            {
                result = this.UnderlyingCollection.Delete(query, out IList<BsonValue> deletedIds);

                if (deletedIds != null && deletedIds.Count > 0)
                {
                    var deletedEntities = deletedIds.Select(x => new DeletedEntity(this.Name, x));
                    this.database.GetDeletedEntitiesCollection().Insert(deletedEntities);
                }

                tx.Commit();
            }

            return result;
        }

        public int Delete(Expression<Func<T, bool>> predicate)
        {
            int result;

            using (var tx = this.database.BeginTrans())
            {
                result = this.UnderlyingCollection.Delete(predicate, out IList<BsonValue> deletedIds);

                if (deletedIds != null && deletedIds.Count > 0)
                {
                    var deletedEntities = deletedIds.Select(x => new DeletedEntity(this.Name, x));
                    this.database.GetDeletedEntitiesCollection().Insert(deletedEntities);
                }

                tx.Commit();
            }

            return result;
        }

        public bool Delete(BsonValue id)
        {
            bool result;

            using (var tx = this.database.BeginTrans())
            {
                result = this.UnderlyingCollection.Delete(id);

                if (result)
                {
                    var deletedEntity = new DeletedEntity(this.Name, id);
                    this.database.GetDeletedEntitiesCollection().Insert(deletedEntity);
                }

                tx.Commit();
            }

            return result;
        }

        public int Delete(Query query, out IList<BsonValue> deletedIds)
        {
            throw new NotImplementedException();
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
            this.MarkDirty(document);

            return this.UnderlyingCollection.Insert(document);
        }

        public void Insert(BsonValue id, T document)
        {
            this.MarkDirty(document);

            this.UnderlyingCollection.Insert(id, document);
        }

        public int Insert(IEnumerable<T> docs)
        {
            this.MarkDirty(docs);

            return this.UnderlyingCollection.Insert(docs);
        }

        public int InsertBulk(IEnumerable<T> docs, int batchSize = 5000)
        {
            this.MarkDirty(docs);

            return this.UnderlyingCollection.InsertBulk(docs, batchSize);
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
            this.MarkDirty(entity);

            return this.UnderlyingCollection.Update(entity);
        }

        public bool Update(BsonValue id, T document)
        {
            this.MarkDirty(document);

            return this.UnderlyingCollection.Update(id, document);
        }

        public int Update(IEnumerable<T> entities)
        {
            this.MarkDirty(entities);

            return this.UnderlyingCollection.Update(entities);
        }

        public bool Upsert(T entity)
        {
            this.MarkDirty(entity);

            return this.UnderlyingCollection.Upsert(entity);
        }

        public bool Upsert(BsonValue id, T document)
        {
            this.MarkDirty(document);

            return this.UnderlyingCollection.Upsert(id, document);
        }

        public int Upsert(IEnumerable<T> entities)
        {
            this.MarkDirty(entities);

            return this.UnderlyingCollection.Upsert(entities);
        }

        private void MarkDirty(IEnumerable<T> entities)
        {
            if (entities == null)
            {
                return;
            }

            foreach (var entity in entities)
            {
                this.MarkDirty(entity);
            }
        }

        private void MarkDirty(T entity)
        {
            var syncedEntity = entity as ILiteSyncEntity;

            if (syncedEntity != null)
            {
                syncedEntity.RequiresSync = true;
            }
        }
    }
}