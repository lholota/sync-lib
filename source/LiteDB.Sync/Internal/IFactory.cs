namespace LiteDB.Sync.Internal {
    internal interface IFactory
    {
        ICloudClient CreateCloudClient(ILiteSyncCloudProvider provider);
    }
}