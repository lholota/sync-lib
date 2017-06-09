using System.Collections.Generic;
using System.Linq;

namespace LiteDB.Sync.Internal
{
    internal class PullResult
    {
        public PullResult(IList<Patch> patches, CloudState cloudState)
        {
            this.RemotePatch = this.CombineChanges(patches);
            this.CloudState = cloudState;
        }

        public PullResult(CloudState cloudState)
        {
            this.CloudState = cloudState;
            this.RemotePatch = new Patch();
        }

        public PullResult()
        {
            this.RemotePatch = new Patch();
        }

        public Patch RemotePatch { get; }

        public CloudState CloudState { get; }

        public bool CloudStateChanged => this.CloudState != null;

        public bool HasChanges => this.RemotePatch != null && this.RemotePatch.Changes.Any();

        private Patch CombineChanges(IList<Patch> patches)
        {
            if (patches == null || patches.Count == 0)
            {
                return new Patch();
            }

            return Patch.Combine(patches);
        }
    }
}