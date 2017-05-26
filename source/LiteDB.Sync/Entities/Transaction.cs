namespace LiteDB.Sync.Entities
{
    using System.Collections.Generic;

    internal class Transaction
    {
        // TBA: Metadata - local date/time, device id etc?

        public IList<EntityOperation> Operations { get; set; }
    }
}