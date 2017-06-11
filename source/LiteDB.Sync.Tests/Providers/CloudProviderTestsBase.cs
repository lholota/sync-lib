using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LiteDB.Sync.Exceptions;
using NUnit.Framework;

namespace LiteDB.Sync.Tests.Providers
{
    [TestFixture]
    public abstract class CloudProviderTestsBase<TProvider> where TProvider : ILiteSyncCloudProvider
    {
        private const string FileContent = "Hello, World!";

        protected TProvider Provider;

        [SetUp]
        public void Setup()
        {
            var secrets = TestSecrets.LoadFromFile();

            this.Provider = this.CreateCloudProvider(secrets);
            this.ClearAppFolder().Wait();
        }

        [Test]
        public async Task ShouldThrowIfInitFileIsUploadedTwice()
        {
            await this.Provider.UploadInitFile(this.CreateFileContent());
            Assert.ThrowsAsync<LiteSyncConflictOccuredException>(async () => await this.Provider.UploadInitFile(this.CreateFileContent()));
        }

        [Test]
        public async Task ShouldReturnNullOnDownloadIfInitFileDoesNotExist()
        {
            var initFile = await this.Provider.DownloadInitFile(CancellationToken.None);
            Assert.IsNull(initFile);
        }

        [Test]
        public async Task ShouldUploadAndDownloadInitFile()
        {
            await this.Provider.UploadInitFile(this.CreateFileContent());

            var contentStream = await this.Provider.DownloadInitFile(CancellationToken.None);
            Assert.IsNotNull(contentStream);

            var actualContent = this.ReadContent(contentStream);
            Assert.AreEqual(FileContent, actualContent);
        }

        [Test]
        public async Task ShouldThrowIfPatchIsUploadedTwice()
        {
            const string patchId = "SomePatchId";

            await this.Provider.UploadPatchFile(patchId, this.CreateFileContent());
            Assert.ThrowsAsync<LiteSyncConflictOccuredException>(async () => await this.Provider.UploadPatchFile(patchId, this.CreateFileContent()));
        }

        [Test]
        public async Task ShouldReturnNullOnDownloadIfPatchFileDoesNotExist()
        {
            var initFile = await this.Provider.DownloadPatchFile("SomePatch", CancellationToken.None);
            Assert.IsNull(initFile);
        }

        [Test]
        public async Task ShouldUploadAndDownloadPatchFile()
        {
            const string patchId = "SomePatchId";

            await this.Provider.UploadPatchFile(patchId, this.CreateFileContent());

            var contentStream = await this.Provider.DownloadPatchFile(patchId, CancellationToken.None);
            Assert.IsNotNull(contentStream);

            var actualContent = this.ReadContent(contentStream);
            Assert.AreEqual(FileContent, actualContent);
        }

        protected abstract TProvider CreateCloudProvider(TestSecrets secrets);

        protected abstract Task ClearAppFolder();

        protected Stream CreateFileContent()
        {
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);

            writer.Write(FileContent);
            writer.Flush();

            ms.Position = 0;

            return ms;
        }

        protected string ReadContent(Stream stream)
        {
            using (stream)
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}