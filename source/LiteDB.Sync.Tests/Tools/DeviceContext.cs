namespace LiteDB.Sync.Tests.Tools
{
    using System.IO;

    public class DeviceContext
    {
        public DeviceContext()
        {
            this.Provider = new TestsSyncProvider(this);
            this.Controller = new LiteDbSyncController(this.Provider, TODO);
            
            this.Stream = new MemoryStream();
        }

        public TestsSyncProvider Provider { get; }

        public LiteDbSyncController Controller { get; }

        public Stream Stream { get; }

        public LiteSyncDatabase CreateLiteDatabase()
        {
            return new LiteSyncDatabase(this.Controller, Stream);
        }
    }
}