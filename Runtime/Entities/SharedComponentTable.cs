using System;
using System.Runtime.InteropServices;
using OpenUGD.ECS.Components;
using OpenUGD.ECS.Utilities;

namespace OpenUGD.ECS.Entities
{
    public unsafe class SharedComponentTable
    {
        private readonly Type[] _componentTypes;
        private readonly uint[] _offsets;
        private readonly uint[] _size;
        private readonly byte[] _buffer;
        private readonly bool[] _contains;
        private uint _lastOffset;
        private int _count;

        public SharedComponentTable()
        {
            _buffer = new byte[Constants.SharedComponentsBufferCapacity];
            _componentTypes = new Type[Constants.SharedComponentsCapacity];
            _offsets = new uint[Constants.SharedComponentsCapacity];
            _size = new uint[Constants.SharedComponentsCapacity];
            _contains = new bool[Constants.SharedComponentsCapacity];
        }

        public SharedComponentTable(int sharedComponentsBufferCapacity, int sharedComponentsCapacity)
        {
            _buffer = new byte[sharedComponentsBufferCapacity];
            _componentTypes = new Type[sharedComponentsCapacity];
            _offsets = new uint[sharedComponentsCapacity];
            _size = new uint[sharedComponentsCapacity];
            _contains = new bool[sharedComponentsCapacity];
        }

        public int Count => _count;

        public IComponent[] Components
        {
            get
            {
                var array = new IComponent[_count];
                var index = 0;
                for (int i = 0; i < _count; i++)
                {
                    if (_contains[i])
                    {
                        var value = (IComponent)Activator.CreateInstance(_componentTypes[i]);
                        fixed (byte* iterator = _buffer)
                        {
                            void* pointer = iterator + _offsets[i];
                            Marshal.PtrToStructure((IntPtr)pointer, value);
                        }

                        array[index] = value;
                        index++;
                    }
                }

                if (array.Length < index)
                {
                    Array.Resize(ref array, index);
                }

                return array;
            }
        }

        public void RegisterComponent<T>() where T : struct, IComponent
        {
            var index = GetComponentIndex<T>();
            Contract.True(index == -1);
            var size = Unsafe.SizeOf<T>();
            Contract.True(size >= 0);
            var nextIndex = _count;
            _componentTypes[nextIndex] = typeof(T);
            _lastOffset = Unsafe.AlignUInt(_lastOffset);
            _offsets[nextIndex] = _lastOffset;
            _size[nextIndex] = (uint)size;
            _lastOffset += (uint)size;
            _count++;
        }

        public void Delete<T>()
        {
            var index = GetComponentIndex<T>();
            Contract.True(index != -1);
            _contains[index] = false;
        }

        public bool Contains<T>()
        {
            var index = GetComponentIndex<T>();
            Contract.True(index != -1);
            return _contains[index];
        }

        public T GetComponent<T>() where T : struct, IComponent
        {
            var index = GetComponentIndex<T>();
            Contract.True(index != -1);

            if (!_contains[index]) return default;

            var offset = _offsets[index];
            T component;
            fixed (byte* iterator = _buffer)
            {
                void* pointer = iterator + offset;
                component = Unsafe.As<T>(pointer);
            }

            return component;
        }

        public ref T GetComponentRef<T>() where T : struct, IComponent
        {
            var index = GetComponentIndex<T>();
            Contract.True(index != -1);

            if (!_contains[index])
            {
                throw new InvalidOperationException($"type: {typeof(T)} did not register");
            }

            var offset = _offsets[index];
            fixed (byte* iterator = _buffer)
            {
                void* pointer = iterator + offset;
                return ref Unsafe.AsRef<T>(pointer);
            }
        }

        public UnsafePointer GetUnsafeComponent<T>() where T : struct, IComponent
        {
            var index = GetComponentIndex<T>();
            Contract.True(index != -1);
            if (!_contains[index])
            {
                return new UnsafePointer();
            }

            var handler = Unsafe.AllocPinned(_buffer);
            var offset = _offsets[index];
            var size = _size[index];
            void* address = null;
            fixed (byte* buffer = _buffer)
            {
                address = buffer + offset;
            }

            var pointer = new UnsafePointer(handler, address, size);

            return pointer;
        }

        public void SetComponent<T>(T value) where T : struct, IComponent
        {
            var index = GetComponentIndex<T>();
            Contract.True(index != -1);

            var offset = _offsets[index];
            var size = _size[index];
            _contains[index] = true;

            fixed (byte* iterator = _buffer)
            {
                void* bufferPointer = iterator + offset;
                void* valuePointer = Unsafe.AddressOf(ref value);
                Buffer.MemoryCopy(valuePointer, bufferPointer, size, size);
            }
        }

        private int GetComponentIndex<T>()
        {
            for (var i = 0; i < _count; i++)
            {
                Type componentType = _componentTypes[i];
                if (componentType == typeof(T))
                {
                    return i;
                }
            }

            return -1;
        }

        public struct UnsafePointer
        {
            private GCHandle _gcHandle;

            public UnsafePointer(GCHandle gcHandle, void* address, uint size)
            {
                _gcHandle = gcHandle;
                Address = address;
                Size = size;
            }

            public void* Address { get; private set; }
            public uint Size { get; }
            public bool IsZero => Address == null;

            public void Free()
            {
                if (Address != null)
                {
                    _gcHandle.Free();
                    Address = null;
                }
            }
        }
    }
}