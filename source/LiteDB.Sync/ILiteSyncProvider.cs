namespace LiteDB.Sync
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Entities;

    public interface ILiteSyncProvider
    {
        Task<IList<Transaction>> Pull(Guid latestTransactionId);

        Task Push(object args);
    }
}