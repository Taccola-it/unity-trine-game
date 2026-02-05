using System.Collections.Generic;
using UnityEngine;

namespace Trine.World.Biome
{
    public readonly struct BiomeSample
    {
        public readonly BiomeConfigSO primary;
        public readonly float blend; // 0..1, для границ (упрощенно)

        public BiomeSample(BiomeConfigSO primary, float blend)
        {
            this.primary = primary;
            this.blend = blend;
        }
    }

    public class BiomeResolver
    {
        private readonly List<BiomeConfigSO> _biomes;

        public BiomeResolver(List<BiomeConfigSO> biomesSortedByPriority)
        {
            _biomes = biomesSortedByPriority;
        }

        public BiomeSample Resolve(float height01, float temp01, float moist01, float slope01, float dist01, float patchNoise01)
        {
            BiomeConfigSO best = null;
            int bestPriority = int.MinValue;

            for (int i = 0; i < _biomes.Count; i++)
            {
                var b = _biomes[i];
                if (!In(b.height01, height01)) continue;
                if (!In(b.temp01, temp01)) continue;
                if (!In(b.moist01, moist01)) continue;
                if (!In(b.slope01, slope01)) continue;
                if (!In(b.dist01, dist01)) continue;

                // patchNoise может сдвигать шанс в пользу некоторых биомов через доп. правила (расширишь без изменения ядра)
                if (b.priority >= bestPriority)
                {
                    best = b;
                    bestPriority = b.priority;
                }
            }

            if (best == null && _biomes.Count > 0) best = _biomes[0];

            // Blend на границе: простая схема по близости к краю диапазона (можно улучшать)
            float blend = 0f;
            if (best != null)
            {
                blend = EdgeBlend(best, height01, temp01, moist01, slope01, dist01);
            }

            return new BiomeSample(best, blend);
        }

        private static bool In(Vector2 r, float v) => v >= r.x && v <= r.y;

        private static float EdgeBlend(BiomeConfigSO b, float h, float t, float m, float s, float d)
        {
            // Чем ближе к краю любого диапазона, тем больше blend (0..1)
            float e1 = Edge01(b.height01, h);
            float e2 = Edge01(b.temp01, t);
            float e3 = Edge01(b.moist01, m);
            float e4 = Edge01(b.slope01, s);
            float e5 = Edge01(b.dist01, d);
            return Mathf.Clamp01(Mathf.Max(e1, e2, e3, e4, e5));
        }

        private static float Edge01(Vector2 r, float v)
        {
            float width = Mathf.Max(0.0001f, r.y - r.x);
            float a = Mathf.Abs(v - r.x) / width;
            float b = Mathf.Abs(r.y - v) / width;
            float edge = Mathf.Min(a, b); // 0 на краю, 0.5 в середине при равных
            return Mathf.Clamp01(1f - (edge * 2f)); // 1 на краю, 0 в центре
        }
    }
}
