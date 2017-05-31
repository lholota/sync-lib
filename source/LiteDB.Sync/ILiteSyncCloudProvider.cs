using System;
using System.Threading;
using System.Threading.Tasks;
using LiteDB.Sync.Contract;

namespace LiteDB.Sync
{
    public interface ILiteSyncCloudProvider
    {
        Task<LiteSyncPullResult> Pull(Guid? localHeadId, CancellationToken ct);

        Task Push(Patch args, object pullEtag, CancellationToken ct);
    }
}