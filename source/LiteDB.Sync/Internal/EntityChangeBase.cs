using System;

namespace LiteDB.Sync.Internal
{
    public abstract class EntityChangeBase
    {
        protected EntityChangeBase(EntityId entityId)
        {
            this.EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
        }

        public EntityId EntityId { get; }

        internal abstract void Apply(ILiteCollection<BsonDocument> collection);
    }
}