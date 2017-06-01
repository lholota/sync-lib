using System;

namespace LiteDB.Sync.Exceptions
{
    public class LiteSyncInvalidEntityException : LiteSyncException
    {
        public LiteSyncInvalidEntityException(Type entityType)
            : base($"The entity type {entityType} does not implement the {nameof(ILiteSyncEntity)} interface. Only objects implementing this interface can be stored in a synced collection.")
        {
        }
    }
}