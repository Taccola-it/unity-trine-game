using UnityEngine;
using Trine.World.Generation;

namespace Trine.World.Water
{
    public class WaterPlaneRuntime : MonoBehaviour
    {
        [SerializeField] private WorldSettingsSO worldSettings;
        [SerializeField] private Transform followTarget;
        [SerializeField] private Material waterMaterial;

        private MeshRenderer _mr;
        private MeshFilter _mf;

        private void Awake()
        {
            _mf = gameObject.GetComponent<MeshFilter>();
            if (_mf == null) _mf = gameObject.AddComponent<MeshFilter>();

            _mr = gameObject.GetComponent<MeshRenderer>();
            if (_mr == null) _mr = gameObject.AddComponent<MeshRenderer>();

            gameObject.name = "WaterPlaneRuntime";
        }

        private void Start()
        {
            if (worldSettings == null)
            {
                Debug.LogError("[WaterPlaneRuntime] WorldSettingsSO not assigned.");
                enabled = false;
                return;
            }

            if (waterMaterial == null)
            {
                Debug.LogError("[WaterPlaneRuntime] Water material not assigned.");
                enabled = false;
                return;
            }

            _mr.sharedMaterial = waterMaterial;
            _mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _mr.receiveShadows = false;

            RebuildMesh();
        }

        private void LateUpdate()
        {
            if (followTarget == null || worldSettings == null) return;

            // Следуем за игроком, но только по XZ
            Vector3 p = followTarget.position;
            transform.position = new Vector3(p.x, worldSettings.oceanLevel, p.z);
        }

        private void RebuildMesh()
        {
            // Размер воды покрывает полный стриминг + запас
            float zone = worldSettings.zoneSizeMeters;
            float rFull = worldSettings.viewDistanceZonesFull * zone;
            float rDist = worldSettings.viewDistanceZonesDistant * zone;
            float halfSize = Mathf.Max(rFull, rDist) + worldSettings.waterFollowExtraMeters;

            // Простой quad (2 треугольника)
            var mesh = new Mesh();
            mesh.name = "WaterPlaneMesh";

            Vector3[] v =
            {
                new Vector3(-halfSize, 0f, -halfSize),
                new Vector3( halfSize, 0f, -halfSize),
                new Vector3(-halfSize, 0f,  halfSize),
                new Vector3( halfSize, 0f,  halfSize),
            };

            int[] t = { 0, 2, 1, 1, 2, 3 };

            Vector2[] uv =
            {
                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(0,1),
                new Vector2(1,1),
            };

            mesh.vertices = v;
            mesh.triangles = t;
            mesh.uv = uv;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            _mf.sharedMesh = mesh;
        }
    }
}
