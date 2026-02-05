using UnityEngine;
using Trine.World.Noise;
using Trine.World.Biome;

namespace Trine.World.Generation
{
    public readonly struct WorldSample
    {
        public readonly float landMask01;
        public readonly float baseHeight;
        public readonly float temp01;
        public readonly float moist01;
        public readonly float rough01;
        public readonly float slope01;
        public readonly BiomeSample biome;

        public WorldSample(
            float landMask01,
            float baseHeight,
            float temp01,
            float moist01,
            float rough01,
            float slope01,
            BiomeSample biome)
        {
            this.landMask01 = landMask01;
            this.baseHeight = baseHeight;
            this.temp01 = temp01;
            this.moist01 = moist01;
            this.rough01 = rough01;
            this.slope01 = slope01;
            this.biome = biome;
        }
    }

    public sealed class WorldFunctions
    {
        private readonly int _seed;
        private readonly WorldSettingsSO _settings;
        private readonly BiomeResolver _biomeResolver;

        public WorldFunctions(int seed, WorldSettingsSO settings, BiomeResolver biomeResolver)
        {
            _seed = seed;
            _settings = settings;
            _biomeResolver = biomeResolver;
        }

        // ============================================================
        // PUBLIC API
        // ============================================================

        public WorldSample Sample(float x, float z)
        {
            float r01 = Distance01FromCenter(x, z);

            float landMask = ComputeLandMask01(x, z, r01);
            float baseHeight = ComputeBaseHeight(x, z, landMask);

            float temp01 = ComputeTemperature01(x, z, r01);
            float moist01 = ComputeMoisture01(x, z, baseHeight);

            float rough01 = Sample01(_settings.roughness, x, z);
            float slope01 = EstimateSlope01(x, z, landMask);

            float height01 = Height01RelativeToOcean(baseHeight);
            float patch01 = Sample01(_settings.biomePatches, x, z);

            BiomeSample biome = _biomeResolver.Resolve(
                height01,
                temp01,
                moist01,
                slope01,
                r01,
                patch01
            );

            return new WorldSample(
                landMask,
                baseHeight,
                temp01,
                moist01,
                rough01,
                slope01,
                biome
            );
        }

        public float ComputeFinalHeight(float x, float z, float deltaHeight)
        {
            WorldSample s = Sample(x, z);
            float h = ApplyBiomeHeightModifiers(s, x, z);
            return h + deltaHeight;
        }

        // ============================================================
        // LANDMASK (ARCHIPELAGO CORE)
        // ============================================================

        private float ComputeLandMask01(float x, float z, float r01)
        {
            float ridged = Sample01(_settings.landRidged, x, z);
            float killer = Sample01(_settings.continentKiller, x, z);

            float falloff = Mathf.Pow(r01, 1.35f) * _settings.radialFalloffStrength;

            float raw = ridged - falloff - killer * 0.6f;

            // ---------- CHANNELS (проливы / каналы) ----------
            if (_settings.channels != null)
            {
                float ch = Sample01(_settings.channels, x, z);
                float band = Mathf.SmoothStep(
                    _settings.channelsThreshold,
                    1f,
                    ch
                );
                raw -= band * _settings.channelsStrength;
            }

            float land = Mathf.Clamp01((raw - _settings.landThreshold) * 2.2f);
            return land;
        }

        // ============================================================
        // HEIGHT
        // ============================================================

        private float ComputeBaseHeight(float x, float z, float landMask01)
        {
            float hn = Sample01(_settings.heightBase, x, z);

            float h = hn * _settings.maxTerrainHeight * landMask01;

            // Подводный рельеф
            if (landMask01 < 0.01f)
            {
                h = -Mathf.Abs(hn) * 15f;
            }

            // ---------- DETAIL HEIGHT ----------
            if (landMask01 > 0.01f && _settings.detailHeight != null)
            {
                float d = Sample01(_settings.detailHeight, x, z) * 2f - 1f;
                h += d * _settings.detailHeightStrengthMeters * landMask01;
            }

            return h + _settings.oceanLevel;
        }

