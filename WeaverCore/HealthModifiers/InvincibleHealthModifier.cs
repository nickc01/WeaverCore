using WeaverCore.Interfaces;

namespace WeaverCore
{
    public class InvincibleHealthModifier : IHealthModifier
    {
        public int Priority => int.MaxValue;

        public int OnHealthChange(int oldHealth, int newHealth)
        {
            return oldHealth;
        }
    }
}
