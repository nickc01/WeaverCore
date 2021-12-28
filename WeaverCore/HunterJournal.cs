using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Implementations;

namespace WeaverCore
{
	/// <summary>
	/// Used for recording entries into the hunter's journal
	/// </summary>
    public static class HunterJournal
	{
		/// <summary>
		/// Has the player killed this enemy at least once?
		/// </summary>
		/// <param name="name">The name of the enemy in the hunter's journal</param>
		/// <returns>Returns whether this player has killed the enemy before</returns>
		public static bool HasKilled(string name)
		{
			return HunterJournal.impl.HasKilled(name);
		}

		/// <summary>
		/// How many kills are left to fully unlock the enemy?
		/// </summary>
		/// <param name="name">The name of the enemy in the hunter's journal</param>
		/// <returns>Returns how many kills are left to fully unlock the enemy</returns>
		public static int KillsLeft(string name)
		{
			return HunterJournal.impl.KillsLeft(name);

		}

		/// <summary>
		/// Records a kill for the enemy
		/// </summary>
		/// <param name="name">The name of the enemy in the hunter's journal</param>
		public static void RecordKillFor(string name)
		{
			HunterJournal.impl.RecordKillFor(name);
		}

		/// <summary>
		/// Does the enemy entry exist in the hunter's journal?
		/// </summary>
		/// <param name="name">The name of the enemy in the hunter's journal</param>
		/// <returns>Returns whether the enemy entry exists in the hunter's journal</returns>
		public static bool HasEntryFor(string name)
		{
			return HunterJournal.impl.HasEntryFor(name);
		}

		/// <summary>
		/// Displays an icon at the bottom-right of the screen indicating the hunter's journal was updated
		/// </summary>
		/// <param name="displayText">Should the text "Journal Updated" also be displayed?</param>
		public static void DisplayJournalUpdate(bool displayText = false)
		{
			HunterJournal.impl.DisplayJournalUpdate(displayText);
		}

		private static HunterJournal_I impl = ImplFinder.GetImplementation<HunterJournal_I>();
	}
}
