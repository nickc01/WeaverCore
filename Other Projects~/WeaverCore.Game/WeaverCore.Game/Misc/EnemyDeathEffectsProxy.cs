using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using WeaverCore;
using WeaverCore.Attributes;
using WeaverCore.Components;
using WeaverCore.Interfaces;



public class EnemyDeathEffectsProxy : EnemyDeathEffects
{
	[OnHarmonyPatch]
	static void ApplyPatches(HarmonyPatcher patcher)
	{
		On.EnemyDeathEffects.RecieveDeathEvent += EnemyDeathEffects_RecieveDeathEvent;
		On.EnemyDeathEffects.EmitSound += EnemyDeathEffects_EmitSound;
	}

	private static void EnemyDeathEffects_EmitSound(On.EnemyDeathEffects.orig_EmitSound orig, EnemyDeathEffects self)
	{
		if (self is EnemyDeathEffectsProxy)
		{
			var proxy = (EnemyDeathEffectsProxy)self;
			self.GetComponent<IDeathEffects>().EmitSounds();
		}
		else
		{
			orig(self);
		}
	}

	delegate void RecieveDeathDelegate(ModHooks instance, EnemyDeathEffects enemyDeathEffects, bool eventAlreadyRecieved, ref float? attackDirection, ref bool resetDeathEvent, ref bool spellBurn, ref bool isWatery);

	static RecieveDeathDelegate DeathEventFunc;
	static FieldInfo DidFireField;

	EntityHealth health;

	void Awake()
	{
		health = GetComponent<EntityHealth>();
	}

	private static void EnemyDeathEffects_RecieveDeathEvent(On.EnemyDeathEffects.orig_RecieveDeathEvent orig, EnemyDeathEffects self, float? attackDirection, bool resetDeathEvent, bool spellBurn, bool isWatery)
	{
		if (self is EnemyDeathEffectsProxy)
		{
			var proxy = (EnemyDeathEffectsProxy)self;
			if (DeathEventFunc == null)
			{
				var deathEventMethod = typeof(ModHooks).GetMethod("OnRecieveDeathEvent", BindingFlags.Instance | BindingFlags.NonPublic);
				DeathEventFunc = (RecieveDeathDelegate)Delegate.CreateDelegate(typeof(RecieveDeathDelegate), deathEventMethod);
				DidFireField = typeof(EnemyDeathEffects).GetField("didFire", BindingFlags.Instance | BindingFlags.NonPublic);
			}
			DeathEventFunc.Invoke(null, self, (bool)DidFireField.GetValue(self),ref attackDirection,ref resetDeathEvent,ref spellBurn,ref isWatery);
			self.GetComponent<IDeathEffects>().PlayDeathEffects(proxy.health.LastAttackInfo);
		}
		else
		{
			orig(self,attackDirection,resetDeathEvent,spellBurn,isWatery);
		}
	}
}

