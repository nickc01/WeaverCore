using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Components;
using WeaverCore.Enums;
using WeaverCore.Game;
using WeaverCore.Utilities;

/// <summary>
/// Serves as a way of having a health manager by syncing it's properties with WeaverCore's EntityHealth and vice versa
/// </summary>
public class HealthManagerProxy : HealthManager
{
	EntityHealth weaverHealth;

	int previousHP;

	static MethodInfo GetProxyMethod(string name)
	{
		return typeof(HealthManagerProxy).GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
	}

	[OnHarmonyPatch]
	static void ApplyPatches(HarmonyPatcher patcher)
	{
		On.HealthManager.Awake += HealthManager_Awake;
		On.HealthManager.OnEnable += HealthManager_OnEnable;
		On.HealthManager.Hit += HealthManager_Hit;
		On.HealthManager.ApplyExtraDamage += HealthManager_ApplyExtraDamage;
		On.HealthManager.Die += HealthManager_Die;
		On.HealthManager.SendDeathEvent += HealthManager_SendDeathEvent;
		On.HealthManager.IsBlockingByDirection += HealthManager_IsBlockingByDirection;
		On.HealthManager.GetAttackDirection += HealthManager_GetAttackDirection;
		On.HealthManager.GetIsDead += HealthManager_GetIsDead;
		On.HealthManager.CheckInvincible += HealthManager_CheckInvincible;
		//On.HealthManager.CheckPersistence += HealthManager_CheckPersistence;

		var invisProp = typeof(HealthManager).GetProperty("IsInvincible");

		patcher.Patch(invisProp.GetGetMethod(), GetProxyMethod("InvisGetPrefix"),null);
		patcher.Patch(invisProp.GetSetMethod(), GetProxyMethod("InvisSetPrefix"),null);
	}

	private static bool HealthManager_CheckInvincible(On.HealthManager.orig_CheckInvincible orig, HealthManager self)
	{
		if (self is HealthManagerProxy)
		{
			var proxy = (HealthManagerProxy)self;
			return proxy.weaverHealth.Invincible;
		}
		else
		{
			return orig(self);
		}
	}

	private static bool HealthManager_GetIsDead(On.HealthManager.orig_GetIsDead orig, HealthManager self)
	{
		if (self is HealthManagerProxy)
		{
			return ((HealthManagerProxy)self).weaverHealth.Health == 0;
		}
		else
		{
			return orig(self);
		}
	}

	private static int HealthManager_GetAttackDirection(On.HealthManager.orig_GetAttackDirection orig, HealthManager self)
	{
		if (self is HealthManagerProxy)
		{
			var proxy = (HealthManagerProxy)self;
			switch (proxy.weaverHealth.LastAttackDirection)
			{
				case CardinalDirection.Up:
					return 1;
				case CardinalDirection.Down:
					return 3;
				case CardinalDirection.Left:
					return 2;
				case CardinalDirection.Right:
					return 0;
				default:
					return 0;
			}
		}
		else
		{
			return orig(self);
		}
	}

	private static bool HealthManager_IsBlockingByDirection(On.HealthManager.orig_IsBlockingByDirection orig, HealthManager self, int cardinalDirection, AttackTypes attackType)
	{
		if (self is HealthManagerProxy)
		{
			var proxy = (HealthManagerProxy)self;
			if ((attackType == AttackTypes.Spell || attackType == AttackTypes.SharpShadow) && proxy.gameObject.CompareTag("Spell Vulnerable"))
			{
				return false;
			}
			if (!proxy.weaverHealth.Invincible)
			{
				return false;
			}
			if (proxy.InvincibleFromDirection == 0)
			{
				return true;
			}
			switch (cardinalDirection)
			{
				case 0:
					{
						int num = proxy.InvincibleFromDirection;
						switch (num)
						{
							case 5:
							case 8:
							case 10:
								break;
							default:
								if (num != 1)
								{
									return false;
								}
								break;
						}
						return true;
					}
				case 1:
					switch (proxy.InvincibleFromDirection)
					{
						case 2:
						case 5:
						case 6:
						case 7:
						case 8:
						case 9:
							return true;
					}
					return false;
				case 2:
					{
						int num2 = proxy.InvincibleFromDirection;
						switch (num2)
						{
							case 3:
							case 6:
								break;
							default:
								switch (num2)
								{
									case 9:
									case 11:
										return true;
								}
								return false;
						}
						return true;
					}
				case 3:
					switch (proxy.InvincibleFromDirection)
					{
						case 4:
						case 7:
						case 8:
						case 9:
						case 10:
						case 11:
							return true;
					}
					return false;
				default:
					return false;
			}
		}
		else
		{
			return orig(self,cardinalDirection,attackType);
		}
	}

