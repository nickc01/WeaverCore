using System;
using UnityEngine;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	public abstract class HunterJournal_I : MonoBehaviour, IImplementation
	{
		public abstract bool HasKilled(string name);

		public abstract int KillsLeft(string name);

		public abstract void RecordKillFor(string name);

		public abstract void DisplayJournalUpdate(bool displayText);

		public abstract bool HasEntryFor(string name);
	}
}