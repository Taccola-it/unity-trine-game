using System.Collections.Generic;
using UnityEngine;

using Trine.World.Generation;
using Trine.World.Biome;
using Trine.World.Save;
using Trine.World.Streaming;
using Trine.World.Terrain;
using Trine.World.Start;

namespace Trine.World
{
    public class WorldRoot : MonoBehaviour
    {
        [Header("World Config")]
        [SerializeField] private WorldSettingsSO worldSettings;
        [SerializeField] private List<BiomeConfigSO> biomes = new();

        [Header("Start / Spawn")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private WorldStartSettingsSO startSettings;

        [Header("Systems")]
        [SerializeField] private WorldStreamer streamer;
        [SerializeField] private TerrainDeformationSystem deformation;

        [Header("Runtime")]
        [SerializeField] private int seed = 12345;
        [SerializeField] private int worldGenVersion = 1;

        private WorldFunctions _worldFunctions;
        private TerrainDeltaGrid _deltaGrid;

        private void Start()
        {
            Boot();
        }

        public void Boot()
        {
            // -----------------------------
            // Validation
            // -----------------------------
            if (worldSettings == null)
            {
                Debug.LogError("[WorldRoot] WorldSettingsSO is not assigned.");
                return;
            }

            if (streamer == null)
            {
                Debug.LogError("[WorldRoot] WorldStreamer is not assigned.");
                return;
            }

            if (deformation == null)
            {
                Debug.LogError("[WorldRoot] TerrainDeformationSystem is not assigned.");
                return;
            }

            if (biomes == null || biomes.Count == 0)
            {
                Debug.LogError("[WorldRoot] Biomes list is empty.");
                return;
            }

            // -----------------------------
            // Biome resolver
            // -----------------------------
            // Приоритет: чем выше priority — тем раньше проверяется
            biomes.Sort((a, b) => b.priority.CompareTo(a.priority));
            var biomeResolver = new BiomeResolver(biomes);

            // -----------------------------
            // World functions (DETERMINISTIC)
            // -----------------------------
            _worldFunctions = new WorldFunctions(seed, worldSettings, biomeResolver);

            if (playerTransform != null)
            {
                Vector3 spawn = WorldStartFinder.FindSpawnPoint(_worldFunctions, worldSettings);
                playerTransform.position = spawn;
            }

            // -----------------------------
            // Terrain deltas (persistence-ready)
            // -----------------------------
            _deltaGrid = new TerrainDeltaGrid(
                worldSettings.zoneSizeMeters,
                cellSizeMeters: 2
            );

            // -----------------------------
            // START POSITION (VALHEIM-STYLE)
            // -----------------------------
            if (playerTransform != null && startSettings != null)
            {
                Vector3 spawnPoint = WorldStartFinder.FindSpawnPoint(
                    _worldFunctions,
                    worldSettings,
                    startSettings,
                    seed
                );

                playerTransform.position = spawnPoint;
            }
            else
            {
                Debug.LogWarning(
                    "[WorldRoot] PlayerTransform or WorldStartSettingsSO not assigned. " +
                    "Player will NOT be repositioned."
                );
            }

            // -----------------------------
            // Streaming & deformation init
            // -----------------------------
            streamer.Init(
                _worldFunctions,
                worldSettings,
                _deltaGrid
            );

            deformation.Init(
                _deltaGrid,
                worldSettings.zoneSizeMeters
            );

            Debug.Log(
                $"[WorldRoot] Boot OK | Seed={seed} | WorldGenVersion={worldGenVersion} | Biomes={biomes.Count}"
            );
        }
    }
}
