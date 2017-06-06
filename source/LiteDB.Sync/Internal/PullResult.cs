using System.Collections.Generic;
using System.Linq;

namespace LiteDB.Sync.Internal
{
    internal class PullResult
    {
        public PullResult(IList<Patch> patches, CloudState cloudState)
        {
            this.RemoteChanges = this.CombineChanges(patches);
            this.CloudState = cloudState;
        }

        public PullResult(CloudState cloudState)
        {
            this.CloudState = cloudState;
        }

        public PullResult()
        {
        }

        public Patch RemoteChanges { get; }

        public CloudState CloudState { get; }

        public bool CloudStateChanged => this.CloudState != null;

        public bool HasChanges => this.RemoteChanges != null && this.RemoteChanges.Any();

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