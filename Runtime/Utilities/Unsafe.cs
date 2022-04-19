using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OpenUGD.ECS.Utilities
{
    public static unsafe class Unsafe
    {
        public static bool IsPowerOfTwo(int value)
        {
            var mask = value - 1;
            return (mask & value) != 0;
        }

        public static uint AlignUInt(uint value)
        {
            if (value == 0) return 0;
            uint alignment = 4;
            uint mask = alignment - 1;
            uint offset = value & mask;
            uint result = value + (alignment - offset);
            return result;
        }

        public static ulong AlignULong(ulong value)
        {
            if (value == 0) return 0;
            var alignment = 4UL;
            var mask = alignment - 1;
            var offset = value & mask;
            var result = value + (alignment - offset);
            return result;
        }

        public static IntPtr Align<T>(IntPtr pointer) where T : struct
        {
            int size;
            if (typeof(T) == typeof(byte)) size = sizeof(byte);
            else if (typeof(T) == typeof(sbyte)) size = sizeof(sbyte);
            else if (typeof(T) == typeof(short)) size = sizeof(short);
            else if (typeof(T) == typeof(ushort)) size = sizeof(ushort);
            else if (typeof(T) == typeof(int)) size = sizeof(int);
            else if (typeof(T) == typeof(uint)) size = sizeof(uint);
            else if (typeof(T) == typeof(float)) size = sizeof(float);
            else if (typeof(T) == typeof(long)) size = sizeof(long);
            else if (typeof(T) == typeof(ulong)) size = sizeof(ulong);
            else if (typeof(T) == typeof(double)) size = sizeof(double);
            else throw new ArgumentException($"type: {typeof(T)} is not supported");

            if (pointer == IntPtr.Zero) return IntPtr.Zero;

            var alignment = size; //(sizeof(long) * 2) / size * sizeof(float);
            var mask = alignment - 1;
            var offset = ((ulong)pointer) & (ulong)mask;
            var result = pointer.ToInt64() + (uint)(alignment - (uint)offset);
            return (IntPtr)result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GCHandle AllocPinned(object value)
        {
            var handler = GCHandle.Alloc(value, GCHandleType.Pinned);
            return handler;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GCHandle AllocPinned<T>(T value) where T : class
        {
            var handler = GCHandle.Alloc(value, GCHandleType.Pinned);
            return handler;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T As<T>(IntPtr source) where T : struct
        {
#if UNITY_2019_2_OR_NEWER
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.CopyPtrToStructure(source.ToPointer(), out T result);
            return result;
#else
      return System.Runtime.CompilerServices.Unsafe.Read<T>(source.ToPointer());
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T As<T>(void* source) where T : struct
        {
#if UNITY_2019_2_OR_NEWER
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.CopyPtrToStructure(source, out T result);
            return result;
#else
      return System.Runtime.CompilerServices.Unsafe.Read<T>(source);
#endif
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AsRef<T>(void* source) where T : struct
        {
#if UNITY_2020_3_OR_NEWER
            return ref Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AsRef<T>(source);
#else
            return ref System.Runtime.CompilerServices.Unsafe.AsRef<T>(source);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SizeOf<T>() where T : struct
        {
#if UNITY_2019_2_OR_NEWER
            return Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<T>();
#else
      return System.Runtime.CompilerServices.Unsafe.SizeOf<T>();
#endif
        }

        public static void* AddressOf<T>(ref T output) where T : struct
        {
#if UNITY_2019_2_OR_NEWER
            return Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AddressOf<T>(ref output);
#else
      return System.Runtime.CompilerServices.Unsafe.AsPointer(ref output);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MemSet(byte* buffer, byte value, int size)
        {
            for (int i = 0; i < size; i++)
            {
                *(buffer + i) = value;
            }
        }
    }
}