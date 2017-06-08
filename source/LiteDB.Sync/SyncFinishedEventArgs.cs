using System;

namespace LiteDB.Sync
{
    public class SyncFinishedEventArgs : EventArgs
    {
        public bool Successful => this.Error == null;

        public Exception Error { get; set; }

        // TBA: Add stats - no of entity changes pulled and pushed, number of patches downloaded
    }
}
