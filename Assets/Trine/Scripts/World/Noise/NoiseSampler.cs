using UnityEngine;

namespace Trine.World.Noise
{
    public static class NoiseSampler
    {
        // Unity Perlin is deterministic per platform обычно, но для полной стабильности позже заменишь на FastNoiseLite.
        private static float Perlin(float x, float z) => Mathf.PerlinNoise(x, z);

        public static float Fractal(NoiseLayerSO layer, int worldSeed, float x, float z)
        {
            if (layer == null) return 0f;

            int seed = worldSeed + layer.seedOffset;
            float fx = x, fz = z;

            if (layer.domainWarp)
            {
                float wx = Perlin((x + seed * 13) * layer.warpFrequency, (z - seed * 7) * layer.warpFrequency);
                float wz = Perlin((x - seed * 11) * layer.warpFrequency, (z + seed * 5) * layer.warpFrequency);
                fx += (wx * 2f - 1f) * layer.warpAmount;
                fz += (wz * 2f - 1f) * layer.warpAmount;
            }

            float amp = 1f;
            float freq = layer.frequency;
            float sum = 0f;
            float norm = 0f;

            for (int o = 0; o < layer.octaves; o++)
            {
                float n = Perlin((fx + seed * 0.01f) * freq, (fz - seed * 0.01f) * freq);
                if (layer.ridged) n = 1f - Mathf.Abs(n * 2f - 1f);

                sum += n * amp;
                norm += amp;

                amp *= layer.persistence;
                freq *= layer.lacunarity;
            }

            float v = (norm > 0f) ? (sum / norm) : 0f; // 0..1
            return v * layer.amplitude;
        }

        public static float Saturate(float v) => Mathf.Clamp01(v);
    }
}
