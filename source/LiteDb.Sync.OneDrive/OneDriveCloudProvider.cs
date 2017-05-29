namespace LiteDb.Sync.OneDrive
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LiteDB.Sync;
    using LiteDB.Sync.Entities;

    public class OneDriveCloudProvider : ILiteSyncCloudProvider
    {
        public Task<IList<Patch>> Pull(Guid localHeadId)
        {
            throw new NotImplementedException();
        }

        public Task Push(object args)
        {
            throw new NotImplementedException();
        }
    }
}