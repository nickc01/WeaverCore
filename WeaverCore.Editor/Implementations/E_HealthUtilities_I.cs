using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Implementations;
using WeaverCore.Utilities;

namespace WeaverCore.Editor.Implementations
{
    public class E_HealthUtilities_I : HealthUtilities_I
    {
        public override MonoBehaviour GetHealthComponent(GameObject obj)
        {
            return obj.GetComponent<EntityHealth>();
        }

        public override HealthUtilities.HealthComponentType GetHealthComponentType(MonoBehaviour healthComponent)
        {
            return healthComponent is EntityHealth ? HealthUtilities.HealthComponentType.EntityHealth : HealthUtilities.HealthComponentType.None;
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
