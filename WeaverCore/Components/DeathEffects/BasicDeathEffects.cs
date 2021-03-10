using System;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Features;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Components.DeathEffects
{
	// Token: 0x0200001E RID: 30
	public class BasicDeathEffects : MonoBehaviour, IDeathEffects
	{
		// Token: 0x1700001B RID: 27
		// (get) Token: 0x0600008F RID: 143 RVA: 0x00003E05 File Offset: 0x00002005
		// (set) Token: 0x06000090 RID: 144 RVA: 0x00003E0D File Offset: 0x0000200D
		public bool RanOnce { get; protected set; }

		// Token: 0x06000091 RID: 145 RVA: 0x00003E16 File Offset: 0x00002016
		protected virtual void Awake()
		{
			this.OnValidate();
		}

		// Token: 0x06000092 RID: 146 RVA: 0x00003E1E File Offset: 0x0000201E
		protected virtual void OnValidate()
		{
			if (this.EssenceCollectPrefab == null)
			{
				this.EssenceCollectPrefab = WeaverAssets.LoadWeaverAsset<GameObject>("Corpse Dream Essence");
			}
		}

		// Token: 0x06000093 RID: 147 RVA: 0x00002D1A File Offset: 0x00000F1A
		public virtual void EmitEffects()
		{
		}

		// Token: 0x06000094 RID: 148 RVA: 0x00003E44 File Offset: 0x00002044
		public virtual void PlayDeathEffects(HitInfo lastHit)
		{
			if (this.RanOnce)
			{
				return;
			}
			this.RanOnce = true;
			if (HunterJournal.HasEntryFor(this.JournalEntryName))
			{
				HunterJournal.RecordKillFor(this.JournalEntryName);
			}
			if (lastHit.AttackType != AttackType.Acid && lastHit.AttackType != AttackType.RuinsWater)
			{
				this.EmitEffects();
			}
			if (this.FreezeGameOnDeath)
			{
				WeaverGameManager.FreezeGameTime(WeaverGameManager.TimeFreezePreset.Preset1);
			}
			if (!Boss.InGodHomeArena && this.doEssenceChance)
			{
				this.DoEssenceChance();
			}
		}

		// Token: 0x06000095 RID: 149 RVA: 0x00003ECC File Offset: 0x000020CC
		public bool DoEssenceChance()
		{
			Player player = Player.Player1;
			if (!Player.Player1.HasDreamNail)
			{
				return false;
			}
			int max;
			if (player.HasCharmEquipped(30) && player.EssenceSpent > 0)
			{
				max = 40;
			}
			else if (player.HasCharmEquipped(30) && player.EssenceSpent <= 0)
			{
				max = 200;
			}
			else if (player.EssenceSpent <= 0)
			{
				max = 300;
			}
			else
			{
				max = 60;
			}
			if (UnityEngine.Random.Range(0, max) == 0)
			{
				this.EmitEssenceParticles();
				player.EssenceCollected++;
				player.EssenceSpent--;
				return true;
			}
			return false;
		}

		// Token: 0x06000096 RID: 150 RVA: 0x00003F80 File Offset: 0x00002180
		protected void EmitEssenceParticles()
		{
			UnityEngine.Object.Instantiate<GameObject>(this.EssenceCollectPrefab, base.transform.position + this.EffectsOffset, Quaternion.identity);
		}

		// Token: 0x0400006A RID: 106
		public string JournalEntryName;

		// Token: 0x0400006B RID: 107
		public bool FreezeGameOnDeath;

		// Token: 0x0400006C RID: 108
		[Space]
		[SerializeField]
		protected Vector3 EffectsOffset;

		// Token: 0x0400006D RID: 109
		[HideInInspector]
		[SerializeField]
		private GameObject EssenceCollectPrefab;

		// Token: 0x0400006E RID: 110
		[SerializeField]
		[Tooltip("When set to true, will cause the enemy to rarely give the player 1 essence upon death")]
		private bool doEssenceChance = true;
	}
}
