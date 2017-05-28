namespace LiteDB.Sync
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Entities;

    public interface ILiteSyncCloudProvider
    {
        Task<IList<Transaction>> Pull(Guid latestTransactionId);

        Task Push(object args);
    }
}