namespace LiteDB.Sync.Internal
{
    internal class Factory : IFactory
    {
        public ICloudClient CreateCloudClient(ILiteSyncCloudProvider provider)
        {
            return new CloudClient(provider);
        }

        public ILiteSynchronizer CreateSynchronizer(ILiteDatabase db, ILiteSyncConfiguration config, ICloudClient cloudClient)
        {
            return new LiteSynchronizer(db, config, cloudClient);
        }
    }
}