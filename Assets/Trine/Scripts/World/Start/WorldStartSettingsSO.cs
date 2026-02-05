using UnityEngine;

namespace Trine.World.Start
{
    [CreateAssetMenu(menuName = "Trine/World/World Start Settings")]
    public class WorldStartSettingsSO : ScriptableObject
    {
        [Header("Target biome id (must match BiomeConfigSO.id)")]
        public string targetBiomeId = "nayavi";

        [Header("Search area (meters)")]
        [Min(0f)] public float minRadius = 20f;
        [Min(1f)] public float maxRadius = 650f;

        [Header("Sampling")]
        [Tooltip("Distance between sample points along the spiral/rings.")]
        [Min(0.5f)] public float step = 14f;

        [Tooltip("How many attempts max before fallback.")]
        [Min(100)] public int maxAttempts = 5000;

        [Header("Constraints")]
        [Range(0f, 1f)] public float minLandMask01 = 0.65f;
        [Tooltip("Minimum meters above ocean level.")]
        public float minAboveOceanMeters = 1.2f;

        [Range(0f, 1f)] public float maxSlope01 = 0.22f;

        [Header("Placement")]
        [Tooltip("Extra height added on top of ground to avoid initial overlap.")]
        public float spawnLiftMeters = 0.25f;
    }
}
