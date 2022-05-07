using System;
using WeaverCore.Enums;
using WeaverCore.Implementations;

namespace WeaverCore.Editor.Implementations
{
    public class E_HitTaker_I : HitTaker_I
    {
        public override bool Hit(UnityEngine.Transform target, UnityEngine.GameObject attacker, int damage, AttackType type, CardinalDirection hitDirection)
        {
            return HitDefault(target, attacker, damage, type, hitDirection);
        }
    }
}
