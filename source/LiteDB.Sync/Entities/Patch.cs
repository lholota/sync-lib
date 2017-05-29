using System;
using System.Collections.Generic;
using LiteDB.Sync.Internal;

namespace LiteDB.Sync.Entities
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

        internal void AddDeletes(IEnumerable<DeletedEntity> deletes)
        {

        }
    }
}