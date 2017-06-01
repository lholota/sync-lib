namespace LiteDB.Sync.Internal
{
    internal class Factory : IFactory
    {
        public ICloudClient CreateCloudClient(ILiteSyncCloudProvider provider)
        {
            return new CloudClient(provider);
        }
    }
}