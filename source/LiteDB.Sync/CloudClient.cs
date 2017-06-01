using System;
using System.Threading.Tasks;
using LiteDB.Sync.Contract;

namespace LiteDB.Sync
{
    internal class CloudClient : ICloudClient
    {
        private readonly ILiteSyncCloudProvider provider;
        private readonly IContractSerializer serializer;

        public CloudClient(ILiteSyncCloudProvider provider, IContractSerializer serializer)
        {
            this.serializer = serializer;
            this.provider = provider;
        }

        public Task<object> Pull(Head localHead)
        {
            throw new NotImplementedException();
        }

        public Task Push(Patch patch)
        {
            throw new NotImplementedException();
        }
    }

    internal interface ICloudClient
    {
        
    }
}
