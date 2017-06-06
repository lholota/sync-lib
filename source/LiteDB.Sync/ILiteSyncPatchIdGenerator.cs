using System.Threading;
using System.Threading.Tasks;

namespace LiteDB.Sync
{
    public interface ILiteSyncPatchIdGenerator
    {
        Task<string> GeneratePatchId(CancellationToken ct);
    }
}