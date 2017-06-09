namespace LiteDB.Sync.Internal
{
    internal interface IFactory
    {
        ICloudClient CreateCloudClient(ILiteSyncCloudProvider provider);

        ISynchronizer CreateSynchronizer(ILiteSyncDatabase db, ILiteSyncConfiguration config, ICloudClient cloudClient);
    }
}