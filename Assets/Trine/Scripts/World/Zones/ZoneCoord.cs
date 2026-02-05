using System;
using UnityEngine;

namespace Trine.World.Zones
{
    [Serializable]
    public readonly struct ZoneCoord : IEquatable<ZoneCoord>
    {
        public readonly int x;
        public readonly int z;

        public ZoneCoord(int x, int z) { this.x = x; this.z = z; }

        public bool Equals(ZoneCoord other) => x == other.x && z == other.z;
        public override bool Equals(object obj) => obj is ZoneCoord other && Equals(other);
        public override int GetHashCode() => unchecked((x * 73856093) ^ (z * 19349663));
        public override string ToString() => $"({x},{z})";
    }
}
