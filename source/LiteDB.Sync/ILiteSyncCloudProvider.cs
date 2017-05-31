using System.Threading;
using LiteDB.Sync.Contract;

namespace LiteDB.Sync
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ILiteSyncCloudProvider
    {
        Task<IList<Patch>> Pull(Guid? localHeadId, CancellationToken ct);

        Task Push(Patch args, CancellationToken ct);
    }
}