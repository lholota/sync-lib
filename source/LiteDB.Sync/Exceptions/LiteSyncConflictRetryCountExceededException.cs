using System;

namespace LiteDB.Sync.Exceptions
{
    public class LiteSyncConflictRetryCountExceededException : LiteSyncException
    {
        public LiteSyncConflictRetryCountExceededException(int retryCount, Exception lastException)
            : base(string.Format("The retry count ({0}) for sync/push operation via provider was exceeded.", retryCount), lastException)
        {
        }
    }
}