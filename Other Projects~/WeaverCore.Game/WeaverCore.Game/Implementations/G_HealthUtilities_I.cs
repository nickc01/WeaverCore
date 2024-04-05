using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Implementations;
using WeaverCore.Utilities;
using static UnityEngine.Networking.UnityWebRequest;

namespace WeaverCore.Game.Implementations
{
    public class G_HealthUtilities_I : HealthUtilities_I
    {
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
    }
}
