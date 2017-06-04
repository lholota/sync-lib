using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LiteDB.Sync;
using LiteDB.Sync.Internal;

namespace LiteDb.Sync.GoogleDrive
{
    public class GoogleDriveCloudProvider : ILiteSyncCloudProvider
    {
        /*
         * Has local state -> knows what file id is next -> can check if it exists directly
         * Doesn't have local state 
         *      -> doesn't know if it's connecting to existing repository or to a new one
         */


        public Task<CloudState> CreateCloudState()
        {
            /*
             * Check if LiteSync.init file exists
             *  -> exists -> download and set the next 
             */
        }


        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public Task<HeadDownloadResult> DownloadHeadFile(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task UploadHeadFile(Stream contents, string etag, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> DownloadPatch(Guid id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task UploadPatch(Stream contents, Guid id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}