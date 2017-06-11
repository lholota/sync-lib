using System.Threading.Tasks;
using LiteDB.Sync.FileShareProvider;
using NUnit.Framework;

namespace LiteDB.Sync.Tests.Providers
{
    [TestFixture]
    public class FileShareCloudProviderTests : CloudProviderTestsBase<FileShareCloudProvider>
    {
        protected override FileShareCloudProvider CreateCloudProvider(TestSecrets secrets)
        {
            return new FileShareCloudProvider();
        }

        protected override Task ClearAppFolder()
        {
            this.Provider.Cleanup();

            return Task.FromResult(0);
        }
    }
}