namespace LiteDB.Sync.Internal {
    internal interface IFactory
    {
        ICloudClient CreateCloudClient(ILiteSyncCloudProvider provider);

        ILiteSynchronizer CreateSynchronizer(ILiteDatabase db, ILiteSyncConfiguration config, ICloudClient cloudClient);
    }
}