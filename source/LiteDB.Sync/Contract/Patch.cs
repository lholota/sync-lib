using System.Collections.Generic;
using System.Linq;
using LiteDB.Sync.Internal;

namespace LiteDB.Sync.Contract
{
    public class Patch
    {
        public static Patch Combine(IList<Patch> patches)
        {
            var operations = new Dictionary<BsonValue, EntityOperation>();

            foreach (var patch in patches)
            {
                foreach (var operation in patch.operations)
                {
                    operations[operation.MergeId] = operation;
                }
            }

            var opList = operations.Select(x => x.Value).ToList();
            return new Patch(opList);
        }

        private List<EntityOperation> operations;

        public Patch()
        {
            this.operations = new List<EntityOperation>();
        }

        private Patch(List<EntityOperation> operations)
        {
            this.operations = operations;
        }

        public IEnumerable<EntityOperation> Operations
        {
            get => this.operations;
            set => this.operations = value.ToList();
        }

        internal void AddChanges(string collectionName, IEnumerable<BsonDocument> dirtyEntities)
        {
            var changeOperations = dirtyEntities.Select(x => new EntityOperation
            {
                OperationType = EntityOperationType.Upsert,
                CollectionName = collectionName,
                EntityId = x["_id"],
                Entity = x
            });

            this.operations.AddRange(changeOperations);
        }

        internal void AddDeletes(IEnumerable<DeletedEntity> deletedEntities)
        {
            var changeOperations = deletedEntities.Select(x => new EntityOperation
            {
                OperationType = EntityOperationType.Delete,
                CollectionName = x.CollectionName,
                EntityId = x.EntityId
            });

            this.operations.AddRange(changeOperations);
        }
    }
}