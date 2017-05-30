namespace LiteDB.Sync.Internal
{
    internal class SequenceCounter
    {
        private int currentSequenceNumber;

        public int CurrentSequenceNumber { get; set; }

        public int Next()
        {
            return this.currentSequenceNumber++;
        }
    }
}