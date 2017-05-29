using LiteDB.Sync.Contract;

namespace LiteDB.Sync
{
    using System.Collections.Generic;

    internal static class Extensions
    {
        private const string LocalHeadId = "LocalHead";
        private const string SyncStateCollectionName = "LiteSync_State";

        internal static IEnumerable<BsonDocument> FindDirtyEntities(this ILiteCollection<BsonDocument> collection)
        {
            var query = Query.Not(Query.EQ(nameof(ILiteSyncEntity.SyncState), new BsonValue(EntitySyncState.None)));

            return collection.Find(query);
        }

        internal static Head GetSyncHead(this ILiteDatabase db)
        {
            return db.GetCollection<Head>(SyncStateCollectionName).FindById(LocalHeadId);
        }
    }
}