using System;

namespace LiteDB.Sync
{
    public class LiteSyncException : Exception
    {
        public static class ErrorCodes
        {
            public const int EntityDoesntImplementInterface = 1;
        }

        internal static LiteSyncException EntityDoesntImplementInterface(Type type)
        {
            var message = string.Format("The entity type {0} does not implement the {1} interface. " +
                                        "Only objects implementing this interface can be stored in a synced collection.",
                                        type, nameof(ILiteSyncEntity));

            return new LiteSyncException(ErrorCodes.EntityDoesntImplementInterface, message);
        }

        private LiteSyncException(int errorCode, string message)
            : base(message)
        {
            this.ErrorCode = errorCode;
        }

        public int ErrorCode { get; }
    }
}