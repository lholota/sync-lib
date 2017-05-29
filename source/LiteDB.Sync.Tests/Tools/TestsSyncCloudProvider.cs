using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiteDB.Sync.Contract;

namespace LiteDB.Sync.Tests.Tools
{
    public class TestsSyncCloudProvider : ILiteSyncCloudProvider
    {
        public TestsSyncCloudProvider(DeviceContext deviceContext)
        {
            throw new NotImplementedException();
        }

        public Task<IList<Patch>> Pull(Guid latestTransactionId)
        {
            throw new NotImplementedException();
        }

        public Task Push(Patch patch)
        {
            throw new NotImplementedException();
        }
    }
}