using System;

namespace LiteDB.Sync.Exceptions
{
    public class LiteSyncConflictOccuredException : LiteSyncException
    {
        public LiteSyncConflictOccuredException(Exception innerEx = null) 
            : base("Conflict occured", innerEx) { }
    }
}