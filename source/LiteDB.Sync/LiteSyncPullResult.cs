using System.Collections.Generic;
using LiteDB.Sync.Contract;

namespace LiteDB.Sync
{
    public class LiteSyncPullResult
    {
        public LiteSyncPullResult(IList<Patch> patches, object etag = null)
        {
            this.RemoteChanges = this.CombineChanges(patches);
            this.Etag = etag;
        }

        public object Etag { get; }

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