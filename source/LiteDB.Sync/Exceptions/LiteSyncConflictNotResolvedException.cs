using System;
using LiteDB.Sync.Internal;

namespace LiteDB.Sync.Exceptions
{
    public class LiteSyncConflictNotResolvedException : LiteSyncException
    {
        public LiteSyncConflictNotResolvedException(EntityId entityId, Exception innerEx = null) 
            : base($"The conflict for the entity {entityId} was not resolved.", innerEx)
        { }
    }
}