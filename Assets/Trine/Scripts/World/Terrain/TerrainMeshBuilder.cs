using UnityEngine;
using Trine.World.Generation;
using Trine.World.Biome;

namespace Trine.World.Terrain
{
    public static class TerrainMeshBuilder
    {
        // 9-точечный kernel для мягкого смешивания биомов по вершинам
        private static readonly Vector2[] Kernel9 =
        {
            new Vector2( 0,  0),
            new Vector2( 1,  0),
            new Vector2(-1,  0),
            new Vector2( 0,  1),
            new Vector2( 0, -1),
            new Vector2( 1,  1),
            new Vector2(-1,  1),
            new Vector2( 1, -1),
            new Vector2(-1, -1),
        };

        /// <summary>
        /// Строит меш зоны: вершины/треугольники + colors[] по смешанному биому.
        /// </summary>
        public static Mesh BuildZoneMesh(
            WorldFunctions wf,
            WorldSettingsSO settings,
            Vector2Int zoneCoord,
            int lod,
            float worldX0,
            float worldZ0)
        {
            int zoneSize = settings.zoneSizeMeters;

            // LOD: 0=1м, 1=2м, 2=4м...
            int step = Mathf.Max(1, 1 << Mathf.Clamp(lod, 0, 6));
            int vertsPerSide = (zoneSize / step) + 1;

            int vertCount = vertsPerSide * vertsPerSide;
            var vertices = new Vector3[vertCount];
            var colors = new Color[vertCount];
            var uvs = new Vector2[vertCount];

            float blendRadiusMeters = Mathf.Clamp(settings.biomeBlendRadiusMeters, 3f, 20f);

            int i = 0;
            for (int z = 0; z < vertsPerSide; z++)
            {
                for (int x = 0; x < vertsPerSide; x++)
                {
                    float wx = worldX0 + x * step;
                    float wz = worldZ0 + z * step;

                    var s = wf.Sample(wx, wz);

                    // ✅ ВАЖНО: у тебя есть baseHeight (а finalHeight нет)
                    float h = s.baseHeight;

                    // Safety clamp
                    h = Mathf.Clamp(h, settings.minTerrainHeight, settings.maxTerrainHeight);

                    // Локальные координаты меша внутри зоны
                    vertices[i] = new Vector3(x * step, h, z * step);

                    // UV (пока просто нормализованные)
                    uvs[i] = new Vector2(
                        (float)x / (vertsPerSide - 1),
                        (float)z / (vertsPerSide - 1)
                    );

                    // Цвет биома (с мягким смешиванием)
                    colors[i] = SampleBlendedBiomeColor(wf, wx, wz, blendRadiusMeters);

                    i++;
                }
            }

            // Triangles
            int quadCount = (vertsPerSide - 1) * (vertsPerSide - 1);
            int[] triangles = new int[quadCount * 6];

            int ti = 0;
            for (int z = 0; z < vertsPerSide - 1; z++)
            {
                for (int x = 0; x < vertsPerSide - 1; x++)
                {
                    int a = z * vertsPerSide + x;
                    int b = a + 1;
                    int c = a + vertsPerSide;
                    int d = c + 1;

                    // a-c-b
                    triangles[ti++] = a;
                    triangles[ti++] = c;
                    triangles[ti++] = b;

                    // b-c-d
                    triangles[ti++] = b;
                    triangles[ti++] = c;
                    triangles[ti++] = d;
                }
            }

            var mesh = new Mesh();
            mesh.indexFormat = (vertCount > 65000)
                ? UnityEngine.Rendering.IndexFormat.UInt32
                : UnityEngine.Rendering.IndexFormat.UInt16;

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.colors = colors;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        /// <summary>
        /// Смешанный цвет: берём 9 семплов вокруг точки, считаем "голоса" доминирующих биомов,
        /// затем смешиваем debugColor пропорционально весам.
        /// </summary>
        private static Color SampleBlendedBiomeColor(WorldFunctions wf, float x, float z, float radiusMeters)
        {
            var dict = new System.Collections.Generic.Dictionary<BiomeConfigSO, float>(8);

            for (int k = 0; k < Kernel9.Length; k++)
            {
                Vector2 o = Kernel9[k];

                // Центр весит больше → стабильнее внутри биома
                float w = (k == 0) ? 2.2f : 1.0f;

                float sx = x + o.x * radiusMeters;
                float sz = z + o.y * radiusMeters;

                var s = wf.Sample(sx, sz);
                var biome = s.biome.primary;

                if (biome == null) continue;

                if (dict.TryGetValue(biome, out float cur)) dict[biome] = cur + w;
                else dict[biome] = w;
            }

            if (dict.Count == 0)
                return new Color(0.5f, 0.5f, 0.5f, 1f);

            float sum = 0f;
            foreach (var kv in dict) sum += kv.Value;

            Color c = Color.black;
            foreach (var kv in dict)
            {
                float t = kv.Value / sum;
                c += kv.Key.debugColor * t;
            }

            c.a = 1f;
            return c;
        }
    }
}
