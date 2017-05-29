using System;

namespace LiteDB.Sync.Tests.Tools
{
    using System.IO;

    public class DeviceContext
    {
        public DeviceContext()
        {
            this.CloudProvider = new TestsSyncCloudProvider(this);
            // this.Service = new LiteSyncService(this.CloudProvider, null);
            
            this.Stream = new MemoryStream();
        }

        public TestsSyncCloudProvider CloudProvider { get; }

        public LiteSyncService Service { get; }

        public Stream Stream { get; }

        public LiteSyncDatabase CreateLiteDatabase()
        {
            throw new NotImplementedException();
            // return new LiteSyncDatabase(Stream);
        }
    }
}