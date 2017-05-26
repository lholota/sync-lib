namespace LiteDB.Sync
{
    using System;

    public class LiteSyncTransaction : IDisposable
    {
        private readonly LiteTransaction originalTransaction;

        internal LiteSyncTransaction(LiteTransaction originalTransaction)
        {
            this.originalTransaction = originalTransaction;
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
        }
    }
}