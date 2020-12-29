using System;
using WeaverCore.Enums;
using WeaverCore.Implementations;

namespace WeaverCore.Game.Implementations
{
	// Token: 0x02000022 RID: 34
	public class G_Player_I : Player_I
	{
		// Token: 0x1700000C RID: 12
		// (get) Token: 0x0600009B RID: 155 RVA: 0x0000438C File Offset: 0x0000258C
		public override bool HasDreamNail
		{
			get
			{
				return GameManager.instance.playerData.GetBool("hasDreamNail");
			}
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x0600009C RID: 156 RVA: 0x000043B4 File Offset: 0x000025B4
		// (set) Token: 0x0600009D RID: 157 RVA: 0x000043DA File Offset: 0x000025DA
		public override int EssenceCollected
		{
			get
			{
				return GameManager.instance.playerData.GetInt("dreamOrbs");
			}
			set
			{
				GameManager.instance.playerData.SetInt("dreamOrbs", value);
			}
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x0600009E RID: 158 RVA: 0x000043F4 File Offset: 0x000025F4
		// (set) Token: 0x0600009F RID: 159 RVA: 0x0000441A File Offset: 0x0000261A
		public override int EssenceSpent
		{
			get
			{
				return GameManager.instance.playerData.GetInt("dreamOrbsSpent");
			}
			set
			{
				GameManager.instance.playerData.SetInt("dreamOrbsSpend", value);
			}
		}

		// Token: 0x060000A0 RID: 160 RVA: 0x00004433 File Offset: 0x00002633
		public override void EnterParryState()
		{
			HeroController.instance.NailParry();
		}

		// Token: 0x060000A1 RID: 161 RVA: 0x00004441 File Offset: 0x00002641
		public override bool HasCharmEquipped(int charmNumber)
		{
			throw new NotImplementedException();
		}

		// Token: 0x060000A2 RID: 162 RVA: 0x00004449 File Offset: 0x00002649
		public override void Initialize()
		{
		}

		// Token: 0x060000A3 RID: 163 RVA: 0x0000444C File Offset: 0x0000264C
		public override void Recoil(CardinalDirection direction)
		{
			switch (direction)
			{
				case CardinalDirection.Up:
					HeroController.instance.Bounce();
					break;
				case CardinalDirection.Down:
					HeroController.instance.RecoilDown();
					break;
				case CardinalDirection.Left:
					HeroController.instance.RecoilLeft();
					break;
				default:
					HeroController.instance.RecoilRight();
					break;
			}
		}

		// Token: 0x060000A4 RID: 164 RVA: 0x000044A6 File Offset: 0x000026A6
		public override void RecoverFromParry()
		{
			HeroController.instance.NailParryRecover();
		}

		// Token: 0x060000A5 RID: 165 RVA: 0x000044B4 File Offset: 0x000026B4
		public override void RefreshSoulUI()
		{
			GameCameras.instance.soulVesselFSM.SendEvent("MP RESERVE DOWN");
			GameCameras.instance.soulVesselFSM.SendEvent("MP RESERVE UP");
			GameCameras.instance.soulOrbFSM.SendEvent("MP SET");
		}

		// Token: 0x060000A6 RID: 166 RVA: 0x00004501 File Offset: 0x00002701
		public override void SoulGain()
		{
			HeroController.instance.SoulGain();
		}
	}
}
