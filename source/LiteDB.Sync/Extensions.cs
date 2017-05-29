namespace LiteDB.Sync
{
    using System.Collections.Generic;
    using System.Linq;
    using Entities;
    using Internal;

    internal static class Extensions
    {
        private const string LocalHeadId = "LocalHead";
        private const string SyncStateCollectionName = "LiteSync_State";
        private const string DeletedEntitiesCollectionName = "LiteSync_Deleted";

        internal static IEnumerable<BsonDocument> FindDirtyEntities(this ILiteCollection<BsonDocument> collection)
        {
            var query = Query.EQ(nameof(ILiteSyncEntity.RequiresSync), new BsonValue(true));

            return collection.Find(query);
        }

        internal static Head GetSyncHead(this ILiteDatabase db)
        {
            return db.GetCollection<Head>(SyncStateCollectionName).FindById(LocalHeadId);
        }

        internal static void InsertDeletedEntity(this ILiteDatabase db, string collectionName, BsonValue id)
        {
            var pointer = new DeletedEntity
            {
                CollectionName = collectionName,
                EntityId = id
            };

            db.GetCollection<DeletedEntity>(DeletedEntitiesCollectionName).Insert(pointer);
        }

        internal static void InsertDeletedEntities(this ILiteDatabase db, string collectionName, IEnumerable<BsonValue> ids)
        {
            var pointers = ids.Select(x => new DeletedEntity
            {
                CollectionName = collectionName,
                EntityId = x
            });

            db.GetCollection<DeletedEntity>(DeletedEntitiesCollectionName).Insert(pointers);
        }

        internal static IEnumerable<DeletedEntity> FindDeletedEntities(this ILiteDatabase db)
        {
            var collection = db.GetCollection<DeletedEntity>(DeletedEntitiesCollectionName);

            return collection.FindAll();
        }
    }
}