using Moq;

namespace LiteDB.Sync.Tests.Tools
{
    public static class MockExtensions
    {
        public static LiteSyncConfiguration CreateMockedConfiguration(params string[] syncedCollections)
        {
            return new LiteSyncConfiguration(
                new Mock<ILiteSyncCloudProvider>().Object,
                new Mock<ILiteSyncConflictResolver>().Object,
                syncedCollections);
        }
    }
}