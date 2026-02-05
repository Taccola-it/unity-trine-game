# World Generation Pipeline

## Layers (deterministic by seed)
1. HeightField (base)
   - continent noise + ridges + erosion-like smoothing
2. Water / Ocean mask
   - seaLevel
   - distance-to-coast
3. Biome map
   - rules depend on height, humidity/temperature noise, coast distance
4. Surface rendering
   - land material by biome
   - water plane with shoreline blend/foam
5. POI & Spawners
   - biome-filtered placement, persistence

## Invariants
- No worldgen in Update
- Same seed -> same world
- Ocean is water (separate material/rendering)
- Biome assignment independent from rendering
