using UnityEngine;
using Trine.World.Generation;

namespace Trine.World.Start
{
    public static class WorldStartFinder
    {
        // Быстрый детерминированный поиск точки на суше.
        // Мы не используем Random runtime — только WorldFunctions.
        public static Vector3 FindSpawnPoint(WorldFunctions wf, WorldSettingsSO settings)
        {
            // Сначала пробуем центр (Valheim start)
            if (TryPoint(wf, settings, 0f, 0f, out Vector3 p0))
                return p0;

            // Спираль вокруг центра
            const int rings = 60;              // радиус поиска
            const float step = 12f;            // шаг по метрам
            const int samplesPerRing = 24;

            for (int r = 1; r <= rings; r++)
            {
                float radius = r * step;
                for (int i = 0; i < samplesPerRing; i++)
                {
                    float a = (i / (float)samplesPerRing) * Mathf.PI * 2f;
                    float x = Mathf.Cos(a) * radius;
                    float z = Mathf.Sin(a) * radius;

                    if (TryPoint(wf, settings, x, z, out Vector3 p))
                        return p;
                }
            }

            // Фолбэк: просто ставим над океаном, если что-то сломано
            return new Vector3(0f, settings.oceanLevel + 5f, 0f);
        }

        private static bool TryPoint(WorldFunctions wf, WorldSettingsSO settings, float x, float z, out Vector3 point)
        {
            var s = wf.Sample(x, z);

            // Должно быть выше воды
            float y = s.baseHeight;
            if (y < settings.oceanLevel + 1.2f)
            {
                point = default;
                return false;
            }

            // В идеале: стартовый биом Nayavi (если у тебя id именно "nayavi")
            // Если id отличается — просто не будет фильтра по биому, но суша найдётся.
            if (s.biome.primary != null && s.biome.primary.id != null)
            {
                // разрешаем и без фильтра, но предпочитаем Nayavi
                // (поэтому здесь не режем жёстко)
            }

            point = new Vector3(x, y + 1.0f, z);
            return true;
        }
    }
}
