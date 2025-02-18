// © 2025 OpenUGD

using System;

namespace OpenUGD.ECS.Utilities
{
    /// <summary>
    /// Based on a modified version of Knuth's subtractive random number generator algorithm
    /// </summary>
    public unsafe struct UnsafeRandom
    {
        private fixed int _seedArray[56];
        private int _inext;
        private int _inextp;
        private bool _initialized;
        private int _seed;

        public UnsafeRandom(int seed)
        {
            _seed = seed;
            _initialized = false;
            _inext = 0;
            _inextp = 0;
        }

        public int Seed => _seed;

        public void InitWith(int seed)
        {
            _seed = seed;
            _initialized = false;
        }

        public byte[] Serialize()
        {
            byte[] data = new byte[56 * sizeof(int) + 3 * sizeof(int) + sizeof(bool)];
            fixed (byte* ptr = data)
            {
                int* intPtr = (int*)ptr;
                for (int i = 0; i < 56; i++)
                {
                    intPtr[i] = _seedArray[i];
                }

                intPtr[56] = _inext;
                intPtr[57] = _inextp;
                intPtr[58] = _seed;
                *(bool*)(ptr + 59 * sizeof(int)) = _initialized;
            }

            return data;
        }

        public void Deserialize(byte[] data)
        {
            if (data.Length != 56 * sizeof(int) + 3 * sizeof(int) + sizeof(bool))
            {
                throw new ArgumentException("Invalid data length for UnsafeRandom deserialization.");
            }

            fixed (byte* ptr = data)
            {
                int* intPtr = (int*)ptr;
                for (int i = 0; i < 56; i++)
                {
                    _seedArray[i] = intPtr[i];
                }

                _inext = intPtr[56];
                _inextp = intPtr[57];
                _seed = intPtr[58];
                _initialized = *(bool*)(ptr + 59 * sizeof(int));
            }
        }

        public double NextDouble()
        {
            return Sample();
        }

        public float NextFloat()
        {
            while (true)
            {
                float f = (float)Sample();
                if (f < 1.0f) // reject 1.0f, which is rare but possible due to rounding
                {
                    return f;
                }
            }
        }

        public int Next() => InternalSample();

        public int Next(int maxValue)
        {
            unchecked
            {
                return (int)(Sample() * maxValue);
            }
        }

        public int Next(int minValue, int maxValue)
        {
            unchecked
            {
                long range = (long)maxValue - minValue;
                return range <= int.MaxValue
                    ? (int)(Sample() * range) + minValue
                    : (int)((long)(GetSampleForLargeRange() * range) + minValue);
            }
        }

        public long NextInt64()
        {
            while (true)
            {
                ulong result = NextUInt64() >> 1;
                if (result != long.MaxValue)
                {
                    return (long)result;
                }
            }
        }

        public ulong NextUInt64() =>
            ((ulong)(uint)Next(1 << 22)) |
            (((ulong)(uint)Next(1 << 22)) << 22) |
            (((ulong)(uint)Next(1 << 20)) << 44);

        public void NextBytes(Span<byte> buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)InternalSample();
            }
        }

        private double Sample()
        {
            return InternalSample() * (1.0 / int.MaxValue);
        }

        private void Initialize(int seed)
        {
            int subtraction = (seed == int.MinValue) ? int.MaxValue : Math.Abs(seed);
            int mj = 161803398 - subtraction; // magic number based on Phi (golden ratio)
            _seedArray[55] = mj;
            int mk = 1;

            int ii = 0;
            for (int i = 1; i < 55; i++)
            {
                // The range [1..55] is special (Knuth) and so we're wasting the 0'th position.
                if ((ii += 21) >= 55)
                {
                    ii -= 55;
                }

                _seedArray[ii] = mk;
                mk = mj - mk;
                if (mk < 0)
                {
                    mk += int.MaxValue;
                }

                mj = _seedArray[ii];
            }

            for (int k = 1; k < 5; k++)
            {
                for (int i = 1; i < 56; i++)
                {
                    int n = i + 30;
                    if (n >= 55)
                    {
                        n -= 55;
                    }

                    _seedArray[i] -= _seedArray[1 + n];
                    if (_seedArray[i] < 0)
                    {
                        _seedArray[i] += int.MaxValue;
                    }
                }
            }

            _inext = 0;
            _inextp = 21;
        }

        private int InternalSample()
        {
            if (!_initialized)
            {
                _initialized = true;
                Initialize(_seed);
            }

            int next = _inext;
            if (++next >= 56)
            {
                next = 1;
            }

            int nextp = _inextp;
            if (++nextp >= 56)
            {
                nextp = 1;
            }

            int result = _seedArray[next] - _seedArray[nextp];

            if (result == int.MaxValue)
            {
                result--;
            }

            if (result < 0)
            {
                result += int.MaxValue;
            }

            _seedArray[next] = result;
            _inext = next;
            _inextp = nextp;

            return result;
        }

        private double GetSampleForLargeRange()
        {
            // The distribution of the double returned by Sample is not good enough for a large range.
            // If we use Sample for a range [int.MinValue..int.MaxValue), we will end up getting even numbers only.
            int result = InternalSample();

            // We can't use addition here: the distribution will be bad if we do that.
            if (InternalSample() % 2 == 0) // decide the sign based on second sample
            {
                result = -result;
            }

            double d = result;
            d += int.MaxValue - 1; // get a number in range [0..2*int.MaxValue-1)
            d /= 2u * int.MaxValue - 1;
            return d;
        }
    }
}
