using System;
using OpenUGD.ECS.Components;

namespace OpenUGD.ECS.Entities
{
    public struct EntityFilter<T> where T : struct, IComponent
    {
        public delegate void ForEachAction(
            EntityId entityId,
            ref T component
        );

        public T[] Components;

        private readonly TypeDescriptor _typeDescriptor;
        private readonly SubWorld.EntityFilter _entityFilter;
        private readonly WorldPool _pool;

        public EntityFilter(TypeDescriptor typeDescriptor, SubWorld.EntityFilter entityFilter,
            WorldPool pool)
        {
            _typeDescriptor = typeDescriptor;
            _entityFilter = entityFilter;
            _pool = pool;
            Components = (T[])_entityFilter.EntityComponents[_typeDescriptor.IncludeTypes[0]].GetComponents();
        }

        public void ForEach(ForEachAction action)
        {
            object? context = null;
#if DEBUG
            // ReSharper disable once HeapView.BoxingAllocation
            context = this;
#endif
            var entities = _pool.PopEntityIds(context);
            Matcher.GetEntities(entities, _typeDescriptor, _entityFilter);

            foreach (var entityId in entities)
            {
                action(entityId, ref Components[entityId.Index]);
            }

            _pool.Return(entities);
        }

        public EntityList GetEntities(object? context = null)
        {
#if DEBUG
            // ReSharper disable once HeapView.BoxingAllocation
            context = context ?? this;
#endif
            var entities = _pool.PopEntityIds(context);
            Matcher.GetEntities(entities, _typeDescriptor, _entityFilter);
            return entities;
        }

        public void GetEntities(EntityList entities)
        {
            Matcher.GetEntities(entities, _typeDescriptor, _entityFilter);
        }
    }

    public struct EntityFilter<T0, T1> where T0 : struct, IComponent where T1 : struct, IComponent
    {
        public delegate void ForEachAction(
            EntityId entityId,
            ref T0 component0,
            ref T1 component1
        );

        public T0[] Components0;
        public T1[] Components1;

        private readonly TypeDescriptor _typeDescriptor;
        private readonly SubWorld.EntityFilter _entityFilter;
        private readonly WorldPool _pool;

        public EntityFilter(TypeDescriptor typeDescriptor, SubWorld.EntityFilter entityFilter,
            WorldPool pool) : this()
        {
            _typeDescriptor = typeDescriptor;
            _entityFilter = entityFilter;
            _pool = pool;

            Components0 = (T0[])_entityFilter.EntityComponents[_typeDescriptor.IncludeTypes[0]].GetComponents();
            Components1 = (T1[])_entityFilter.EntityComponents[_typeDescriptor.IncludeTypes[1]].GetComponents();
        }

        public void ForEach(ForEachAction action)
        {
            object? context = null;
#if DEBUG
            // ReSharper disable once HeapView.BoxingAllocation
            context = this;
#endif
            var entities = _pool.PopEntityIds(context);
            Matcher.GetEntities(entities, _typeDescriptor, _entityFilter);

            foreach (var entityId in entities)
            {
                var index = entityId.Index;
                action(entityId, ref Components0[index], ref Components1[index]);
            }

            _pool.Return(entities);
        }

        public EntityList GetEntities(object? context = null)
        {
#if DEBUG
            // ReSharper disable once HeapView.BoxingAllocation
            context = context ?? this;
#endif
            var entities = _pool.PopEntityIds(context);
            Matcher.GetEntities(entities, _typeDescriptor, _entityFilter);
            return entities;
        }

        public void GetEntities(EntityList entities)
        {
            Matcher.GetEntities(entities, _typeDescriptor, _entityFilter);
        }
    }

    public struct EntityFilter<T0, T1, T2> where T0 : struct, IComponent
        where T1 : struct, IComponent
        where T2 : struct, IComponent
    {
        public delegate void ForEachAction(
            EntityId entityId,
            ref T0 component0,
            ref T1 component1,
            ref T2 component2
        );

        public T0[] Components0;
        public T1[] Components1;
        public T2[] Components2;

        private readonly TypeDescriptor _typeDescriptor;
        private readonly SubWorld.EntityFilter _entityFilter;
        private readonly WorldPool _pool;

        public EntityFilter(TypeDescriptor typeDescriptor, SubWorld.EntityFilter entityFilter,
            WorldPool pool) : this()
        {
            _typeDescriptor = typeDescriptor;
            _entityFilter = entityFilter;
            _pool = pool;

            Components0 = (T0[])_entityFilter.EntityComponents[_typeDescriptor.IncludeTypes[0]].GetComponents();
            Components1 = (T1[])_entityFilter.EntityComponents[_typeDescriptor.IncludeTypes[1]].GetComponents();
            Components2 = (T2[])_entityFilter.EntityComponents[_typeDescriptor.IncludeTypes[2]].GetComponents();
        }

        public void ForEach(ForEachAction action)
        {
            object? context = null;
#if DEBUG
            // ReSharper disable once HeapView.BoxingAllocation
            context = this;
#endif
            var entities = _pool.PopEntityIds(context);
            Matcher.GetEntities(entities, _typeDescriptor, _entityFilter);

            foreach (var entityId in entities)
            {
                var index = entityId.Index;
                action(entityId, ref Components0[index], ref Components1[index], ref Components2[index]);
            }

            _pool.Return(entities);
        }

        public EntityList GetEntities(object? context = null)
        {
#if DEBUG
            // ReSharper disable once HeapView.BoxingAllocation
            context = context ?? this;
#endif
            var entities = _pool.PopEntityIds(context);
            Matcher.GetEntities(entities, _typeDescriptor, _entityFilter);
            return entities;
        }

        public void GetEntities(EntityList entities)
        {
            Matcher.GetEntities(entities, _typeDescriptor, _entityFilter);
        }
    }

