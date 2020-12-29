using System;
using WeaverCore.Implementations;

namespace WeaverCore.Editor.Implementations
{
	public class E_HunterJournal_I : HunterJournal_I
    {
        public override void DisplayJournalUpdate(bool displayText)
        {
            //EDITOR DEFAULT
        }

        public override bool HasEntryFor(string name)
        {
            //EDITOR DEFAULT
            return false;
        }

        public override bool HasKilled(string name)
        {
            //EDITOR DEFAULT
            return false;
        }

        public override int KillsLeft(string name)
        {
            //EDITOR DEFAULT
            return 0;
        }

        public override void RecordKillFor(string name)
        {
            //EDITOR DEFAULT
        }
    }
}