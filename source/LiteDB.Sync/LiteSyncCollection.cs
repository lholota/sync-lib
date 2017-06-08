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

        internal LiteSyncCollection(ILiteCollection<T> nativeCollection, LiteSyncDatabase database)
        {
            this.NativeCollection = nativeCollection.NativeCollection;
            this.database = database;
        }

        public LiteCollection<T> NativeCollection { get; }

        public string Name => this.NativeCollection.Name;

        public int Count()
        {
            return this.NativeCollection.Count();
        }

        public int Count(Query query)
        {
            return this.NativeCollection.Count(query);
        }

        public int Count(Expression<Func<T, bool>> predicate)
        {
            return this.NativeCollection.Count(predicate);
        }

        public bool Delete(BsonValue id)
        {
            bool result;

            using (var tx = this.database.BeginTrans())
            {
                result = this.NativeCollection.Delete(id);

                if (result)
                {
                    var deletedEntity = new DeletedEntity(this.Name, id);
                    this.database.GetDeletedEntitiesCollection().Insert(deletedEntity);
                }

                tx.Commit();
            }

            return result;
        }

        public int Delete(Query query)
        {
            return this.Delete(query, out IList<BsonValue> _);
        }

        public int Delete(Expression<Func<T, bool>> predicate)
        {
            return this.Delete(predicate, out IList<BsonValue> _);
        }

        public int Delete(Query query, out IList<BsonValue> deletedIds)
        {
            int result;

            using (var tx = this.database.BeginTrans())
            {
                result = this.NativeCollection.Delete(query, out deletedIds);

                if (deletedIds != null && deletedIds.Count > 0)
                {
                    var deletedEntities = deletedIds.Select(x => new DeletedEntity(this.Name, x));
                    this.database.GetDeletedEntitiesCollection().Insert(deletedEntities);
                }

                tx.Commit();
            }

            return result;
        }

        public int Delete(Expression<Func<T, bool>> predicate, out IList<BsonValue> deletedIds)
        {
            int result;

            using (var tx = this.database.BeginTrans())
            {
                result = this.NativeCollection.Delete(predicate, out deletedIds);

                if (deletedIds != null && deletedIds.Count > 0)
                {
                    var deletedEntities = deletedIds.Select(x => new DeletedEntity(this.Name, x));
                    this.database.GetDeletedEntitiesCollection().Insert(deletedEntities);
                }

                tx.Commit();
            }

            return result;
        }

        public bool DropIndex(string field)
        {
            return this.NativeCollection.DropIndex(field);
        }

        public bool EnsureIndex(string field, bool unique = false)
        {
            return this.NativeCollection.EnsureIndex(field, unique);
        }

        public bool EnsureIndex<TProp>(Expression<Func<T, TProp>> property, bool unique = false)
        {
            return this.NativeCollection.EnsureIndex(property, unique);
        }

        public bool Exists(Query query)
        {
            return this.NativeCollection.Exists(query);
        }

        public bool Exists(Expression<Func<T, bool>> predicate)
        {
            return this.NativeCollection.Exists(predicate);
        }

        public IEnumerable<T> Find(Query query, int skip = 0, int limit = int.MaxValue)
        {
            return this.NativeCollection.Find(query, skip, limit);
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> predicate, int skip = 0, int limit = int.MaxValue)
        {
            return this.NativeCollection.Find(predicate, skip, limit);
        }

        public IEnumerable<T> FindAll()
        {
            return this.NativeCollection.FindAll();
        }

        public T FindById(BsonValue id)
        {
            return this.NativeCollection.FindById(id);
        }

        public T FindOne(Query query)
        {
            return this.NativeCollection.FindOne(query);
        }

        public T FindOne(Expression<Func<T, bool>> predicate)
        {
            return this.NativeCollection.FindOne(predicate);
        }

        public IEnumerable<IndexInfo> GetIndexes()
        {
            return this.NativeCollection.GetIndexes();
        }

        public LiteCollection<T> Include<TProp>(Expression<Func<T, TProp>> dbref)
        {
            return this.NativeCollection.Include(dbref);
        }

        public LiteCollection<T> Include(string path)
        {
            return this.NativeCollection.Include(path);
        }

        public BsonValue Insert(T document)
        {
            BsonValue result;
            
            using (var tx = this.database.BeginTrans())
            {
                this.MarkDirty(document);

                result = this.NativeCollection.Insert(document);

                this.RemoveDeletedEntity(result);

                tx.Commit();
            }

            return result;
        }

        public void Insert(BsonValue id, T document)
        {
            using (var tx = this.database.BeginTrans())
            {
                this.MarkDirty(document);

                this.NativeCollection.Insert(id, document);

                this.RemoveDeletedEntity(id);

                tx.Commit();
            }
        }

        public int Insert(IEnumerable<T> docs)
        {
            int result;

            using (var tx = this.database.BeginTrans())
            {
                this.MarkDirty(docs);

                result = this.NativeCollection.Insert(docs);

                if (result > 0)
                {
                    this.RemoveDeletedEntities(docs);
                }

                tx.Commit();
            }

            return result;
        }

        public int InsertBulk(IEnumerable<T> docs, int batchSize = 5000, Action<IEnumerable<BsonDocument>> batchInsertedAction = null)
        {
            this.MarkDirty(docs);

            return this.NativeCollection.InsertBulk(docs, batchSize, batch =>
            {
                var entityIds = batch.Select(doc => doc["_id"]);

                this.RemoveDeletedEntities(entityIds);

                batchInsertedAction?.Invoke(batch);
            });
        }

        public long LongCount()
        {
            return this.NativeCollection.LongCount();
        }

        public long LongCount(Query query)
        {
            return this.NativeCollection.LongCount(query);
        }

        public long LongCount(Expression<Func<T, bool>> predicate)
        {
            return this.NativeCollection.LongCount(predicate);
        }

        public BsonValue Max(string field)
        {
            return this.NativeCollection.Max(field);
        }

        public BsonValue Max()
        {
            return this.NativeCollection.Max();
        }

        public BsonValue Max<TProp>(Expression<Func<T, TProp>> property)
        {
            return this.NativeCollection.Max(property);
        }

        public BsonValue Min(string field)
        {
            return this.NativeCollection.Min(field);
        }

        public BsonValue Min()
        {
            return this.NativeCollection.Min();
        }

        public BsonValue Min<TProp>(Expression<Func<T, TProp>> property)
        {
            return this.NativeCollection.Min(property);
        }

        public bool Update(T entity)
        {
            this.MarkDirty(entity);

            return this.NativeCollection.Update(entity);
        }

        public bool Update(BsonValue id, T document)
        {
            this.MarkDirty(document);

            return this.NativeCollection.Update(id, document);
        }

        public int Update(IEnumerable<T> entities)
        {
            this.MarkDirty(entities);

            return this.NativeCollection.Update(entities);
        }

        public bool Upsert(T entity)
        {
            bool result;

            using (var tx = this.database.BeginTrans())
            {
                this.MarkDirty(entity);

                result = this.NativeCollection.Upsert(entity);

                if (result)
                {
                    var id = this.database.Mapper.GetEntityId(entity);
                    this.RemoveDeletedEntity(id);
                }

                tx.Commit();
            }

            return result;
        }

        public bool Upsert(BsonValue id, T document)
        {
            bool result;

            using (var tx = this.database.BeginTrans())
            {
                this.MarkDirty(document);

                result = this.NativeCollection.Upsert(id, document);

                if (result)
                {
                    this.RemoveDeletedEntity(id);
                }

                tx.Commit();
            }

            return result;
        }

        public int Upsert(IEnumerable<T> entities)
        {
            int result;

            using (var tx = this.database.BeginTrans())
            {
                this.MarkDirty(entities);

                result = this.NativeCollection.Upsert(entities);

                if (result > 0)
                {
                    var deletedEntCollection = this.database.GetDeletedEntitiesCollection();

                    foreach (var entity in entities)
                    {
                        var docId = this.database.Mapper.GetEntityId(entity);
                        var deletedEntityId = new EntityId(this.Name, docId);
                        var deletedEntityIdBson = this.database.Mapper.ToDocument(deletedEntityId);

                        deletedEntCollection.Delete(deletedEntityIdBson);
                    }
                }

                tx.Commit();
            }

            return result;
        }

        private void RemoveDeletedEntities(IEnumerable<T> docs)
        {
            var docIds = docs.Select(doc => this.database.Mapper.GetEntityId(doc));

            this.RemoveDeletedEntities(docIds);
        }

        private void RemoveDeletedEntities(IEnumerable<BsonValue> ids)
        {
            foreach (var id in ids)
            {
                this.RemoveDeletedEntity(id);
            }
        }

        private void RemoveDeletedEntity(BsonValue id)
        {
            var deletedEntCollection = this.database.GetDeletedEntitiesCollection();

            var deletedEntityId = new EntityId(this.Name, id);
            var deletedEntityIdBson = this.database.Mapper.ToDocument(deletedEntityId);

            deletedEntCollection.Delete(deletedEntityIdBson);
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