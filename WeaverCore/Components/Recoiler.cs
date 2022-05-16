using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
using WeaverCore.Attributes;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
	/// <summary>
	/// When attached to an enemy object, this component will cause the enemy to recoil from an attack
	/// </summary>
	public class Recoiler : Recoil
	{
		float? _origRecoilSpeed = null;
		public float OriginalRecoilSpeed
		{
			get
			{
				if (_origRecoilSpeed == null)
				{
					_origRecoilSpeed = GetRecoilSpeed();
				}
				return _origRecoilSpeed.Value;
			}
			set
			{
				_origRecoilSpeed = value;
			}
		}

		public virtual void Start()
		{
			OriginalRecoilSpeed = GetRecoilSpeed();
		}

		public void RecoilByDamage(HitInfo hit)
		{
			int attackDirection = 0;
			CardinalDirection cardinalDirection = DirectionUtilities.DegreesToDirection(hit.Direction);
			if (cardinalDirection != CardinalDirection.Up)
			{
				if (cardinalDirection != CardinalDirection.Down)
				{
					if (cardinalDirection == CardinalDirection.Left)
					{
						attackDirection = 2;
					}
				}
				else
				{
					attackDirection = 3;
				}
			}
			else
			{
				attackDirection = 1;
			}
			base.RecoilByDirection(attackDirection, hit.AttackStrength);
		}

		public void RecoilByDirection(CardinalDirection attackDirection, float attackStrength)
		{
			int attackDirection2 = 0;
			if (attackDirection != CardinalDirection.Up)
			{
				if (attackDirection != CardinalDirection.Down)
				{
					if (attackDirection == CardinalDirection.Left)
					{
						attackDirection2 = 2;
					}
				}
				else
				{
					attackDirection2 = 3;
				}
			}
			else
			{
				attackDirection2 = 1;
			}
			base.RecoilByDirection(attackDirection2, attackStrength);
		}


		public void ResetRecoilSpeed()
		{
			SetRecoilSpeed(OriginalRecoilSpeed);
		}


		static Func<Recoil, float> recoilSpeedDel;

		static Func<Recoil, Vector2> recoilDirectionDel;

		static Func<Recoil, float> recoilMagnitudeDel;


		public float GetRecoilSpeed()
		{
			if (recoilSpeedDel == null)
			{
				recoilSpeedDel = ReflectionUtilities.CreateFieldGetter<Recoil, float>(typeof(Recoiler).BaseType.GetField("recoilSpeedBase", BindingFlags.NonPublic | BindingFlags.Instance));
			}

			return recoilSpeedDel.Invoke(this);
		}



		public Vector2 GetRecoilDirection()
        {
			if (recoilDirectionDel == null)
			{
				recoilDirectionDel = ReflectionUtilities.CreateFieldGetter<Recoil, Vector2>(typeof(Recoiler).BaseType.GetField("recoilDirection", BindingFlags.NonPublic | BindingFlags.Instance));
			}
			return recoilDirectionDel.Invoke(this);
		}

		public float GetRecoilMagnitude()
		{
			if (recoilMagnitudeDel == null)
			{
				recoilMagnitudeDel = ReflectionUtilities.CreateFieldGetter<Recoil, float>(typeof(Recoiler).BaseType.GetField("recoilSpeed", BindingFlags.NonPublic | BindingFlags.Instance));
			}

			return recoilSpeedDel.Invoke(this) / GetRecoilSpeed();
		}

		public Vector2 GetCurrentRecoilAmount()
        {
            if (IsRecoiling)
            {
				return GetRecoilDirection() * GetRecoilMagnitude() * GetRecoilSpeed() * Time.deltaTime;
			}
			else
            {
				return default;
            }
        }
	}
}
