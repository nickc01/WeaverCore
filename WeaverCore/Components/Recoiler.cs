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
		public class RecoilOverride : IComparable<RecoilOverride>, IDisposable
		{
			public readonly Recoiler Recoiler;

			int _priority;

			public int Priority
			{
				get => _priority;
				set
				{
					if (_priority != value)
					{
						_priority = value;
						Recoiler.RefreshRecoilSpeed();
                    }
				}

            }

			float _recoilSpeed;

			public float RecoilSpeed
			{
				get => _recoilSpeed;
				set
				{
					if (_recoilSpeed != value)
					{
						_recoilSpeed = value;
						Recoiler.RefreshRecoilSpeed();
					}
				}
			}

            public RecoilOverride(Recoiler recoiler, int priority, float recoilSpeed)
            {
                Recoiler = recoiler;
                _priority = priority;
                _recoilSpeed = recoilSpeed;
            }

            public int CompareTo(RecoilOverride other)
            {
				return other.Priority.CompareTo(Priority);
            }

            public void Dispose()
            {
				Recoiler.RemoveRecoilOverride(this);
            }
        }

		SortedSet<RecoilOverride> recoilOverrides = new SortedSet<RecoilOverride>();

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

		static Func<Recoil, float> recoilTotalSpeedDel;


		public float GetRecoilSpeed()
		{
			if (recoilSpeedDel == null)
			{
				recoilSpeedDel = ReflectionUtilities.CreateFieldGetter<Recoil, float>(typeof(Recoiler).BaseType.GetField("recoilSpeedBase", BindingFlags.NonPublic | BindingFlags.Instance));
			}

			return recoilSpeedDel.Invoke(this);
		}

		public RecoilOverride AddRecoilOverride(float recoilSpeed, int priority = 0)
		{
			var instance = new RecoilOverride(this, priority, recoilSpeed);
			recoilOverrides.Add(instance);
			RefreshRecoilSpeed();
			return instance;
        }

		public bool RemoveRecoilOverride(RecoilOverride recoilOverride)
		{
			if (recoilOverrides.Remove(recoilOverride))
			{
                RefreshRecoilSpeed();
				return true;
            }
			return false;
        }

		public void ClearRecoilOverrides()
		{
			recoilOverrides.Clear();
			RefreshRecoilSpeed();
		}

		private void RefreshRecoilSpeed()
		{
			if (recoilOverrides.Count > 0)
			{
				SetRecoilSpeed(recoilOverrides.First().RecoilSpeed);
			}
			else
			{
				SetRecoilSpeed(OriginalRecoilSpeed);
			}
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
			if (recoilTotalSpeedDel == null)
			{
				recoilTotalSpeedDel = ReflectionUtilities.CreateFieldGetter<Recoil, float>(typeof(Recoiler).BaseType.GetField("recoilSpeed", BindingFlags.NonPublic | BindingFlags.Instance));
			}

			//Divide the actual speed by the base speed to get the magnitude
			return recoilTotalSpeedDel.Invoke(this) / GetRecoilSpeed();
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

    static class Recoiler_Patches
    {
        static Func<Recoil, Rigidbody2D> bodyGetter;
        static Func<Recoil, Vector2> recoilDirectionGetter;
        static Func<Recoil, float> recoilSpeedGetter;
        static Func<Recoil, float> recoilTimeRemainingGetter;
        static Action<Recoil, float> recoilTimeRemainingSetter;
        static FieldInfo recoilStateField;
        static object recoilingState;

        static void EnsureReflection()
        {
            if (bodyGetter != null)
            {
                return;
            }

            var recoilType = typeof(Recoil);
            bodyGetter = ReflectionUtilities.CreateFieldGetter<Recoil, Rigidbody2D>(recoilType.GetField("body", BindingFlags.NonPublic | BindingFlags.Instance));
            recoilDirectionGetter = ReflectionUtilities.CreateFieldGetter<Recoil, Vector2>(recoilType.GetField("recoilDirection", BindingFlags.NonPublic | BindingFlags.Instance));
            recoilSpeedGetter = ReflectionUtilities.CreateFieldGetter<Recoil, float>(recoilType.GetField("recoilSpeed", BindingFlags.NonPublic | BindingFlags.Instance));
            recoilTimeRemainingGetter = ReflectionUtilities.CreateFieldGetter<Recoil, float>(recoilType.GetField("recoilTimeRemaining", BindingFlags.NonPublic | BindingFlags.Instance));
            recoilTimeRemainingSetter = ReflectionUtilities.CreateFieldSetter<Recoil, float>(recoilType.GetField("recoilTimeRemaining", BindingFlags.NonPublic | BindingFlags.Instance));
            recoilStateField = recoilType.GetField("state", BindingFlags.NonPublic | BindingFlags.Instance);
            recoilingState = Enum.Parse(recoilStateField.FieldType, "Recoiling");
        }

        static bool UpdatePhysics_Prefix(Recoil __instance, float deltaTime)
        {
            if (!(__instance is Recoiler))
            {
                return true;
            }

            try
            {
                EnsureReflection();

                if (!Equals(recoilStateField.GetValue(__instance), recoilingState))
                {
                    return true;
                }

                Vector2 movement = recoilDirectionGetter(__instance) * recoilSpeedGetter(__instance) * Time.deltaTime;
                var body = bodyGetter(__instance);

                if (body != null && body.simulated && body.bodyType != RigidbodyType2D.Static)
                {
                    body.MovePosition(body.position + movement);
                }
                else
                {
                    __instance.transform.Translate(movement, Space.World);
                }

                float remainingTime = recoilTimeRemainingGetter(__instance) - deltaTime;
                recoilTimeRemainingSetter(__instance, remainingTime);
                if (remainingTime <= 0f)
                {
                    __instance.CancelRecoil();
                }

                return false;
            }
            catch (Exception e)
            {
                WeaverLog.LogError($"Recoiler recoil MovePosition patch failed: {e}");
                return true;
            }
        }

        [OnHarmonyPatch]
        static void OnHarmonyPatch(HarmonyPatcher patcher)
        {
            try
            {
                EnsureReflection();

                var updatePhysics = typeof(Recoil).GetMethod("UpdatePhysics", BindingFlags.NonPublic | BindingFlags.Instance);
                var prefix = typeof(Recoiler_Patches).GetMethod(nameof(UpdatePhysics_Prefix), BindingFlags.NonPublic | BindingFlags.Static);

                patcher.Patch(updatePhysics, prefix, null);
            }
            catch (Exception e)
            {
                WeaverLog.LogError($"Failed to patch Recoiler recoil movement: {e}");
            }
        }
    }
}
