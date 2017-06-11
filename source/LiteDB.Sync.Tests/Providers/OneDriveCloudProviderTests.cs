using System.Threading.Tasks;
using LiteDb.Sync.OneDrive;

namespace LiteDB.Sync.Tests.Providers
{
    public class OneDriveCloudProviderTests : CloudProviderTestsBase<OneDriveCloudProvider>
    {
        // TBA: Test token refresh
        // TBA: Failing auth

        protected override OneDriveCloudProvider CreateCloudProvider(TestSecrets secrets)
        {
            return new OneDriveCloudProvider(secrets.OneDriveClientId);
        }

        protected override async Task ClearAppFolder()
        {
            await this.Provider.ClearAppDirectory();
        }
    }
}