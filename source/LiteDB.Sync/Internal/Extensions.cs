using System;
using System.Collections.Generic;
using System.Reflection;

namespace LiteDB.Sync.Internal
{
    internal static class Extensions
    {
        internal static readonly Query FindDirtyEntitiesQuery =
            Query.Not(Query.EQ(nameof(ILiteSyncEntity.RequiresSync), new BsonValue(true)));

        internal static IEnumerable<BsonDocument> FindDirtyEntities(this ILiteCollection<BsonDocument> collection)
        {
            return collection.Find(FindDirtyEntitiesQuery);
        }

        internal static bool IsSyncEntityType(this Type type)
        {
            return typeof(ILiteSyncEntity).IsAssignableFrom(type);
        }

        //internal static CloudState GetLocalCloudState(this ILiteDatabase db)
        //{
        //    return db.GetCollection<CloudState>(LiteSyncDatabase.SyncStateCollectionName).FindById(LiteSyncDatabase.LocalCloudStateId);
        //}

        //internal static void SaveLocalCloudState(this ILiteDatabase db, CloudState cloudState)
        //{
        //    db.GetCollection<CloudState>(LiteSyncDatabase.SyncStateCollectionName).Upsert(LiteSyncDatabase.LocalCloudStateId, cloudState);
        //}
    }
}