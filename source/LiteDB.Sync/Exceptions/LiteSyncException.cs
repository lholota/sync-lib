using System;

namespace LiteDB.Sync.Exceptions
{
    public class LiteSyncException : Exception
    {
        internal LiteSyncException(string message, Exception innerEx = null)
            : base(message, innerEx)
        {
        }
    }
}