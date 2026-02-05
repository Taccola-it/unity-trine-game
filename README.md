# TRINE — Procedural World Generation (Unity C#)

Valheim-like procedural world generation prototype.

 The project demonstrates the development of an open world generation architecture:
islands, ocean, biomes, streaming zones, and preparation for landscape changes.

---

## Key Features

- Seed-based world generation (deterministic world)
- Multi-stage generation pipeline
- Separation of data and logic (ScriptableObjects)
- Biomes based on climate and geography
- Streaming of world chunks
- Preparation for terrain changes
- World save system

---

## Generation pipeline

1. Base height map (continents)
2. Ocean / coast detection
3. Climate calculation (temperature + moisture)
4. Biome resolving
5. Mesh building
6. Zone streaming

---

## Архитектура

    World
    ├── Noise        → noise generation
    ├── Generation   → world / seed functions
    ├── Biome        → biome definition
    ├── Terrain      → mesh construction
    ├── Streaming    → zone loading
    ├── Water        → ocean
    └── Save         → world saving
---

## Biomes
- Nayavi (starting zone)
- Wildwood
- Wetland
- Pale Peaks
- Great Waters (ocean)

---

## What the project demonstrates
This project demonstrates my skills:

- Game systems architecture
- Procedural generation
- Working with noises (noise layers)
- Optimization through streaming zones
- Data-driven approach
- Pure C# code

---

## Technologies
- Unity
- C#
- ScriptableObjects
- Deterministic RNG

---

## Author
https://github.com/Taccola-it
