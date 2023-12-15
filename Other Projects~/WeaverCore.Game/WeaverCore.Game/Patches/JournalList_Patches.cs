using WeaverCore.Attributes;

namespace WeaverCore.Game.Patches
{
    static class JournalList_Patches
	{
		[OnInit]
		static void OnInit()
		{
            On.JournalList.BuildEnemyList += JournalList_BuildEnemyList;
            On.JournalList.UpdateEnemyList += JournalList_UpdateEnemyList;
        }

        private static void JournalList_UpdateEnemyList(On.JournalList.orig_UpdateEnemyList orig, JournalList self)
        {
            WeaverLog.Log("UPDATE ENEMY LIST CALLED");
            orig(self);
        }

        private static void JournalList_BuildEnemyList(On.JournalList.orig_BuildEnemyList orig, JournalList self)
        {
            WeaverLog.Log("BUILD ENEMY LIST CALLED");
            orig(self);
        }


    }
}
