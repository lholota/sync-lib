namespace LiteDB.Sync.Internal
{
    internal static class BsonMappings
    {
        internal static void RegisterCustomMappings(this BsonMapper mapper)
        {
            mapper.RegisterType(SerializeEntityId, DeserializeEntityId);
            mapper.RegisterType(SerializeDeletedEntity, DeserializeDeletedEntity);
        }

        private static DeletedEntity DeserializeDeletedEntity(BsonValue bson)
        {
            var doc = bson.AsDocument;

            var entityId = DeserializeEntityId(doc["_id"]);

            return new DeletedEntity(entityId);
        }

        private static BsonValue SerializeDeletedEntity(DeletedEntity ent)
        {
            var doc = new BsonDocument();
            doc["_id"] = SerializeEntityId(ent.EntityId);

            return doc;
        }

        private static EntityId DeserializeEntityId(BsonValue bson)
        {
            var doc = bson.AsDocument;
            var collName = doc[nameof(EntityId.CollectionName)].AsString;
            var id = doc[nameof(EntityId.BsonId)];

            return new EntityId(collName, id);
        }

        private static BsonValue SerializeEntityId(EntityId id)
        {
            var doc = new BsonDocument();
            doc[nameof(EntityId.CollectionName)] = id.CollectionName;
            doc[nameof(EntityId.BsonId)] = id.BsonId;

            return doc;
        }
    }
}