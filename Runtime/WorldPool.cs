using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenUGD.ECS.Entities;
using OpenUGD.ECS.Utilities;

namespace OpenUGD.ECS
{
    public class WorldPool
    {
        private readonly List<EntityList> _entityRefs = new();
        private readonly Stack<EntityList> _entities = new();
        private readonly Dictionary<Type, Stack<IList>> _listPool = new();
        private readonly Dictionary<Type, Stack<IList>> _rawListPool = new();
        private readonly Stack<HashSet<int>> _intHashSet = new();

        public HashSet<int> PopIntHashSet()
        {
            if (_intHashSet.Count == 0)
            {
                return new HashSet<int>();
            }

            return _intHashSet.Pop();
        }

        public void Return(HashSet<int> hashSet)
        {
            hashSet.Clear();
            _intHashSet.Push(hashSet);
        }

        public EntityList PopEntityIds(object? context = null, int capacity = Constants.DefaultEntityListPoolCapacity)
        {
            EntityList result;
            if (_entities.Count != 0)
            {
                result = _entities.Pop();
            }
            else
            {
                result = new EntityListImpl(this, capacity);
#if DEBUG
                if (_entityRefs.Count >= 20)
                {
                    var refs = string.Join(Environment.NewLine, _entityRefs.Select(e =>
                        e.Context?.ToString() ?? "---"
                    ));
                    throw new InvalidOperationException(
                        $"MEMORY LEAK: {nameof(WorldPool)}.{nameof(PopEntityIds)}{Environment.NewLine}refs:{refs}");
                }
#endif
            }

            result.Context = context;
#if DEBUG
            _entityRefs.Add(result);
#endif
            return result;
        }

        public void Return(EntityList list)
        {
            if (_entities.Contains(list))
            {
                ThrowHelper.ThrowValueExistException();
            }

            list.Clear();
            _entities.Push(list);
#if DEBUG
            _entityRefs.Remove(list);
#endif
        }

        public List<T> PopList<T>(int capacity = 0)
        {
            Stack<IList>? stack;
            if (_listPool.TryGetValue(typeof(T), out stack) && stack.Count != 0)
            {
                var result = ((List<T>)stack.Pop());
                if (result.Capacity < capacity)
                {
                    result.Capacity = capacity;
                }

                return result;
            }

            return new List<T>(capacity);
        }

        public void Return<T>(List<T> value)
        {
            value.Clear();
            Stack<IList>? stack;
            if (!_listPool.TryGetValue(typeof(T), out stack))
            {
                _listPool[typeof(T)] = stack = new Stack<IList>();
            }

            if (stack.Contains(value)) ThrowHelper.ThrowValueExistException();
            stack.Push(value);
        }

        public RawList<T> PopRawList<T>(int capacity)
        {
            Stack<IList>? stack;
            if (_rawListPool.TryGetValue(typeof(T), out stack) && stack.Count != 0)
            {
                var result = ((RawList<T>)stack.Pop());
                if (result.Capacity < capacity)
                {
                    result.Capacity = capacity;
                }

                return result;
            }

            return new RawList<T>(capacity);
        }

        public void Return<T>(RawList<T> value)
        {
            value.Clear();
            Stack<IList>? stack;
            if (!_rawListPool.TryGetValue(typeof(T), out stack))
            {
                _rawListPool[typeof(T)] = stack = new Stack<IList>();
            }

            if (stack.Contains(value)) ThrowHelper.ThrowValueExistException();
            stack.Push(value);
        }

        private class EntityListImpl : EntityList
        {
            public EntityListImpl(WorldPool pool, int capacity) : base(pool, capacity)
            {
            }
        }
    }
}