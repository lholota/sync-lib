using System.Collections;
using System.Collections.Generic;

namespace LiteDB.Sync.Contract
{
    /*
     * Overwrite duplicates when merging patches -> HashSet
     * Find conflicts when comparing patches -> Dictionary based lookup
     * List changes for applying them to the local repository -> ReadOnlyList
     */

    /*
     * Conflict detection and resolution -> how will the resolve actions be applied?
     */

    public class EntityChangeCollection : IEnumerable<EntityChange>
    {
        private readonly Dictionary<GlobalEntityId, EntityChange> changes;

        public EntityChangeCollection()
        {
            this.changes = new Dictionary<GlobalEntityId, EntityChange>();
        }

        public IEnumerator<EntityChange> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}