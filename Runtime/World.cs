using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OpenUGD.ECS.Entities;

namespace OpenUGD.ECS
{
    public abstract class World : SubWorld.WorldBase, IEnumerable<SubWorld>
    {
        private struct WorldWrapper
        {
            public short SubWorldId;
            public SubWorld SubWorld;
        }

        private readonly WorldPool _pool;
        private readonly Dictionary<Type, WorldWrapper> _subWorldsMap;
        private readonly SharedEntityComponents _sharedEntityComponents;
        private readonly SubWorld[] _subWorlds;
        private int _id;

        protected World(
            int entitySubWorldCapacity = Constants.EntitySubWorldCapacity,
            int sharedComponentsBufferCapacity = Constants.SharedComponentsBufferCapacity,
            int sharedComponentsCapacity = Constants.SharedComponentsCapacity
        )
        {
            if (entitySubWorldCapacity <= 0)
            {
                throw new ArgumentException("Entity sub world capacity must be greater than 0");
            }

            _subWorldsMap = new Dictionary<Type, WorldWrapper>();
            _subWorlds = new SubWorld[entitySubWorldCapacity];
            _pool = new WorldPool();
            _sharedEntityComponents =
                new SharedEntityComponents(sharedComponentsBufferCapacity, sharedComponentsCapacity);
        }

        public SharedEntityComponents Shared => _sharedEntityComponents;

        public WorldPool Pool => _pool;

        public EntityId CreateEntity<T>() where T : SubWorld
        {
            var wrapper = GetSubWorldInternal<T>();

            var entity = CreateEntity(GenerateNextId(), wrapper.SubWorldId, wrapper.SubWorld);
            return entity;
        }

        public void DeleteEntity(EntityId id)
        {
            var subWorld = GetSubWorldByIdInternal(id);
#if DEBUG && !ENABLE_PROFILER
            ECS.Utilities.Contract.True(subWorld.Contains(id));
#endif
            DeleteEntity(id, subWorld);
        }

        public T GetSubWorld<T>() where T : SubWorld
        {
            return (T)_subWorldsMap[typeof(T)].SubWorld;
        }

        public int GetSubWorldId<T>()
        {
            return _subWorldsMap[typeof(T)].SubWorldId;
        }

        public SubWorld GetSubWorld(EntityId entityId)
        {
            return GetSubWorldByIdInternal(entityId);
        }

        public bool Contains(EntityId entityId)
        {
            var subWorld = GetSubWorldByIdInternal(entityId);
            return subWorld.Contains(entityId);
        }

        public SubWorld[] SubWorldUnsafe => _subWorlds;
        public int SubWorldCount => _subWorldsMap.Count;

        protected TSubWorld AddSubWorld<TSubWorld>(TSubWorld subWorld) where TSubWorld : SubWorld
        {
            var wrapper = new WorldWrapper {
                SubWorldId = (short)_subWorldsMap.Count,
                SubWorld = subWorld
            };
            _subWorldsMap.Add(typeof(TSubWorld), wrapper);

            if (_subWorlds.Length <= wrapper.SubWorldId)
            {
                throw new ArgumentException("Entity sub world capacity exceeded");
            }
            _subWorlds[wrapper.SubWorldId] = subWorld;
            return subWorld;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private WorldWrapper GetSubWorldInternal<T>() where T : SubWorld
        {
            return _subWorldsMap[typeof(T)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GenerateNextId()
        {
            return ++_id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private SubWorld GetSubWorldByIdInternal(EntityId id)
        {
            return _subWorlds[id.SubWorldId];
        }

        IEnumerator<SubWorld> IEnumerable<SubWorld>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public Enumerator GetEnumerator() => new Enumerator(this);

        public struct Enumerator : IEnumerator<SubWorld>
        {
            private readonly int _total;
            private World _world;
            private int _index;

            public Enumerator(World world)
            {
                _world = world;
                _total = world._subWorldsMap.Count;
                _index = -1;
            }

            public bool MoveNext()
            {
                if (_total != _world._subWorldsMap.Count)
                {
                    throw new InvalidOperationException("subWorlds were changed");
                }

                while (++_index < _total)
                {
                    if (_world._subWorlds[_index] != null)
                    {
                        return true;
                    }
                }

                return false;
            }

            public void Reset() => _index = -1;
            object IEnumerator.Current => Current;
            public SubWorld Current => _world._subWorlds[_index];
            public void Dispose() => _world = null;
        }
    }
}
