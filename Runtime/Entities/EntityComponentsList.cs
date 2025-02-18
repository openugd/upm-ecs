using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OpenUGD.ECS.Components;
using OpenUGD.ECS.Utilities;

namespace OpenUGD.ECS.Entities
{
    public abstract class EntityComponentsList<T> : IEntityComponents<T>, IEntityComponentsRawData where T : struct, IComponent
    {
        private T[] _components;
        private bool[] _contains;
        private int _capacity;
        private int _count;
        private int _version;
        private int _startIndex;
        private readonly int _typeIndex;
        private readonly SubWorld _subWorld;
        private readonly EntityComponentHook<T>? _hook;
        private readonly SubWorld.EntitiesMap _entitiesMap;

        protected EntityComponentsList(
            SubWorld subWorld,
            int typeIndex,
            SubWorld.EntitiesMap entitiesMap,
            EntityComponentHook<T>? hook,
            int initialCapacity
        )
        {
            _subWorld = subWorld;
            _typeIndex = typeIndex;
            _entitiesMap = entitiesMap;
            _hook = hook;
            _startIndex = int.MaxValue;
            _capacity = initialCapacity;
            _components = new T[initialCapacity];
            _contains = new bool[initialCapacity];
        }

        public SubWorld SubWorld {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _subWorld;
        }

        public int Capacity {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _capacity;
        }

        public int TypeIndex {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _typeIndex;
        }

        public T[] RawComponents {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _components;
        }

        public bool[] RawContains {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _contains;
        }

        public void GetEntities(EntityList entities)
        {
            var index = -1;
            var count = 0;
            var ids = _entitiesMap.EntityIds;
            while (true)
            {
                do
                {
                    ++index;
                    if (count == _count) return;
                } while (!_contains[index]);

                entities.Add(ids[index]);
                ++count;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Array GetComponents() => _components;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool[] GetContains() => _contains;

        public int Count {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(EntityId entity)
        {
            if (!_entitiesMap.Contains(entity)) return false;

            return _contains[entity.Index];
        }

        public void DeleteAll()
        {
            var index = -1;
            var ids = _entitiesMap.EntityIds;
            while (true)
            {
                do
                {
                    if (_count == 0)
                    {
                        return;
                    }

                    ++index;
                } while (!_contains[index]);

                Delete(ids[index]);
            }
        }

        public void Delete(EntityId entity)
        {
            Contract.True(_entitiesMap.Contains(entity));

            int index = entity.Index;
            if (_contains[index])
            {
                _contains[index] = false;
                if (_hook != null)
                {
                    _hook.BeforeDelete(ref entity, ref _components[index]);
                }

                _components[index] = default(T);
                --_count;
                ++_version;
                if (index == _startIndex)
                {
                    if (_count > 0) _startIndex++;
                    else
                    {
                        _startIndex = int.MaxValue;
                    }
                }

                if (_hook != null)
                {
                    _hook.AfterDelete(ref entity);
                }
            }
        }

        public T this[EntityId entityId] {
            get {
                int index = entityId.Index;

                string? errorMessage = null;
#if !ENABLE_PROFILER && DEBUG
                errorMessage = $"EntityId not found id:{entityId}, in:{_subWorld}";
#endif
                Contract.True(_entitiesMap.EntityIds[index].FullEquals(entityId), errorMessage);

                return _components[index];
            }
            set { Set(entityId, value); }
        }

        public ref T this[short index] => ref _components[index];

        public ref T GetRef(EntityId entityId)
        {
            int index = entityId.Index;

            string? errorMessage = null;
#if !ENABLE_PROFILER && DEBUG
            errorMessage = $"EntityId not found id:{entityId}, in:{_subWorld}";
#endif
            Contract.True(_entitiesMap.EntityIds[index].FullEquals(entityId), errorMessage);

            return ref _components[index];
        }

        public bool TryGet(EntityId entityId, out T component)
        {
            int index = entityId.Index;

            if (_contains[entityId.Index])
            {
                string? errorMessage = null;
#if !ENABLE_PROFILER && DEBUG
                errorMessage = $"EntityId not found id:{entityId}, in:{_subWorld}";
#endif
                Contract.True(_entitiesMap.EntityIds[index].FullEquals(entityId), errorMessage);

                component = _components[index];
                return true;
            }

            component = default(T);
            return false;
        }

        public void Set(EntityId entity, T component)
        {
            Contract.True(_entitiesMap.Contains(entity));

            var index = entity.Index;
            var isSet = false;
            if (!_contains[index])
            {
                ++_count;
                if (_hook != null)
                {
                    _hook.BeforeSet(ref entity, ref component);
                }

                isSet = true;
                if (_startIndex > index) _startIndex = index;
            }
            else
            {
                if (_hook != null)
                {
                    _hook.Replace(ref entity, ref _components[index], ref component);
                }
            }

            _components[index] = component;
            _contains[index] = true;
            ++_version;

            if (_hook != null)
            {
                if (isSet)
                {
                    _hook.AfterSet(ref entity, ref component);
                }
                else
                {
                    _hook.AfterReplace(ref entity, ref component);
                }
            }
        }

        public void ResizeTo(int newCapacity)
        {
            if (_capacity < newCapacity)
            {
                _capacity = newCapacity;
                Array.Resize(ref _components, newCapacity);
                Array.Resize(ref _contains, newCapacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // ReSharper disable once HeapView.BoxingAllocation
        public IComponent GetComponent(int index) => _components[index];

        private int GetNextSize(int v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<ComponentIndex<T>> GetEnumerator() => new Enumerator(this);

        public struct Enumerator : IEnumerator<ComponentIndex<T>>
        {
            private EntityComponentsList<T> _entityComponents;
            private EntityId[] _ids;
            private int _index;
            private int _componentsVersion;
            private int _entityVersion;
            private int _count;
            private ComponentIndex<T> _current;

            public Enumerator(EntityComponentsList<T> entityComponentsList)
            {
                _entityComponents = entityComponentsList;
                _ids = entityComponentsList._entitiesMap.EntityIds;
                _index = entityComponentsList._startIndex;
                _count = 0;
                _componentsVersion = entityComponentsList._version;
                _entityVersion = entityComponentsList._entitiesMap.Version;
                _current = default(ComponentIndex<T>);
            }

            public bool MoveNext()
            {
                if (_componentsVersion != _entityComponents._version) throw new InvalidOperationException();
                if (_entityVersion != _entityComponents._entitiesMap.Version) throw new InvalidOperationException();
                if (_count == _entityComponents._count) return false;
                if (_index >= _ids.Length) return false;

                while (!_entityComponents._contains[_index])
                {
                    ++_index;
                }

                ++_count;
                _current = new ComponentIndex<T> {
                    Id = _ids[_index],
                    Component = _entityComponents._components[_index]
                };
                ++_index;
                return true;
            }

            public void Reset()
            {
                _index = _entityComponents._startIndex;
                _count = 0;
            }

            public ComponentIndex<T> Current {
                get {
                    if (_componentsVersion != _entityComponents._version) throw new InvalidOperationException();
                    return _current;
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose() => _entityComponents = null;
        }
    }
}
