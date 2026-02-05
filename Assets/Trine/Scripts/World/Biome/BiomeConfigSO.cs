using UnityEngine;

namespace Trine.World.Biome
{
    [CreateAssetMenu(menuName = "Trine/World/Biome Config")]
    public class BiomeConfigSO : ScriptableObject
    {
        [Header("Identity")]
        public string id = "meadows";
        public string displayName = "Meadows";
        public int priority = 0;

        [Header("Debug / Visual")]
        [Tooltip("÷вет биома дл€ дебаг-отрисовки (vertex colors / карты).")]
        public Color debugColor = new Color(0.35f, 0.75f, 0.35f, 1f);

        [Header("Ranges (0..1 unless noted)")]
        public Vector2 height01 = new Vector2(0.45f, 1.0f); // относительно океана
        public Vector2 temp01 = new Vector2(0.3f, 0.9f);
        public Vector2 moist01 = new Vector2(0.2f, 0.9f);
        public Vector2 slope01 = new Vector2(0.0f, 0.6f);
        public Vector2 dist01 = new Vector2(0.0f, 0.5f);

        [Header("Terrain Modifiers")]
        [Range(0f, 1f)] public float flattenStrength = 0.2f;
        [Range(0f, 1f)] public float smoothStrength = 0.3f;
        [Range(0f, 1f)] public float peakStrength = 0.0f;

        [Header("Water Table")]
        [Tooltip("Ѕолото-подобна€ логика: т€нуть высоту к 'водному столу'.")]
        public float waterTableBiasMeters = 0f;

        // -----------------------------------------------------
        // Optional convenience aliases (не ломают существующий код)
        // -----------------------------------------------------
        public Vector2 temperature01 { get => temp01; set => temp01 = value; }
        public Vector2 moisture01 { get => moist01; set => moist01 = value; }
        public Vector2 distance01 { get => dist01; set => dist01 = value; }
    }
}
