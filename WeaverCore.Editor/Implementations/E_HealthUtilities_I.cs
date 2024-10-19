using System;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Implementations;
using WeaverCore.Utilities;

namespace WeaverCore.Editor.Implementations
{
    public class E_HealthUtilities_I : HealthUtilities_I
    {
        static Func<int, int> smallGeoDropsGetter;
        static Func<int, int> mediumGeoDropsGetter;
        static Func<int, int> largeGeoDropsGetter;

        public override MonoBehaviour GetHealthComponent(GameObject obj)
        {
            return obj.GetComponent<EntityHealth>();
        }

        public override HealthUtilities.HealthComponentType GetHealthComponentType(MonoBehaviour healthComponent)
        {
            return healthComponent is EntityHealth ? HealthUtilities.HealthComponentType.EntityHealth : HealthUtilities.HealthComponentType.None;
        }

        public override int GetLargeGeo(GameObject obj)
        {
            if (obj.TryGetComponent<EntityHealth>(out var eh))
            {
                return eh.LargeGeo;
            }

            return -1;
        }

        public override int GetMediumGeo(GameObject obj)
        {
            if (obj.TryGetComponent<EntityHealth>(out var eh))
            {
                return eh.MediumGeo;
            }

            return -1;
        }

        public override int GetSmallGeo(GameObject obj)
        {
            if (obj.TryGetComponent<EntityHealth>(out var eh))
            {
                return eh.SmallGeo;
            }

            return -1;
        }

        public override bool HasHealthComponent(GameObject obj)
        {
            return GetHealthComponent(obj) != null;
        }

        public override int SetLargeGeo(GameObject obj, int geo)
        {
            if (obj.TryGetComponent<EntityHealth>(out var eh))
            {
                eh.LargeGeo = geo;
                return geo;
            }

            return -1;
        }

        public override int SetMediumGeo(GameObject obj, int geo)
        {
            if (obj.TryGetComponent<EntityHealth>(out var eh))
            {
                eh.MediumGeo = geo;
                return geo;
            }

            return -1;
        }

        public override int SetSmallGeo(GameObject obj, int geo)
        {
            if (obj.TryGetComponent<EntityHealth>(out var eh))
            {
                eh.SmallGeo = geo;
                return geo;
            }

            return -1;
        }

        public override bool TryGetHealth(MonoBehaviour healthComponent, out int result)
        {
            if (healthComponent is EntityHealth eHealth)
            {
                result = eHealth.Health;
                return true;
            }
            else
            {
                result = -1;
                return false;
            }
        }

        public override bool TrySetHealth(MonoBehaviour healthComponent, int newHealth)
        {
            if (healthComponent is EntityHealth eHealth)
            {
                eHealth.Health = newHealth;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
