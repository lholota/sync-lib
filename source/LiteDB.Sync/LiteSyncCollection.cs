using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using LiteDB.Sync.Contract;

namespace LiteDB.Sync
{
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public class LiteSyncCollection<T> : ILiteCollection<T>
    {
        private readonly Query ignoreDeletedQuery;
        private readonly Expression<Func<T, bool>> ignoreDeletedPredicate = x => ((ILiteSyncEntity)x).SyncState != SyncState.RequiresSyncDeleted;
        private readonly LiteSyncDatabase database;

        internal LiteSyncCollection(ILiteCollection<T> underlyingCollection, LiteSyncDatabase database)
        {
            this.UnderlyingCollection = underlyingCollection.UnderlyingCollection;
            this.database = database;

            this.ignoreDeletedQuery = this.CreateIgnoreSoftDeletedQuery();
        }

        public LiteCollection<T> UnderlyingCollection { get; }

        public string Name => this.UnderlyingCollection.Name;

        public int Count()
        {
            return this.UnderlyingCollection.Count(this.ignoreDeletedQuery);
        }

        public int Count(Query query)
        {
            var combined = Query.And(query, this.ignoreDeletedQuery);
            return this.UnderlyingCollection.Count(combined);
        }

        public int Count(Expression<Func<T, bool>> predicate)
        {
            var combined = this.CombineWithIgnoreDeleted(predicate);
            return this.UnderlyingCollection.Count(combined);
        }

        public int Delete(Query query)
        {
            return this.BatchDelete(() => this.UnderlyingCollection.Find(query));
        }

        public int Delete(Expression<Func<T, bool>> predicate)
        {
            return this.BatchDelete(() => this.UnderlyingCollection.Find(predicate));
        }

        public bool Delete(BsonValue id)
        {
            bool result;

            using (var tx = this.database.BeginTrans())
            {
                var item = this.UnderlyingCollection.FindById(id);

                if (item == null)
                {
                    return false;
                }

                var syncItem = (ILiteSyncEntity) item;

                if (syncItem.SyncState == SyncState.RequiresSyncDeleted)
                {
                    return false;
                }

                syncItem.SyncState = SyncState.RequiresSyncDeleted;

                result = this.UnderlyingCollection.Update(item);

                tx.Commit();
            }

            return result;
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
            var combined = Query.And(query, this.ignoreDeletedQuery);
            return this.UnderlyingCollection.Exists(combined);
        }

        public bool Exists(Expression<Func<T, bool>> predicate)
        {
            var combined = this.CombineWithIgnoreDeleted(predicate);
            return this.UnderlyingCollection.Exists(combined);
        }

        public IEnumerable<T> Find(Query query, int skip = 0, int limit = int.MaxValue)
        {
            var combined = Query.And(query, this.ignoreDeletedQuery);
            return this.UnderlyingCollection.Find(combined, skip, limit);
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate, int skip = 0, int limit = int.MaxValue)
        {
            var combined = this.CombineWithIgnoreDeleted(predicate);
            return this.UnderlyingCollection.Find(combined, skip, limit);
        }

        public IEnumerable<T> FindAll()
        {
            return this.UnderlyingCollection.Find(this.ignoreDeletedPredicate);
        }

        public T FindById(BsonValue id)
        {
            var result = this.UnderlyingCollection.FindById(id);

            var syncResult = (ILiteSyncEntity) result;

            if (syncResult.SyncState == SyncState.RequiresSyncDeleted)
            {
                return default(T);
            }

            return result;
        }

        public T FindOne(Query query)
        {
            var combined = Query.And(query, this.ignoreDeletedQuery);
            return this.UnderlyingCollection.FindOne(combined);
        }

        public T FindOne(Expression<Func<T, bool>> predicate)
        {
            var combined = this.CombineWithIgnoreDeleted(predicate);
            return this.UnderlyingCollection.FindOne(combined);
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
            return this.UnderlyingCollection.LongCount(this.ignoreDeletedQuery);
        }

        public long LongCount(Query query)
        {
            var combined = Query.And(query, this.ignoreDeletedQuery);
            return this.UnderlyingCollection.LongCount(combined);
        }

        public long LongCount(Expression<Func<T, bool>> predicate)
        {
            var combined = this.CombineWithIgnoreDeleted(predicate);
            return this.UnderlyingCollection.LongCount(combined);
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
            MarkDirty(entity);

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

        private Expression<Func<T, bool>> CombineWithIgnoreDeleted(Expression<Func<T, bool>> original)
        {
            var body = Expression.AndAlso(this.ignoreDeletedPredicate.Body, original.Body);
            return Expression.Lambda<Func<T, bool>>(body, original.Parameters[0]);
        }

        private int BatchDelete(Func<IEnumerable<T>> findAllFunc)
        {
            int result = 0;

            using (var tx = this.database.BeginTrans())
            {
                var toBeDeleted = findAllFunc.Invoke();

                foreach (var item in toBeDeleted)
                {
                    var syncItem = (ILiteSyncEntity) item;

                    if (syncItem.SyncState == SyncState.RequiresSyncDeleted)
                    {
                        continue;
                    }

                    syncItem.SyncState = SyncState.RequiresSyncDeleted;

                    this.UnderlyingCollection.Update(item);
                    result++;
                }

                tx.Commit();
            }

            return result;
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
                syncedEntity.SyncState = SyncState.RequiresSync;
            }
        }

        private Query CreateIgnoreSoftDeletedQuery()
        {
            var fieldName = this.database.Mapper.ResolveFieldName.Invoke(nameof(ILiteSyncEntity.SyncState));
            return Query.Not(Query.EQ(fieldName, new BsonValue(SyncState.RequiresSyncDeleted)));
        }
    }
}