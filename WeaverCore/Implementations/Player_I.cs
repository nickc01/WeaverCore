using System;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	// Token: 0x0200006E RID: 110
	public abstract class Player_I : MonoBehaviour, IImplementation
	{
		// Token: 0x060001F3 RID: 499
		public abstract void Initialize();

		// Token: 0x060001F4 RID: 500
		public abstract void SoulGain();

		// Token: 0x060001F5 RID: 501
		public abstract void RefreshSoulUI();

		// Token: 0x060001F6 RID: 502
		public abstract void EnterParryState();

		// Token: 0x060001F7 RID: 503
		public abstract void RecoverFromParry();

		// Token: 0x060001F8 RID: 504
		public abstract void Recoil(CardinalDirection direction);

		// Token: 0x1700005D RID: 93
		// (get) Token: 0x060001F9 RID: 505
		public abstract bool HasDreamNail { get; }

		// Token: 0x060001FA RID: 506
		public abstract bool HasCharmEquipped(int charmNumber);

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x060001FB RID: 507
		// (set) Token: 0x060001FC RID: 508
		public abstract int EssenceCollected { get; set; }

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x060001FD RID: 509
		// (set) Token: 0x060001FE RID: 510
		public abstract int EssenceSpent { get; set; }
	}
}
