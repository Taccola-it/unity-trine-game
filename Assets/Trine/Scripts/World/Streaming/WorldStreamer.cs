using System.Collections.Generic;
using UnityEngine;
using Trine.World.Generation;
using Trine.World.Terrain;

namespace Trine.World.Streaming
{
    public class WorldStreamer : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Transform player;

        [Header("Prefabs")]
        [SerializeField] private WorldZone zonePrefab;

        [Header("Streaming")]
        [Min(1)] public int viewDistanceZones = 3;
        public int lodNear = 0;
        public int lodFar = 1;

        private WorldFunctions _wf;
        private WorldSettingsSO _settings;
        private object _deltaGrid; // если у теб€ есть TerrainDeltaGrid Ч поставь тип назад

        private readonly Dictionary<Vector2Int, WorldZone> _zones = new();

        public void Init(WorldFunctions wf, WorldSettingsSO settings, object deltaGrid)
        {
            _wf = wf;
            _settings = settings;
            _deltaGrid = deltaGrid;
        }

        private void Update()
        {
            if (_wf == null || _settings == null || player == null || zonePrefab == null)
                return;

            StreamAroundPlayer();
        }

        private void StreamAroundPlayer()
        {
            Vector2Int center = WorldToZone(player.position);

            for (int dz = -viewDistanceZones; dz <= viewDistanceZones; dz++)
            for (int dx = -viewDistanceZones; dx <= viewDistanceZones; dx++)
            {
                Vector2Int c = new Vector2Int(center.x + dx, center.y + dz);

                int lod = (Mathf.Abs(dx) <= 1 && Mathf.Abs(dz) <= 1) ? lodNear : lodFar;
                EnsureZone(c, lod);
            }
        }

        private void EnsureZone(Vector2Int coord, int lod)
        {
            if (_zones.ContainsKey(coord))
                return;

            WorldZone z = Instantiate(zonePrefab, ZoneToWorldOrigin(coord), Quaternion.identity, transform);
            z.name = $"Zone_{coord.x}_{coord.y}";

            float wx0 = coord.x * _settings.zoneSizeMeters;
            float wz0 = coord.y * _settings.zoneSizeMeters;

            Mesh mesh = TerrainMeshBuilder.BuildZoneMesh(_wf, _settings, coord, lod, wx0, wz0);
            z.ApplyMesh(mesh);

            _zones.Add(coord, z);
        }

        private Vector2Int WorldToZone(Vector3 pos)
        {
            int x = Mathf.FloorToInt(pos.x / _settings.zoneSizeMeters);
            int z = Mathf.FloorToInt(pos.z / _settings.zoneSizeMeters);
            return new Vector2Int(x, z);
        }

        private Vector3 ZoneToWorldOrigin(Vector2Int coord)
        {
            return new Vector3(coord.x * _settings.zoneSizeMeters, 0f, coord.y * _settings.zoneSizeMeters);
        }
    }
}
