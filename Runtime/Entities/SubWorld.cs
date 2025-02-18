using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OpenUGD.ECS.Components;
using OpenUGD.ECS.Utilities;

namespace OpenUGD.ECS.Entities
{
    public abstract class SubWorld : IEnumerable<EntityId>
    {
        private readonly Dictionary<Type, TypeDescriptor> _descriptorCache;
        private readonly Stack<short> _poolIndex;
        private readonly Dictionary<Type, int> _typeIndex;
        private readonly EntitiesMap _entities;
        private readonly SharedEntityComponents _sharedEntityComponents;
        private IEntityComponentsRawData?[] _entityComponents;
        private int _typeComponentsIndex;
        private short _entityIndex;

        // ReSharper disable once MemberCanBePrivate.Global
        public World World { get; }

        protected SubWorld(
            World world,
            int startEntitiesCapacity = Constants.StartEntitiesCapacity,
            int sharedComponentsBufferCapacity = Constants.SharedComponentsBufferCapacity,
            int sharedComponentsCapacity = Constants.SharedComponentsCapacity,
            int startComponentsCapacity = Constants.StartComponentsCapacity
        )
        {
            if (startEntitiesCapacity <= 0)
                throw new ArgumentException($"{nameof(startEntitiesCapacity)} capacity must be greater than 0");

            World = world;
            _entityIndex = 0;
            _typeComponentsIndex = 0;
            _entityComponents = new IEntityComponentsRawData[startComponentsCapacity];
            _descriptorCache = new Dictionary<Type, TypeDescriptor>();
            _poolIndex = new Stack<short>(startEntitiesCapacity);
            _typeIndex = new Dictionary<Type, int>();
            _entities = new EntitiesMap(this, startEntitiesCapacity);
            _sharedEntityComponents = new SharedEntityComponents(sharedComponentsBufferCapacity, sharedComponentsCapacity);
        }

        public SharedEntityComponents Shared => _sharedEntityComponents;

        public bool Contains(EntityId entityId) => _entities.Contains(entityId);

        protected IEntityComponents<TComponent> AddComponent<TComponent>(EntityComponentHook<TComponent>? hook = null)
            where TComponent : struct, IComponent
        {
            var entityComponents = new EntityComponentsListImpl<TComponent>(
                this,
                _typeComponentsIndex,
                _entities,
                hook,
                _entities.EntityIds.Length
            );
            _typeIndex.Add(typeof(TComponent), _typeComponentsIndex);

            if (_typeComponentsIndex == _entityComponents.Length)
            {
                Array.Resize(ref _entityComponents, _entityComponents.Length << 1);
            }

            _entityComponents[_typeComponentsIndex] = entityComponents;

            ++_typeComponentsIndex;

            return entityComponents;
        }

        public int Count => _entities.Count;

        public bool HasComponent<TComponent>(EntityId entityId) where TComponent : struct, IComponent
        {
            IEntityComponents entityComponents = _entityComponents[GetTypeIndex(typeof(TComponent))]!;
            return entityComponents.Contains(entityId);
        }

        public void GetComponents(EntityId entityId, List<IComponent> components)
        {
            foreach (var data in _entityComponents)
            {
                if (data == null) break;
                if (data.Contains(entityId))
                {
                    components.Add(data.GetComponent(entityId.Index));
                }
            }
        }

        public bool GetComponent<TComponent>(EntityId entityId, out TComponent component)
            where TComponent : struct, IComponent
        {
            IEntityComponents entityComponents = _entityComponents[GetTypeIndex(typeof(TComponent))]!;
            if (entityComponents.Contains(entityId))
            {
                component = ((IEntityComponents<TComponent>)entityComponents)[entityId];
                return true;
            }

            component = default(TComponent);
            return false;
        }

        public void SetComponent<TComponent>(EntityId entityId, TComponent component)
            where TComponent : struct, IComponent
        {
            var entityComponent = (IEntityComponents<TComponent>)_entityComponents[_typeIndex[typeof(TComponent)]]!;
            entityComponent.Set(entityId, component);
        }

