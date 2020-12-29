using System;
using UnityEngine;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	// Token: 0x0200006B RID: 107
	public abstract class HunterJournal_I : MonoBehaviour, IImplementation
	{
		// Token: 0x060001E6 RID: 486
		public abstract bool HasKilled(string name);

		// Token: 0x060001E7 RID: 487
		public abstract int KillsLeft(string name);

		// Token: 0x060001E8 RID: 488
		public abstract void RecordKillFor(string name);

		// Token: 0x060001E9 RID: 489
		public abstract void DisplayJournalUpdate(bool displayText);

		// Token: 0x060001EA RID: 490
		public abstract bool HasEntryFor(string name);
	}
}