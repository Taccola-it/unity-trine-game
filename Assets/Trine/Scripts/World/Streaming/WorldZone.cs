using UnityEngine;

namespace Trine.World.Streaming
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class WorldZone : MonoBehaviour
    {
        [Header("Renderer")]
        [SerializeField] private Material vertexColorMaterial;

        private MeshFilter _mf;
        private MeshRenderer _mr;
        private MeshCollider _mc;

        private void Awake()
        {
            _mf = GetComponent<MeshFilter>();
            _mr = GetComponent<MeshRenderer>();
            _mc = GetComponent<MeshCollider>();
        }

        public void ApplyMesh(Mesh mesh)
        {
            _mf.sharedMesh = mesh;
            _mc.sharedMesh = mesh;

            if (vertexColorMaterial != null)
                _mr.sharedMaterial = vertexColorMaterial;
        }
    }
}
