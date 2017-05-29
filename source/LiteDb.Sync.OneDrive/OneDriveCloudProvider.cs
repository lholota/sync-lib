using LiteDB.Sync.Contract;

namespace LiteDb.Sync.OneDrive
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LiteDB.Sync;

    public class OneDriveCloudProvider : ILiteSyncCloudProvider
    {
        public Task<IList<Patch>> Pull(Guid localHeadId)
        {
            throw new NotImplementedException();
        }

        public Task Push(Patch args)
        {
            throw new NotImplementedException();
        }
    }
}