    public struct EntityFilter<T0, T1, T2, T3> where T0 : struct, IComponent
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
    {
        public delegate void ForEachAction(
            EntityId entityId,
            ref T0 component0,
            ref T1 component1,
            ref T2 component2,
            ref T3 component3
        );

        public T0[] Components0;
        public T1[] Components1;
        public T2[] Components2;
        public T3[] Components3;

        private readonly TypeDescriptor _typeDescriptor;
        private readonly SubWorld.EntityFilter _entityFilter;
        private readonly WorldPool _pool;

        public EntityFilter(TypeDescriptor typeDescriptor, SubWorld.EntityFilter entityFilter,
            WorldPool pool) : this()
        {
            _typeDescriptor = typeDescriptor;
            _entityFilter = entityFilter;
            _pool = pool;

            Components0 = (T0[])_entityFilter.EntityComponents[_typeDescriptor.IncludeTypes[0]].GetComponents();
            Components1 = (T1[])_entityFilter.EntityComponents[_typeDescriptor.IncludeTypes[1]].GetComponents();
            Components2 = (T2[])_entityFilter.EntityComponents[_typeDescriptor.IncludeTypes[2]].GetComponents();
            Components3 = (T3[])_entityFilter.EntityComponents[_typeDescriptor.IncludeTypes[3]].GetComponents();
        }

        public void ForEach(ForEachAction action)
        {
            object? context = null;
#if DEBUG
            // ReSharper disable once HeapView.BoxingAllocation
            context = this;
#endif
            var entities = _pool.PopEntityIds(context);
            Matcher.GetEntities(entities, _typeDescriptor, _entityFilter);

            foreach (var entityId in entities)
            {
                var index = entityId.Index;
                action(entityId, ref Components0[index], ref Components1[index], ref Components2[index],
                    ref Components3[index]);
            }

            _pool.Return(entities);
        }

        public EntityList GetEntities(object? context = null)
        {
#if DEBUG
            // ReSharper disable once HeapView.BoxingAllocation
            context = context ?? this;
#endif
            var entities = _pool.PopEntityIds(context);
            Matcher.GetEntities(entities, _typeDescriptor, _entityFilter);
            return entities;
        }

        public void GetEntities(EntityList entities)
        {
            Matcher.GetEntities(entities, _typeDescriptor, _entityFilter);
        }
    }

    public struct EntityFilter<T0, T1, T2, T3, T4> where T0 : struct, IComponent
        where T1 : struct, IComponent
        where T2 : struct, IComponent
        where T3 : struct, IComponent
        where T4 : struct, IComponent
    {
        public delegate void ForEachAction(
            EntityId entityId,
            ref T0 component0,
            ref T1 component1,
            ref T2 component2,
            ref T3 component3,
            ref T4 component4
        );

        public T0[] Components0;
        public T1[] Components1;
        public T2[] Components2;
        public T3[] Components3;
        public T4[] Components4;

        private readonly TypeDescriptor _typeDescriptor;
        private readonly SubWorld.EntityFilter _entityFilter;
        private readonly WorldPool _pool;

        public EntityFilter(TypeDescriptor typeDescriptor, SubWorld.EntityFilter entityFilter,
            WorldPool pool) : this()
        {
            _typeDescriptor = typeDescriptor;
            _entityFilter = entityFilter;
            _pool = pool;

            Components0 = (T0[])_entityFilter.EntityComponents[_typeDescriptor.IncludeTypes[0]].GetComponents();
            Components1 = (T1[])_entityFilter.EntityComponents[_typeDescriptor.IncludeTypes[1]].GetComponents();
            Components2 = (T2[])_entityFilter.EntityComponents[_typeDescriptor.IncludeTypes[2]].GetComponents();
            Components3 = (T3[])_entityFilter.EntityComponents[_typeDescriptor.IncludeTypes[3]].GetComponents();
            Components4 = (T4[])_entityFilter.EntityComponents[_typeDescriptor.IncludeTypes[4]].GetComponents();
        }

        public void ForEach(ForEachAction action)
        {
            object? context = null;
#if DEBUG
            // ReSharper disable once HeapView.BoxingAllocation
            context = this;
#endif
            var entities = _pool.PopEntityIds(context);
            Matcher.GetEntities(entities, _typeDescriptor, _entityFilter);

            foreach (var entityId in entities)
            {
                var index = entityId.Index;
                action(entityId, ref Components0[index], ref Components1[index], ref Components2[index],
                    ref Components3[index],
                    ref Components4[index]);
            }

            _pool.Return(entities);
        }

        public EntityList GetEntities(object? context = null)
        {
#if DEBUG
            // ReSharper disable once HeapView.BoxingAllocation
            context = context ?? this;
#endif
            var entities = _pool.PopEntityIds(context);
            Matcher.GetEntities(entities, _typeDescriptor, _entityFilter);
            return entities;
        }

        public void GetEntities(EntityList entities)
        {
            Matcher.GetEntities(entities, _typeDescriptor, _entityFilter);
        }
    }
}