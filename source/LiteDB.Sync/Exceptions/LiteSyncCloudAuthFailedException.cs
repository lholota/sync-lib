using System;

namespace LiteDB.Sync.Exceptions
{
    public class LiteSyncCloudAuthFailedException : LiteSyncException
    {
        public LiteSyncCloudAuthFailedException(Type providerType, Exception innerEx)
            : base($"Authentication of the {providerType} provider failed.", innerEx)
        {
        }
    }
}