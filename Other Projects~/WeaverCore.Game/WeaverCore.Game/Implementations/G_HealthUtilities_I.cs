using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Implementations;
using WeaverCore.Utilities;
using static UnityEngine.Networking.UnityWebRequest;
using System;

namespace WeaverCore.Game.Implementations
{
    public class G_HealthUtilities_I : HealthUtilities_I
    {
        static Func<HealthManager, int> smallGeoDropsGetter;
        static Func<HealthManager, int> mediumGeoDropsGetter;
        static Func<HealthManager, int> largeGeoDropsGetter;

        public override MonoBehaviour GetHealthComponent(GameObject obj)
        {
            if (obj.TryGetComponent<EntityHealth>(out var eh))
            {
                return eh;
            }
            else if (obj.TryGetComponent<HealthManager>(out var hm))
            {
                return hm;
            }

            return null;
        }

        public override HealthUtilities.HealthComponentType GetHealthComponentType(MonoBehaviour healthComponent)
        {
            if (healthComponent is EntityHealth)
            {
                return HealthUtilities.HealthComponentType.EntityHealth;
            }
            else if (healthComponent is HealthManager)
            {
                return HealthUtilities.HealthComponentType.HealthManager;
            }
            else
            {
                return HealthUtilities.HealthComponentType.None;
            }
        }

        public override bool HasHealthComponent(GameObject obj)
        {
            return GetHealthComponent(obj) != null;
        }

        public override bool TryGetHealth(MonoBehaviour healthComponent, out int result)
        {
            if (healthComponent is EntityHealth eHealth)
            {
                result = eHealth.Health;
                return true;
            }
            else if (healthComponent is HealthManager hManager)
            {
                result = hManager.hp;
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
            else if (healthComponent is HealthManager hManager)
            {
                hManager.hp = newHealth;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int GetLargeGeo(GameObject obj)
        {
            if (obj.TryGetComponent<EntityHealth>(out var eh))
            {
                return eh.LargeGeo;
            }
            else if (obj.TryGetComponent<HealthManager>(out var hManager))
            {
                if (largeGeoDropsGetter == null)
                {
                    largeGeoDropsGetter = ReflectionUtilities.CreateFieldGetter<HealthManager,int>("largeGeoDrops");
                }
                return largeGeoDropsGetter(hManager);
            }

            return -1;
        }

        public override int GetMediumGeo(GameObject obj)
        {
            if (obj.TryGetComponent<EntityHealth>(out var eh))
            {
                return eh.MediumGeo;
            }
            else if (obj.TryGetComponent<HealthManager>(out var hManager))
            {
                if (mediumGeoDropsGetter == null)
                {
                    mediumGeoDropsGetter = ReflectionUtilities.CreateFieldGetter<HealthManager,int>("mediumGeoDrops");
                }
                return mediumGeoDropsGetter(hManager);
            }

            return -1;
        }

        public override int GetSmallGeo(GameObject obj)
        {
            if (obj.TryGetComponent<EntityHealth>(out var eh))
            {
                return eh.SmallGeo;
            }
            else if (obj.TryGetComponent<HealthManager>(out var hManager))
            {
                if (smallGeoDropsGetter == null)
                {
                    smallGeoDropsGetter = ReflectionUtilities.CreateFieldGetter<HealthManager,int>("smallGeoDrops");
                }
                return smallGeoDropsGetter(hManager);
            }

            return -1;
        }

        public override int SetLargeGeo(GameObject obj, int geo)
        {
            if (obj.TryGetComponent<EntityHealth>(out var eh))
            {
                eh.LargeGeo = geo;
                return geo;
            }
            else if (obj.TryGetComponent<HealthManager>(out var hManager))
            {
                hManager.SetGeoLarge(geo);
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
            else if (obj.TryGetComponent<HealthManager>(out var hManager))
            {
                hManager.SetGeoMedium(geo);
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
            else if (obj.TryGetComponent<HealthManager>(out var hManager))
            {
                hManager.SetGeoSmall(geo);
                return geo;
            }

            return -1;
        }
    }
}
