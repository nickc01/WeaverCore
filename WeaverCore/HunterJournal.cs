using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Implementations;

namespace WeaverCore
{
    public static class HunterJournal
	{
		// Token: 0x060000B6 RID: 182 RVA: 0x00004CC7 File Offset: 0x00002EC7
		public static bool HasKilled(string name)
		{
			return HunterJournal.impl.HasKilled(name);
		}

		// Token: 0x060000B7 RID: 183 RVA: 0x00004CD4 File Offset: 0x00002ED4
		public static int KillsLeft(string name)
		{
			return HunterJournal.impl.KillsLeft(name);

		}

		// Token: 0x060000B8 RID: 184 RVA: 0x00004CE1 File Offset: 0x00002EE1
		public static void RecordKillFor(string name)
		{
			HunterJournal.impl.RecordKillFor(name);
		}

		// Token: 0x060000B9 RID: 185 RVA: 0x00004CEE File Offset: 0x00002EEE
		public static bool HasEntryFor(string name)
		{
			return HunterJournal.impl.HasEntryFor(name);
		}

		// Token: 0x060000BA RID: 186 RVA: 0x00004CFB File Offset: 0x00002EFB
		public static void DisplayJournalUpdate(bool displayText = false)
		{
			HunterJournal.impl.DisplayJournalUpdate(displayText);
		}

		// Token: 0x040000A0 RID: 160
		private static HunterJournal_I impl = ImplFinder.GetImplementation<HunterJournal_I>();
	}
}