        // ============================================================
        // CLIMATE
        // ============================================================

        private float ComputeTemperature01(float x, float z, float r01)
        {
            float n = Sample01(_settings.temperature, x, z);
            float byDist = 1f - Mathf.Clamp01(r01 * _settings.tempDistanceInfluence);
            return Mathf.Clamp01(n * 0.6f + byDist * 0.6f);
        }

        private float ComputeMoisture01(float x, float z, float baseHeight)
        {
            float n = Sample01(_settings.moisture, x, z);

            float shore =
                1f -
                Mathf.Clamp01(
                    Mathf.Abs(baseHeight - _settings.oceanLevel) /
                    Mathf.Max(0.001f, _settings.shoreWidth)
                );

            float shoreBoost = shore * _settings.moistShoreInfluence;
            return Mathf.Clamp01(n * 0.7f + shoreBoost);
        }

        // ============================================================
        // BIOME HEIGHT MODIFIERS + CLIFFS
        // ============================================================

        private float ApplyBiomeHeightModifiers(WorldSample s, float x, float z)
        {
            float h = s.baseHeight;
            var b = s.biome.primary;

            if (b != null)
            {
                // микрорельеф
                float micro = (s.rough01 * 2f - 1f) * 1.2f;
                h += micro * (1f - b.smoothStrength);

                // уплощение
                if (b.flattenStrength > 0f)
                {
                    float target = _settings.oceanLevel + b.waterTableBiasMeters;
                    h = Mathf.Lerp(h, target, b.flattenStrength * 0.35f);
                }

                // пики
                if (b.peakStrength > 0f)
                {
                    float delta = h - _settings.oceanLevel;
                    h = _settings.oceanLevel + delta * (1f + b.peakStrength * 0.6f);
                }
            }

            // ---------- CLIFFS MASK ----------
            if (_settings.cliffsMask != null && h > _settings.oceanLevel)
            {
                float cm = Sample01(_settings.cliffsMask, x, z);
                float cliff = Mathf.SmoothStep(
                    _settings.cliffsThreshold,
                    1f,
                    cm
                );

                if (cliff > 0f)
                {
                    float step = 1.5f;
                    float quant =
                        Mathf.Round((h - _settings.oceanLevel) / step) * step +
                        _settings.oceanLevel;

                    float strength = Mathf.Clamp01(
                        _settings.cliffsStrengthMeters / 12f
                    );

                    h = Mathf.Lerp(h, quant, cliff * strength);
                }
            }

            return h;
        }

        // ============================================================
        // HELPERS
        // ============================================================

        private float Sample01(NoiseLayerSO layer, float x, float z)
        {
            if (layer == null) return 0f;
            return Mathf.Clamp01(
                NoiseSampler.Fractal(layer, _seed, x, z)
            );
        }

        private float Height01RelativeToOcean(float h)
        {
            return Mathf.Clamp01(
                (h - _settings.oceanLevel) /
                Mathf.Max(1f, _settings.maxTerrainHeight)
            );
        }

        private float EstimateSlope01(float x, float z, float landMask01)
        {
            float eps = 2f;

            float h1 = ComputeBaseHeight(x + eps, z, landMask01);
            float h2 = ComputeBaseHeight(x - eps, z, landMask01);
            float h3 = ComputeBaseHeight(x, z + eps, landMask01);
            float h4 = ComputeBaseHeight(x, z - eps, landMask01);

            float dx = Mathf.Abs(h1 - h2);
            float dz = Mathf.Abs(h3 - h4);

            return Mathf.Clamp01((dx + dz) / (eps * 8f));
        }

        private static float Distance01FromCenter(float x, float z)
        {
            float r = new Vector2(x, z).magnitude;
            const float R = 6000f;
            return Mathf.Clamp01(r / R);
        }
    }
}
