﻿using System;
using LiteDB.Sync.Contract;

namespace LiteDB.Sync.Exceptions
{
    public class LiteSyncConflictNotResolvedException : LiteSyncException
    {
        public LiteSyncConflictNotResolvedException(GlobalEntityId entityId, Exception innerEx = null) 
            : base($"The conflict for the entity {entityId} was not resolved.", innerEx) { }
    }
}