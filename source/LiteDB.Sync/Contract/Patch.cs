using System;
using System.Collections.Generic;

namespace LiteDB.Sync.Contract
{
    public class Patch
    {
        public static Patch Combine(IList<Patch> patches)
        {
            throw new NotImplementedException();
        }

        public IList<EntityOperation> Operations { get; set; }

        internal void AddChanges(string collectionName, IEnumerable<BsonDocument> dirtyEntities)
        {
            
        }
    }
}