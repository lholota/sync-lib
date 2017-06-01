using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LiteDB.Sync
{
    public interface ILiteSyncCloudProvider
    {
        void Initialize();

        Task<HeadDownloadResult> DownloadHeadFile(CancellationToken ct);

        Task UploadHeadFile(Stream contents, string etag, CancellationToken ct);

        Task<Stream> DownloadPatch(Guid id, CancellationToken ct);

        Task UploadPatch(Stream contents, Guid id, CancellationToken ct);
    }

    public class HeadDownloadResult : IDisposable
    {
        public HeadDownloadResult(Stream content, string etag)
        {
            this.Content = content;
            this.Etag = etag;
        }

        public Stream Content { get; }

        public string Etag { get; }

        public void Dispose()
        {
            this.Content.Dispose();
        }
    }
}