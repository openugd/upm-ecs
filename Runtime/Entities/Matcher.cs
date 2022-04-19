using System.Collections.Generic;
using OpenUGD.ECS.Components;

namespace OpenUGD.ECS.Entities
{
    public class Matcher
    {
        public static void Fill<T>(T filter, TypeDescriptor typeDescriptor, SubWorld.EntityFilter entityFilter)
            where T : class
        {
            var tables = entityFilter.Tables;
            var descriptorsLength = typeDescriptor.Descriptors.Length;
            var descriptors = typeDescriptor.Descriptors;
            for (var ti = 0; ti < descriptorsLength; ti++)
            {
                var descriptor = descriptors[ti];
                var array = tables[descriptor.ComponentIndex].GetComponents();
                descriptor.SetFieldValue(filter, array);
            }
        }

        public static void GetEntities(EntityList result, TypeDescriptor typeDescriptor,
            SubWorld.EntityFilter entityFilter, List<IIsMatch>? filters = null)
        {
            var hasMatch = filters != null && filters.Count != 0;
            var tables = entityFilter.Tables;

            var includeTypesLength = typeDescriptor.IncludeTypes.Length;
            var excludeTypesLength = typeDescriptor.ExcludeTypes.Length;
            var includeTypes = typeDescriptor.IncludeTypes;
            var excludeTypes = typeDescriptor.ExcludeTypes;

            var entityIterator = 0;
            var entityLastIndex = entityFilter.EntityLastIndex;
            var entitiesItems = entityFilter.EntitiesCount;
            var hasEntities = entityFilter.HasEntities;
            var entityIds = entityFilter.Entities;

            for (int i = 0; i <= entityLastIndex && entityIterator < entitiesItems; ++i)
            {
                if (!hasEntities[i]) continue;
                ++entityIterator;
                var entityId = entityIds[i];
                var entityIndex = entityId.Index;
                var ok = true;

                for (var ii = 0; ii < includeTypesLength; ++ii)
                {
                    var includeTypeIndex = includeTypes[ii];
                    var array = tables[includeTypeIndex].GetContains();
                    if (array.Length > entityIndex && !array[entityIndex])
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok)
                {
                    for (var ie = 0; ie < excludeTypesLength; ++ie)
                    {
                        var excludeTypeIndex = excludeTypes[ie];
                        var array = tables[excludeTypeIndex].GetContains();
                        if (array.Length > entityIndex && tables[excludeTypeIndex].GetContains()[entityIndex])
                        {
                            ok = false;
                            break;
                        }
                    }

                    if (ok)
                    {
                        if (hasMatch)
                        {
                            foreach (var filter in filters!)
                            {
                                if (!filter.IsMatch(i))
                                {
                                    ok = false;
                                    break;
                                }
                            }
                        }

                        if (ok)
                        {
                            result.Add(entityId);
                        }
                    }
                }
            }
        }
    }

    public interface IIsMatch
    {
        bool IsMatch(int index);
    }

    public class Matcher<TFilter> where TFilter : class, new()
    {
        public delegate bool Match(TFilter filter, int index);

        private readonly TypeDescriptor _typeDescriptor;
        private readonly SubWorld.EntityFilter _entityFilter;
        private readonly TFilter _filter;

        private List<IIsMatch>? _filters;

        public Matcher(TFilter filter, TypeDescriptor typeDescriptor, SubWorld.EntityFilter entityFilter)
        {
            _filter = filter;
            _typeDescriptor = typeDescriptor;
            _entityFilter = entityFilter;
        }

        public TFilter Fill()
        {
            Matcher.Fill(_filter, _typeDescriptor, _entityFilter);
            return _filter;
        }

        public (TFilter, EntityList) GetEntities(object? context = null)
        {
            var entities = _entityFilter.SubWorld.World.Pool.PopEntityIds(context);
            
            Matcher.Fill(_filter, _typeDescriptor, _entityFilter);
            Matcher.GetEntities(entities, _typeDescriptor, _entityFilter, _filters);

            return (_filter, entities);
        }

        public TFilter GetEntities(EntityList result)
        {
            Matcher.Fill(_filter, _typeDescriptor, _entityFilter);
            Matcher.GetEntities(result, _typeDescriptor, _entityFilter, _filters);

            return _filter;
        }

        public Matcher<TFilter> AddFilter<TComponent>(MatcherFilter<TComponent>.FilterDelegate filter)
            where TComponent : struct, IComponent
        {
            if (_filters == null)
            {
                _filters = new List<IIsMatch>();
            }

            _filters.Add(new MatcherFilter<TComponent>(this, filter));
            return this;
        }

        public Matcher<TFilter> AddFilter<TComponent0, TComponent1>(
            MatcherFilter<TComponent0, TComponent1>.FilterDelegate filter)
            where TComponent0 : struct, IComponent
            where TComponent1 : struct, IComponent
        {
            if (_filters == null)
            {
                _filters = new List<IIsMatch>();
            }

            _filters.Add(new MatcherFilter<TComponent0, TComponent1>(this, filter));
            return this;
        }

        public class MatcherFilter : IIsMatch
        {
            private readonly Matcher<TFilter> _matcher;
            private readonly FilterDelegate _filter;

            public delegate bool FilterDelegate(TFilter filter, int index);

            public MatcherFilter(Matcher<TFilter> matcher, FilterDelegate filter)
            {
                _matcher = matcher;
                _filter = filter;
            }

            public bool IsMatch(int index)
            {
                return _filter(_matcher._filter, index);
            }
        }

        public class MatcherFilter<TComponent> : IIsMatch
            where TComponent : struct, IComponent
        {
            private readonly Matcher<TFilter> _matcher;
            private readonly FilterDelegate _filter;
            private readonly int _typeIndex;

            public delegate bool FilterDelegate(ref TComponent component);

            public MatcherFilter(Matcher<TFilter> matcher, FilterDelegate filter)
            {
                _matcher = matcher;
                _typeIndex = _matcher._entityFilter.GetTypeIndex<TComponent>();
                _filter = filter;
            }

            public bool IsMatch(int index)
            {
                var table = _matcher._entityFilter.Tables[_typeIndex];
                var array = (TComponent[])table.GetComponents();
                return _filter(ref array[index]);
            }
        }

        public class MatcherFilter<TComponent0, TComponent1> : IIsMatch
            where TComponent0 : struct, IComponent
            where TComponent1 : struct, IComponent
        {
            private readonly Matcher<TFilter> _matcher;
            private readonly FilterDelegate _filter;
            private readonly int _typeIndex0;
            private readonly int _typeIndex1;

            public delegate bool FilterDelegate(ref TComponent0 component0, ref TComponent1 component1);

            public MatcherFilter(Matcher<TFilter> matcher, FilterDelegate filter)
            {
                _matcher = matcher;
                _typeIndex0 = _matcher._entityFilter.GetTypeIndex<TComponent0>();
                _typeIndex1 = _matcher._entityFilter.GetTypeIndex<TComponent1>();
                _filter = filter;
            }

            public bool IsMatch(int index)
            {
                var table0 = _matcher._entityFilter.Tables[_typeIndex0];
                var table1 = _matcher._entityFilter.Tables[_typeIndex1];
                var components0 = (TComponent0[])table0.GetComponents();
                var components1 = (TComponent1[])table1.GetComponents();
                return _filter(ref components0[index], ref components1[index]);
            }
        }
    }
}