	private static void HealthManager_SendDeathEvent(On.HealthManager.orig_SendDeathEvent orig, HealthManager self)
	{
		if (self is HealthManagerProxy)
		{
			var proxy = (HealthManagerProxy)self;
			proxy.weaverHealth.DoDeathEvent();
		}
		orig(self);
	}

	private static void HealthManager_Die(On.HealthManager.orig_Die orig, HealthManager self, float? attackDirection, AttackTypes attackType, bool ignoreEvasion)
	{
		if (self is HealthManagerProxy)
		{
			var proxy = (HealthManagerProxy)self;
			proxy.weaverHealth.Die();
		}
		else
		{
			orig(self, attackDirection, attackType, ignoreEvasion);
		}
	}

	private static void HealthManager_ApplyExtraDamage(On.HealthManager.orig_ApplyExtraDamage orig, HealthManager self, int damageAmount)
	{
		if (self is HealthManagerProxy)
		{
			var proxy = (HealthManagerProxy)self;
			proxy.weaverHealth.Health -= damageAmount;
		}
		else
		{
			orig(self, damageAmount);
		}
	}

	private static void HealthManager_Hit(On.HealthManager.orig_Hit orig, HealthManager self, HitInstance hitInstance)
	{
		if (self is HealthManagerProxy)
		{
			var proxy = (HealthManagerProxy)self;
			proxy.weaverHealth.Hit(Misc.ConvertHitInstance(hitInstance));
		}
		else
		{
			orig(self,hitInstance);
		}
	}

	private static void HealthManager_OnEnable(On.HealthManager.orig_OnEnable orig, HealthManager self)
	{
		if (!(self is HealthManagerProxy))
		{
			orig(self);
		}
	}

	private static void HealthManager_Awake(On.HealthManager.orig_Awake orig, HealthManager self)
	{
		if (self is HealthManagerProxy)
		{
			var proxy = (HealthManagerProxy)self;
			proxy.ProxyAwake();
		}
		else
		{
			orig(self);
		}
	}

	static bool InvisGetPrefix(ref bool __result, HealthManager __instance)
	{
		if (__instance is HealthManagerProxy)
		{
			__result = ((HealthManagerProxy)__instance).weaverHealth.Invincible;
			return false;
		}
		else
		{
			return true;
		}
	}

	static bool InvisSetPrefix(HealthManager __instance, bool value)
	{
		if (__instance is HealthManagerProxy)
		{
			((HealthManagerProxy)__instance).weaverHealth.Invincible = value;
			return false;
		}
		else
		{
			return true;
		}
	}

	void ProxyAwake()
	{
		weaverHealth = GetComponent<EntityHealth>();

		hp = weaverHealth.Health;
		previousHP = weaverHealth.Health;

		weaverHealth.OnHealthChangeEvent += WeaverHealth_OnHealthChangeEvent;

		
	}

	private void WeaverHealth_OnHealthChangeEvent(int oldHealth, int newHealth)
	{
		hp = newHealth;
		previousHP = newHealth;
	}

	void LateUpdate()
	{
		if (hp != previousHP)
		{
			previousHP = hp;
			weaverHealth.Health = previousHP;
		}
	}
}

