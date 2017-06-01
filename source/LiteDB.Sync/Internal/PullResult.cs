using System.Collections.Generic;

namespace LiteDB.Sync.Internal
{
    internal class PullResult
    {
        public PullResult(IList<Patch> patches, string etag = null)
        {
            this.RemoteChanges = this.CombineChanges(patches);
            this.Etag = etag;
        }

        public string Etag { get; }

        public Patch RemoteChanges { get; }

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