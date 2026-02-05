using Trine.World.Zones;

namespace Trine.World.Save
{
    public interface ITerrainDeltaProvider
    {
        float GetDeltaHeight(ZoneCoord zone, float worldX, float worldZ);
        void ApplyDelta(ZoneCoord zone, float worldX, float worldZ, float delta);
    }
}
