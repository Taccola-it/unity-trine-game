using UnityEngine;

namespace Trine.World.Noise
{
    [CreateAssetMenu(menuName = "Trine/World/Noise Layer")]
    public class NoiseLayerSO : ScriptableObject
    {
        public float frequency = 0.002f;
        public float amplitude = 1f;
        [Range(1, 8)] public int octaves = 4;
        public float lacunarity = 2f;
        public float persistence = 0.5f;

        public bool ridged = false;

        public bool domainWarp = true;
        public float warpFrequency = 0.0015f;
        public float warpAmount = 30f;

        public int seedOffset = 0;
    }
}
