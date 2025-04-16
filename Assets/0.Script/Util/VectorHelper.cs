using UnityEngine;

namespace Script.Util
{
    public static class VectorHelper
    {
        public static bool InRange(this Transform from, Transform to, float range)
        {
            return (from.position - to.position).sqrMagnitude <= range * range;
        }
    }
}