namespace LiteDB.Sync
{
    using System;
    using System.Collections.Generic;
    using Entities;

    public class LiteSyncTransaction : ILiteTransaction
    {
        private readonly Guid transactionId;
        private readonly ILiteTransaction originalTransaction;
        private readonly LiteSyncDatabase ownerDatabase;
        private readonly IList<DirtyEntity> dirtyEntities;

        internal LiteSyncTransaction(ILiteTransaction originalTransaction, LiteSyncDatabase ownerDatabase)
        {
            this.originalTransaction = originalTransaction;
            this.ownerDatabase = ownerDatabase;
            this.transactionId = Guid.NewGuid();
            this.dirtyEntities = new List<DirtyEntity>();
        }

        internal void AddDirtyEntity(string collectionName, BsonValue id)
        {
            var dirty = new DirtyEntity
            {
                TransactionId = this.transactionId,
                CollectionName = collectionName,
                EntityId = id
            };

            dirtyEntities.Add(dirty);
        }

        public void Commit()
        {
            this.originalTransaction.Commit();
            // TODO: Reset the transaction obj
        }

        public void Rollback()
        {
            this.originalTransaction.Rollback();
        }

        public void Dispose()
        {
            originalTransaction.Dispose();
            ownerDatabase.PopTransaction();
        }
    }
}