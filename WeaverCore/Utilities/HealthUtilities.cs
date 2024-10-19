using UnityEngine;
using WeaverCore.Implementations;

namespace WeaverCore.Utilities
{
    public static class HealthUtilities
    {
        public enum HealthComponentType
        {
            None,
            EntityHealth,
            HealthManager
        }

        static HealthUtilities_I impl = ImplFinder.GetImplementation<HealthUtilities_I>();

        public static bool HasHealthComponent(GameObject obj) => impl.HasHealthComponent(obj);
        public static MonoBehaviour GetHealthComponent(GameObject obj) => impl.GetHealthComponent(obj);
        public static HealthComponentType GetHealthComponentType(MonoBehaviour healthComponent) => impl.GetHealthComponentType(healthComponent);
        public static bool TrySetHealth(MonoBehaviour healthComponent, int newHealth) => impl.TrySetHealth(healthComponent, newHealth);
        public static bool TryGetHealth(MonoBehaviour healthComponent, out int result) => impl.TryGetHealth(healthComponent, out result);
        public static HealthComponentType GetHealthComponentType(GameObject obj) => impl.GetHealthComponentType(obj);
        public static bool TrySetHealth(GameObject obj, int newHealth) => impl.TrySetHealth(obj, newHealth);
        public static bool TryGetHealth(GameObject obj, out int result) => impl.TryGetHealth(obj, out result);

        public static MonoBehaviour GetHealthComponentInParent(GameObject obj)
        {
            var t = obj.transform;

            while (t != null)
            {
                var c = GetHealthComponent(t.gameObject);

                if (c != null)
                {
                    return c;
                }
                else
                {
                    t = t.parent;
                }
            }

            return null;
        }

        public static int GetSmallGeo(GameObject obj) => impl.GetSmallGeo(obj);
        public static int SetSmallGeo(GameObject obj, int geo) => impl.SetSmallGeo(obj, geo);

        public static int GetMediumGeo(GameObject obj) => impl.GetMediumGeo(obj);
        public static int SetMediumGeo(GameObject obj, int geo) => impl.SetMediumGeo(obj, geo);

        public static int GetLargeGeo(GameObject obj) => impl.GetLargeGeo(obj);
        public static int SetLargeGeo(GameObject obj, int geo) => impl.SetLargeGeo(obj, geo);
    }
}