        public void DeleteComponent<TComponent>(EntityId entityId) where TComponent : struct, IComponent
        {
            var entityComponent = (IEntityComponents<TComponent>)_entityComponents[_typeIndex[typeof(TComponent)]]!;
            entityComponent.Delete(entityId);
        }

        public UnsafeDirectComponent<T> UnsafeGetDirectComponents<T>() where T : struct, IComponent
        {
            var index = GetTypeIndex<T>();
            return UnsafeGetDirectComponents<T>(index);
        }

        public UnsafeDirectComponent<T> UnsafeGetDirectComponents<T>(int typeIndex) where T : struct, IComponent
        {
            var index = typeIndex;
            var entityComponentsRawData = _entityComponents[index];
            var result = new UnsafeDirectComponent<T> {
                Components = (T[])entityComponentsRawData!.GetComponents(),
                Contains = entityComponentsRawData.GetContains(),
                Ids = _entities.EntityIds,
                Count = entityComponentsRawData.Count
            };
            return result;
        }

        public int GetTypeIndex<TComponent>() => _typeIndex[typeof(TComponent)];

        public int GetTypeIndex(Type type) => _typeIndex[type];

        public EntityFilter<T> WhenAll<T>() where T : struct, IComponent
        {
            return new EntityFilter<T>(GetTypeDescriptor(typeof(EntityFilter<T>)), new EntityFilter {
                SubWorld = this
            }, World.Pool);
        }

        public EntityFilter<T0, T1> WhenAll<T0, T1>() where T0 : struct, IComponent where T1 : struct, IComponent
        {
            return new EntityFilter<T0, T1>(GetTypeDescriptor(typeof(EntityFilter<T0, T1>)), new EntityFilter {
                SubWorld = this
            }, World.Pool);
        }

        public EntityFilter<T0, T1, T2> WhenAll<T0, T1, T2>() where T0 : struct, IComponent
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            return new EntityFilter<T0, T1, T2>(GetTypeDescriptor(typeof(EntityFilter<T0, T1, T2>)), new EntityFilter {
                SubWorld = this
            }, World.Pool);
        }

