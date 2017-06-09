namespace LiteDB.Sync.Internal
{
    internal class Factory : IFactory
    {
        public ICloudClient CreateCloudClient(ILiteSyncCloudProvider provider)
        {
            return new CloudClient(provider);
        }

        public ISynchronizer CreateSynchronizer(ILiteSyncDatabase db, ILiteSyncConfiguration config, ICloudClient cloudClient)
        {
            return new Synchronizer(db, config, cloudClient);
        }
    }
}