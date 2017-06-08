using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LiteDB.Sync.Exceptions;
using Newtonsoft.Json;

namespace LiteDB.Sync.Internal
{
    internal class CloudClient : ICloudClient
    {
        private readonly Newtonsoft.Json.JsonSerializer serializer;
        private readonly ILiteSyncCloudProvider provider;

        public CloudClient(ILiteSyncCloudProvider provider)
        {
            this.provider = provider;
            this.serializer = JsonSerialization.CreateSerializer();
        }

        public async Task<PullResult> Pull(CloudState originalState, CancellationToken ct)
        {
            var localState = originalState ?? await this.GetLocalCloudState(ct);

            var patches = await this.DownloadPatches(localState.NextPatchId, ct);

            if (patches.Count > 0)
            {
                var nextPatchId = patches[patches.Count - 1].NextPatchId;
                var newCloudState = new CloudState(nextPatchId);

                return new PullResult(patches, newCloudState);
            }

            if (originalState == null)
            {
                // Initial cloud state was downloaded
                return new PullResult(localState);
            }

            return new PullResult();
        }

        public async Task Push(CloudState localCloudState, Patch patch, CancellationToken ct)
        {
            var nextPatchId = await this.GeneratePatchId(ct);

            patch.NextPatchId = nextPatchId; // TODO: Wrap the Patch

            using (var ms = new MemoryStream())
            using(var writer = new StreamWriter(ms))
            {
                this.serializer.Serialize(writer, patch);

                await writer.FlushAsync();
                ms.Position = 0;

                await this.provider.UploadPatchFile(localCloudState.NextPatchId, ms);
            }

            localCloudState.NextPatchId = nextPatchId;
        }

        private async Task<CloudState> GetLocalCloudState(CancellationToken ct)
        {
            var existingFileStream = await this.provider.DownloadInitFile(ct);

            if (existingFileStream == null)
            {
                var firstPatch = await this.GeneratePatchId(ct);
                var cloudState = new CloudState(firstPatch);

                using (var ms = new MemoryStream())
                using (var writer = new StreamWriter(ms))
                {
                    this.serializer.Serialize(writer, cloudState);
                    await writer.FlushAsync();

                    ms.Position = 0;

                    try
                    {
                        await this.provider.UploadInitFile(ms);

                        return cloudState;
                    }
                    catch (LiteSyncConflictOccuredException)
                    {
                        // Init file already exists, retry read
                        existingFileStream = await this.provider.DownloadInitFile(ct);
                    }
                }
            }

            using (existingFileStream)
            using (var reader = new StreamReader(existingFileStream))
            using (var jsonReader = new JsonTextReader(reader))
            {
                return this.serializer.Deserialize<CloudState>(jsonReader);
            }
        }

        private async Task<IList<Patch>> DownloadPatches(string nextPatchId, CancellationToken ct)
        {
            var patches = new List<Patch>();

            while (true)
            {
                var currentPatchStream = await this.provider.DownloadPatchFile(nextPatchId, ct);

                if (currentPatchStream == null)
                {
                    break;
                }

                using (currentPatchStream)
                using (var strReader = new StreamReader(currentPatchStream))
                using (var jsonReader = new JsonTextReader(strReader))
                {
                    var patch = this.serializer.Deserialize<Patch>(jsonReader);

                    patches.Add(patch);
                    nextPatchId = patch.NextPatchId;
                }
            }

            return patches;
        }

        private async Task<string> GeneratePatchId(CancellationToken ct)
        {
            var generator = this.provider as ILiteSyncPatchIdGenerator;
            if (generator != null)
            {
                return await generator.GeneratePatchId(ct);
            }

            return Guid.NewGuid().ToString("N");
        }
    }
}