        public EntityFilter<T0, T1, T2, T3> WhenAll<T0, T1, T2, T3>() where T0 : struct, IComponent
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            return new EntityFilter<T0, T1, T2, T3>(GetTypeDescriptor(typeof(EntityFilter<T0, T1, T2, T3>)),
                new EntityFilter {
                    SubWorld = this
                }, World.Pool);
        }

        public EntityFilter<T0, T1, T2, T3, T4> WhenAll<T0, T1, T2, T3, T4>() where T0 : struct, IComponent
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            return new EntityFilter<T0, T1, T2, T3, T4>(GetTypeDescriptor(typeof(EntityFilter<T0, T1, T2, T3, T4>)),
                new EntityFilter {
                    SubWorld = this
                }, World.Pool);
        }

        public Matcher<TFilter> CreateMatcher<TFilter>() where TFilter : class, new()
        {
            TypeDescriptor typeDescriptor = GetTypeDescriptor(typeof(TFilter));

            var filter = new TFilter();
            var matcher = new Matcher<TFilter>(filter, typeDescriptor, new EntityFilter {
                SubWorld = this
            });
            return matcher;
        }

        public EntitiesMap.EntityIdEnumerator GetEnumerator() => _entities.GetEntitiesId();

        // ReSharper disable once HeapView.BoxingAllocation
        IEnumerator<EntityId> IEnumerable<EntityId>.GetEnumerator() => _entities.GetEntities();

        // ReSharper disable once HeapView.BoxingAllocation
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private TypeDescriptor GetTypeDescriptor(Type type)
        {
            // ReSharper disable once InlineOutVariableDeclaration
            TypeDescriptor? typeDescriptor;
            if (!_descriptorCache.TryGetValue(type, out typeDescriptor))
            {
                var descriptorList = new List<FieldDescriptor>();
                var includeList = World.Pool.PopIntHashSet();
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField |
                                            BindingFlags.SetField);
                foreach (var field in fields)
                {
                    var fieldType = field.FieldType;
                    if (fieldType.IsArray)
                    {
                        var componentType = fieldType.GetElementType();
                        if (typeof(IComponent).IsAssignableFrom(componentType))
                        {
                            var componentIndex = _typeIndex[componentType];
                            includeList.Add(componentIndex);
                            var descriptor = new FieldDescriptor {
                                SetFieldValue = field.SetValue,
                                ComponentIndex = componentIndex
                            };
                            descriptorList.Add(descriptor);
                        }
                        else
                        {
                            Contract.Throw("does not support the type: " + componentType);
                        }
                    }
                }

                var excludeList = World.Pool.PopIntHashSet();
                var attributes = type.GetCustomAttributes(false);
                if (attributes.Length != 0)
                {
                    foreach (var attribute in attributes)
                    {
                        var excludeAttribute = attribute as ExcludeAttribute;
                        if (excludeAttribute != null)
                        {
                            excludeList.Add(_typeIndex[excludeAttribute.Type]);
                        }
                    }
                }

                _descriptorCache[type] = typeDescriptor = new TypeDescriptor {
                    Descriptors = descriptorList.ToArray(),
                    IncludeTypes = includeList.ToArray(),
                    ExcludeTypes = excludeList.ToArray()
                };

                World.Pool.Return(includeList);
                World.Pool.Return(excludeList);
            }

            return typeDescriptor;
        }

        private EntityId CreateEntityInternal(int id, short managerId)
        {
            var index = GetNextIndex();

            EntityId entityId = new EntityId(id, index, managerId);

            _entities.Add(entityId);

            var capacity = _entities.EntityIds.Length;
            foreach (var rawData in _entityComponents)
            {
                if (rawData == null) break;
                rawData.ResizeTo(capacity);
            }

            return entityId;
        }

        private void DeleteEntityInternal(EntityId id)
        {
            foreach (var rawData in _entityComponents)
            {
                if (rawData == null) break;
                rawData.Delete(id);
            }

            _entities.Remove(id);

            _poolIndex.Push(id.Index);
        }

        private short GetNextIndex()
        {
            var index = _entityIndex;
            if (_poolIndex.Count != 0)
            {
                index = _poolIndex.Pop();
            }
            else
            {
                ++_entityIndex;
            }

            return index;
        }

        public abstract class WorldBase
        {
            protected EntityId CreateEntity(int id, short managerId, SubWorld manager)
            {
                return manager.CreateEntityInternal(id, managerId);
            }

            protected void DeleteEntity(EntityId id, SubWorld manager)
            {
                manager.DeleteEntityInternal(id);
            }
        }

        public class EntitiesMap
        {
#if DEBUG
            private readonly object? _owner = null;
#endif

            // ReSharper disable once MemberCanBePrivate.Global
            public int EntitiesCount;
            public int LastIndex;

            // ReSharper disable once HeapView.ObjectAllocation.Evident
            public EntityId[] EntityIds;

            // ReSharper disable once HeapView.ObjectAllocation.Evident
            public bool[] HasEntities;
            public int Version;

            public EntitiesMap(object? owner, int startEntitiesCapacity)
            {
                EntityIds = new EntityId[startEntitiesCapacity];
                HasEntities = new bool[startEntitiesCapacity];
#if DEBUG
                _owner = owner;
#endif
            }

            public int Count {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => EntitiesCount;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Contains(EntityId id)
            {
                if (id.Id == 0) return false;

                var index = id.Index;
                if (index >= EntityIds.Length) return false;
                unsafe
                {
                    fixed (EntityId* exist = &EntityIds[index])
                    {
                        if (exist->FullEquals(id)) return true;
                        return false;
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Remove(EntityId id)
            {
                if (id.Id == 0) throw new ArgumentException("Entity id is not correct: " + id);
                if (id.Index >= EntityIds.Length) throw new ArgumentException("Entity is not found: " + id);

                int index;

                unsafe
                {
                    index = id.Index;

                    fixed (EntityId* exist = &EntityIds[index])
                    {
                        if (!exist->FullEquals(id)) index = -1;
                    }
                }

                if (index == -1) throw new ArgumentException("Entity id not found: " + id);

                EntityIds[index] = EntityId.Empty;
                HasEntities[index] = false;
                if (LastIndex == index)
                {
                    LastIndex = LastIndex - 1 < 0 ? 0 : LastIndex - 1;
                }

                --EntitiesCount;
                ++Version;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add(EntityId entity)
            {
                var index = entity.Index;

                if (index > LastIndex) LastIndex = index;
                if (index == EntityIds.Length)
                {
                    Array.Resize(ref EntityIds, EntityIds.Length << 1);
                    Array.Resize(ref HasEntities, EntityIds.Length);
                }

                HasEntities[index] = true;
                EntityIds[index] = entity;

                ++EntitiesCount;
                ++Version;
            }

            public Enumerator GetEnumerator() => new Enumerator(this);

            public Enumerator GetEntities() => new Enumerator(this);

            public EntityIdEnumerator GetEntitiesId() => new EntityIdEnumerator(this);

            public struct EntityIdEnumerator : IEnumerator<EntityId>
            {
                private readonly EntitiesMap _entities;
                private readonly int _version;
                private int _index;
                private int _count;

                public EntityIdEnumerator(EntitiesMap entities)
                {
                    _index = -1;
                    _count = 0;
                    _version = entities.Version;
                    _entities = entities;
                }

                public bool MoveNext()
                {
                    if (_version != _entities.Version) throw new InvalidOperationException();

                    do
                    {
                        ++_index;
                        if (_index > _entities.LastIndex || _count >= _entities.EntitiesCount) return false;
                    } while (!_entities.HasEntities[_index]);

                    ++_count;
                    return true;
                }

                public void Reset()
                {
                    _index = -1;
                    _count = 0;
                }

                public EntityId Current {
                    get {
                        if (_version != _entities.Version) throw new InvalidOperationException();
                        return _entities.EntityIds[_index];
                    }
                }

                // ReSharper disable once HeapView.BoxingAllocation
                object IEnumerator.Current => Current;

                public void Dispose()
                {
                }
            }

            public struct Enumerator : IEnumerator<EntityId>
            {
                private readonly EntitiesMap _entities;
                private readonly int _version;
                private int _index;
                private int _count;

                public Enumerator(EntitiesMap entities)
                {
                    _index = -1;
                    _count = 0;
                    _version = entities.Version;
                    _entities = entities;
                }

                public bool MoveNext()
                {
                    if (_version != _entities.Version) throw new InvalidOperationException();

                    do
                    {
                        ++_index;
                        if (_index > _entities.LastIndex || _count >= _entities.EntitiesCount) return false;
                    } while (!_entities.HasEntities[_index]);

                    ++_count;
                    return true;
                }

                public void Reset()
                {
                    _index = -1;
                    _count = 0;
                }

                public EntityId Current {
                    get {
                        if (_version != _entities.Version) throw new InvalidOperationException();
                        return _entities.EntityIds[_index];
                    }
                }

                // ReSharper disable once HeapView.BoxingAllocation
                object IEnumerator.Current => Current;

                public void Dispose()
                {
                }
            }
        }

        public struct EntityFilter
        {
            public SubWorld SubWorld;

            public bool[] HasEntities => SubWorld._entities.HasEntities;

            public EntityId[] Entities => SubWorld._entities.EntityIds;

            public int EntitiesCount => SubWorld._entities.Count;

            public int EntityLastIndex => SubWorld._entities.LastIndex;

            public IEntityComponentsRawData[] EntityComponents => SubWorld._entityComponents!;

            public readonly int GetTypeIndex<T>() where T : struct, IComponent => SubWorld.GetTypeIndex<T>();
        }

        private class EntityComponentsListImpl<T> : EntityComponentsList<T> where T : struct, IComponent
        {
            public EntityComponentsListImpl(
                SubWorld subWorld,
                int typeIndex,
                EntitiesMap entitiesMap,
                EntityComponentHook<T>? hook,
                int initialCapacity
            ) : base(
                subWorld,
                typeIndex,
                entitiesMap,
                hook,
                initialCapacity
            )
            {
            }
        }
    }
}
