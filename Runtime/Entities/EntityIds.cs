using System.Collections;
using System.Collections.Generic;

namespace OpenUGD.ECS.Entities
{
    public struct EntityIds : IEnumerable<EntityId>
    {
        private readonly SubWorld.EntitiesMap _map;

        public EntityIds(SubWorld.EntitiesMap map) => _map = map;

        public SubWorld.EntitiesMap.EntityIdEnumerator GetEnumerator() => _map.GetEntitiesId();

        IEnumerator<EntityId> IEnumerable<EntityId>.GetEnumerator() => this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}