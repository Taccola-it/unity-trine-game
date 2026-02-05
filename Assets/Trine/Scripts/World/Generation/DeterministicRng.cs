using System;
using UnityEngine;

namespace Trine.World.Generation
{
    // Ѕыстрый детерминированный RNG (xorshift)
    public struct DeterministicRng
    {
        private uint _state;

        public DeterministicRng(int seed)
        {
            _state = (uint)seed;
            if (_state == 0) _state = 0x6D2B79F5u;
        }

        public uint NextU()
        {
            uint x = _state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _state = x;
            return x;
        }

        public float Next01()
        {
            // 24-битна€ мантисса
            return (NextU() & 0x00FFFFFFu) / 16777216f;
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive) return minInclusive;
            uint span = (uint)(maxExclusive - minInclusive);
            return minInclusive + (int)(NextU() % span);
        }

        public static int Hash(params int[] v)
        {
            unchecked
            {
                uint h = 2166136261u;
                for (int i = 0; i < v.Length; i++)
                {
                    h ^= (uint)v[i];
                    h *= 16777619u;
                }
                return (int)h;
            }
        }
    }
}
