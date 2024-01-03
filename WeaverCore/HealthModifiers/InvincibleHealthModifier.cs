using WeaverCore.Interfaces;

namespace WeaverCore
{
    /// <summary>
    /// When added to an <see cref="Components.EntityHealth"/> component, will cause the health to never change
    /// </summary>
    public class InvincibleHealthModifier : IHealthModifier
    {
        public int Priority => int.MaxValue;

        public int OnHealthChange(int oldHealth, int newHealth)
        {
            return oldHealth;
        }
    }
}
