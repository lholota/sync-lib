using System;
using System.Reflection;

namespace LiteDB.Sync.Internal
{
    internal static class Extensions
    {
        internal static DeletedEntity FindById(this ILiteCollection<DeletedEntity> collection, BsonMapper mapper, EntityId id)
        {
            return collection.FindById(mapper.ToDocument(id));
        }

        internal static bool IsSyncEntityType(this Type type)
        {
            return typeof(ILiteSyncEntity).IsAssignableFrom(type);
        }

        internal static BsonValue GetId(this BsonDocument doc)
        {
            return doc["_id"];
        }
    }
}