namespace LiteDB.Sync.Internal
{
    public class DeleteEntityChange : EntityChangeBase
    {
        public DeleteEntityChange(EntityId entityId)
            : base(entityId)
        {   
        }

        internal override void Apply(ILiteCollection<BsonDocument> collection)
        {
            collection.Delete(this.EntityId.BsonId);
        }
    }
}