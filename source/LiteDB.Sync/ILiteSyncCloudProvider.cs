using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LiteDB.Sync
{
    public interface ILiteSyncCloudProvider
    {
        Task<Stream> DownloadInitFile(CancellationToken ct);

        Task UploadInitFile(Stream contents);
            
        Task<Stream> DownloadPatchFile(string id, CancellationToken ct);

        Task UploadPatchFile(string id, Stream contents);
    }
}