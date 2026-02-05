using System.Collections.Generic;
using Trine.World.Zones;
using UnityEngine;

namespace Trine.World.Save
{
    // Sparse storage: храним только измененные "ячейки" (квантизируем координаты внутри зоны)
    public class TerrainDeltaGrid : ITerrainDeltaProvider
    {
        private readonly int _cellSize; // например 2м
        private readonly int _zoneSize;

        // key: (zoneCoord -> dict(cellKey -> delta))
        private readonly Dictionary<ZoneCoord, Dictionary<int, float>> _deltas = new();

        public TerrainDeltaGrid(int zoneSizeMeters, int cellSizeMeters = 2)
        {
            _zoneSize = zoneSizeMeters;
            _cellSize = Mathf.Max(1, cellSizeMeters);
        }

        public float GetDeltaHeight(ZoneCoord zone, float worldX, float worldZ)
        {
            if (!_deltas.TryGetValue(zone, out var map)) return 0f;
            int key = CellKey(zone, worldX, worldZ);
            return map.TryGetValue(key, out var v) ? v : 0f;
        }

        public void ApplyDelta(ZoneCoord zone, float worldX, float worldZ, float delta)
        {
            int key = CellKey(zone, worldX, worldZ);
            if (!_deltas.TryGetValue(zone, out var map))
            {
                map = new Dictionary<int, float>();
                _deltas.Add(zone, map);
            }

            map.TryGetValue(key, out float cur);
            float next = cur + delta;

            // если почти 0 — удаляем (экономим)
            if (Mathf.Abs(next) < 0.001f) map.Remove(key);
            else map[key] = next;
        }

        private int CellKey(ZoneCoord zone, float wx, float wz)
        {
            // локальные координаты в зоне
            float ox = zone.x * _zoneSize;
            float oz = zone.z * _zoneSize;

            int lx = Mathf.Clamp(Mathf.FloorToInt((wx - ox) / _cellSize), 0, (_zoneSize / _cellSize));
            int lz = Mathf.Clamp(Mathf.FloorToInt((wz - oz) / _cellSize), 0, (_zoneSize / _cellSize));

            return (lx & 0xFFFF) | (lz << 16);
        }
    }
}
