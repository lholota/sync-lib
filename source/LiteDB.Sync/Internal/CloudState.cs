namespace LiteDB.Sync.Internal
{
    internal class CloudState
    {
        public CloudState()
        {
        }

        public CloudState(string nextPatchId)
        {
            this.NextPatchId = nextPatchId;
        }

        public string NextPatchId { get; set; }
    }
}