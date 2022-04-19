using OpenUGD.ECS.Components;

namespace OpenUGD.ECS.Entities
{
    public struct UnsafeDirectComponent<T> where T : struct, IComponent
    {
        public T[] Components;
        public bool[] Contains;
        public EntityId[] Ids;
        public int Count;
    }

    public unsafe struct UnsafePointerComponent
    {
        public void* Components;
        public bool[] Contains;
        public EntityId[] Ids;
        public int Count;
    }
}