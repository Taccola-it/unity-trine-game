using UnityEngine;

namespace Trine.World.Save
{
    public class WorldSaveSystem
    {
        public int Seed { get; private set; }
        public int WorldGenVersion { get; private set; }

        public WorldSaveSystem(int seed, int worldGenVersion)
        {
            Seed = seed;
            WorldGenVersion = worldGenVersion;
        }

        // Здесь будет сериализация:
        // - seed/settings/version
        // - terrain deltas (sparse)
        // - destroyed objects
        // - placed objects
        // - cleared POI
        //
        // Важно: мир НЕ сохраняем целиком.
    }
}
