using System;

namespace LiteDB.Sync.Exceptions
{
    public class LiteSyncCloudOperationFailedException : LiteSyncException
    {
        public LiteSyncCloudOperationFailedException(string operationName, Exception innerEx)
            : base($"The cloud operation {operationName} failed.", innerEx)
        {
        }
    }
}