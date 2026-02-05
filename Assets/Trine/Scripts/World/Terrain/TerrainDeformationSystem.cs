using UnityEngine;
using Trine.World.Save;
using Trine.World.Zones;

namespace Trine.World.Terrain
{
    public class TerrainDeformationSystem : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private LayerMask terrainMask;

        public ITerrainDeltaProvider Deltas { get; private set; }
        private int _zoneSize;

        public void Init(ITerrainDeltaProvider deltas, int zoneSizeMeters)
        {
            Deltas = deltas;
            _zoneSize = zoneSizeMeters;
        }

        public void Dig(float radius, float strengthPerHit, float maxDepth)
        {
            if (!Ray(out var hit)) return;

            Vector3 p = hit.point;
            ApplyRadial(p, radius, -Mathf.Abs(strengthPerHit), -Mathf.Abs(maxDepth));
        }

        public void HoeFlatten(float radius, float targetDeltaHeight)
        {
            if (!Ray(out var hit)) return;

            Vector3 p = hit.point;
            // упрощенно: тянем дельту к targetDeltaHeight (в реале берешь finalHeight и считаешь нужный delta)
            ApplyRadialToTarget(p, radius, targetDeltaHeight);
        }

        private bool Ray(out RaycastHit hit)
        {
            hit = default;
            if (cam == null) return false;
            var r = cam.ScreenPointToRay(Input.mousePosition);
            return Physics.Raycast(r, out hit, 200f, terrainMask);
        }

        private void ApplyRadial(Vector3 center, float radius, float deltaPerCell, float clampMin)
        {
            int steps = Mathf.CeilToInt(radius / 2f);
            for (int z = -steps; z <= steps; z++)
                for (int x = -steps; x <= steps; x++)
                {
                    Vector3 p = center + new Vector3(x * 2f, 0f, z * 2f);
                    float d = new Vector2(p.x - center.x, p.z - center.z).magnitude;
                    if (d > radius) continue;

                    float falloff = 1f - (d / radius);
                    float delta = deltaPerCell * falloff;

                    var zone = WorldToZone(p);
                    float cur = Deltas.GetDeltaHeight(zone, p.x, p.z);

                    float next = Mathf.Max(cur + delta, clampMin);
                    Deltas.ApplyDelta(zone, p.x, p.z, next - cur);
                }
        }

        private void ApplyRadialToTarget(Vector3 center, float radius, float targetDelta)
        {
            int steps = Mathf.CeilToInt(radius / 2f);
            for (int z = -steps; z <= steps; z++)
                for (int x = -steps; x <= steps; x++)
                {
                    Vector3 p = center + new Vector3(x * 2f, 0f, z * 2f);
                    float d = new Vector2(p.x - center.x, p.z - center.z).magnitude;
                    if (d > radius) continue;

                    float falloff = 1f - (d / radius);

                    var zone = WorldToZone(p);
                    float cur = Deltas.GetDeltaHeight(zone, p.x, p.z);

                    float desired = Mathf.Lerp(cur, targetDelta, falloff * 0.35f);
                    Deltas.ApplyDelta(zone, p.x, p.z, desired - cur);
                }
        }

        private ZoneCoord WorldToZone(Vector3 p)
        {
            int zx = Mathf.FloorToInt(p.x / _zoneSize);
            int zz = Mathf.FloorToInt(p.z / _zoneSize);
            return new ZoneCoord(zx, zz);
        }
    }
}
