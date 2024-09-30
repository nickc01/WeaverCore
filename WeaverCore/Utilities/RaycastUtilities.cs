using UnityEngine;

namespace WeaverCore.Utilities
{
    public static class RaycastUtilities
    {
        static RaycastHit2D[] hitCache = new RaycastHit2D[1];
        static int terrainMask = -1;

        public static int TerrainMask => terrainMask == -1 ? terrainMask = LayerMask.GetMask("Terrain") : terrainMask;

        public static bool SimpleCastTo(Vector2 origin, Vector2 dest, out RaycastHit2D result, float distance = float.NaN, int layerMask = -1)
        {
            if (float.IsNaN(distance))
            {
                distance = (dest - origin).magnitude;
            }

            if (distance < 0f)
            {
                distance = (dest - origin).magnitude - distance;
            }

            return SimpleCast(origin, (dest - origin).normalized, out result, distance, layerMask);
        }

        public static bool SimpleCast(Vector2 origin, Vector2 direction, out RaycastHit2D result, float distance, int layerMask = -1)
        {
            if (layerMask < 0)
            {
                layerMask = TerrainMask;
            }
            var count = Physics2D.RaycastNonAlloc(origin, direction, hitCache, distance, layerMask);

            if (count > 0)
            {
                result = hitCache[0];
            }
            else
            {
                result = default;
            }

            return count > 0;
        }
    }
}