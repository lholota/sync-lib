namespace LiteDB.Sync.Entities
{
    using System.Collections.Generic;

    public class Transaction
    {
        // TBA: Metadata - local date/time, device id etc?

        public IList<EntityOperation> Operations { get; set; }
    }
}