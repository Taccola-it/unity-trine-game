using UnityEngine;
using Trine.World.Noise;

namespace Trine.World.Generation
{
    [CreateAssetMenu(menuName = "Trine/World/World Settings")]
    public class WorldSettingsSO : ScriptableObject
    {
        [Header("Zones")]
        [Min(16)] public int zoneSizeMeters = 64;
        [Range(1, 12)] public int viewDistanceZonesFull = 3;
        [Range(2, 20)] public int viewDistanceZonesDistant = 8;

        [Header("Ocean / Heights")]
        public float oceanLevel = 0f;
        [Min(10f)] public float maxTerrainHeight = 130f;

        // safety clamp (не ломает старый код)
        public float minTerrainHeight = -50f;

        [Header("Landmask (Archipelago)")]
        public NoiseLayerSO landRidged;
        public NoiseLayerSO continentKiller;
        public NoiseLayerSO channels;
        [Min(0f)] public float radialFalloffStrength = 1.25f;
        [Range(0f, 1f)] public float landThreshold = 0.47f;

        [Header("Channels Tuning")]
        [Range(0f, 1f)] public float channelsStrength = 0.55f;
        [Range(0f, 1f)] public float channelsThreshold = 0.65f;

        [Header("Base Height")]
        public NoiseLayerSO heightBase;
        public NoiseLayerSO detailHeight;
        [Min(0f)] public float detailHeightStrengthMeters = 6f;

        [Header("Climate")]
        public NoiseLayerSO temperature;
        public NoiseLayerSO moisture;
        [Range(0f, 1f)] public float tempDistanceInfluence = 0.7f;
        [Range(0f, 1f)] public float moistShoreInfluence = 0.6f;
        [Min(0.1f)] public float shoreWidth = 12f;

        [Header("Biome Patches / Roughness")]
        public NoiseLayerSO biomePatches;
        public NoiseLayerSO roughness;

        [Header("Cliffs")]
        public NoiseLayerSO cliffsMask;
        [Min(0f)] public float cliffsStrengthMeters = 10f;
        [Range(0f, 1f)] public float cliffsThreshold = 0.6f;

        // ✅ NEW: Valheim-like guaranteed start island
        [Header("Start Island (Valheim-like Guarantee)")]
        [Tooltip("Радиус зоны, в которой гарантируется суша вокруг центра мира.")]
        [Min(20f)] public float startIslandRadiusMeters = 140f;

        [Tooltip("Ширина мягкого перехода (blend) к обычной генерации.")]
        [Min(5f)] public float startIslandBlendMeters = 80f;

        [Tooltip("Сколько метров добавлять к высоте в пределах стартового острова.")]
        [Min(0f)] public float startIslandHeightMeters = 10f;

        // ✅ NEW: Water plane tuning
        [Header("Water (Runtime Surface)")]
        [Tooltip("Доп. запас размера воды сверх радиуса стриминга (метры).")]
        [Min(0f)] public float waterFollowExtraMeters = 80f;

        // ✅ визуальный блендинг биомов (у тебя уже используется TerrainMeshBuilder)
        [Header("Biome Visual Blending (Vertex Colors)")]
        [Range(1f, 32f)] public float biomeBlendRadiusMeters = 9f;
    }
}
