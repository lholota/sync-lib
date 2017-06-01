using System;
using LiteDB.Sync.Contract;

namespace LiteDB.Sync
{
    public class LiteSyncException : Exception
    {
        public static class ErrorCodes
        {
            public const int EntityDoesntImplementInterfaceErrorCode = 1;
            public const int ConflictNotResolvedErrorCode = 2;
            public const int ProviderAuthFailedErrorCode = 3;
        }

        public static LiteSyncException ProviderAuthFailed(Type providerType, Exception innerEx)
        {
            var message = string.Format("Authentication of the {0} provider failed.", providerType);

            return new LiteSyncException(ErrorCodes.ProviderAuthFailedErrorCode, message);
        }

        internal static LiteSyncException EntityDoesntImplementInterface(Type type)
        {
            var message = string.Format("The entity type {0} does not implement the {1} interface. " +
                                        "Only objects implementing this interface can be stored in a synced collection.",
                                        type, nameof(ILiteSyncEntity));

            return new LiteSyncException(ErrorCodes.EntityDoesntImplementInterfaceErrorCode, message);
        }

        internal static LiteSyncException ConflictNotResolved(GlobalEntityId globalId)
        {
            var message = string.Format("The conflict for the entity with Id {0} in the collection {2} was not resolved.",
                globalId.EntityId, globalId.CollectionName);

            return new LiteSyncException(ErrorCodes.ConflictNotResolvedErrorCode, message);
        }

        private LiteSyncException(int errorCode, string message, Exception innerEx = null)
            : base(message, innerEx)
        {
            this.ErrorCode = errorCode;
        }

        public int ErrorCode { get; }
    }
}