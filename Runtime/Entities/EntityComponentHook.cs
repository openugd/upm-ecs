using OpenUGD.ECS.Components;

namespace OpenUGD.ECS.Entities
{
    public abstract class EntityComponentHook<TComponent> where TComponent : struct, IComponent
    {
        public abstract void BeforeDelete(ref EntityId id, ref TComponent component);
        public abstract void AfterDelete(ref EntityId entity);

        public abstract void Replace(ref EntityId id, ref TComponent lastComponent, ref TComponent newComponent);
        public abstract void AfterReplace(ref EntityId entity, ref TComponent component);

        public abstract void BeforeSet(ref EntityId id, ref TComponent component);
        public abstract void AfterSet(ref EntityId entity, ref TComponent component);
    }
}