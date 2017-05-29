namespace LiteDB.Sync
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Entities;

    public interface ILiteSyncCloudProvider
    {
        Task<IList<Patch>> Pull(Guid localHeadId);

        Task Push(Patch args);
    }
}