﻿namespace LiteDB.Sync.Tests.Tools
{
    using System;
    using System.Threading.Tasks;

    public class TestsSyncCloudProvider : ILiteSyncCloudProvider
    {
        public TestsSyncCloudProvider(DeviceContext deviceContext)
        {
            throw new NotImplementedException();
        }

        public Task<object> Pull()
        {
            throw new NotImplementedException();
        }

        public Task Push(object args)
        {
            throw new NotImplementedException();
        }
    }
}