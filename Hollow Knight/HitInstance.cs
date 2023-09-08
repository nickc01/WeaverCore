/*using System;
using UnityEngine;

[Serializable]
public struct HitInstance
{
    public GameObject Source;

    public AttackTypes AttackType;

    public bool CircleDirection;

    public int DamageDealt;

    public float Direction;

    public bool IgnoreInvulnerable;

    public float MagnitudeMultiplier;

    public float MoveAngle;

    public bool MoveDirection;

    public float Multiplier;

    public SpecialTypes SpecialType;

    public bool IsExtraDamage;

    public float GetActualDirection(Transform target)
    {
        if (Source != null && target != null && CircleDirection)
        {
            Vector2 vector = (Vector2)target.position - (Vector2)Source.transform.position;
            return Mathf.Atan2(vector.y, vector.x) * 57.29578f;
        }
        return Direction;
    }
}*/
