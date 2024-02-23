using System;
using System.Collections.Generic;
using WeaverCore.Attributes;
using WeaverCore.Interfaces;

namespace WeaverCore.Game.Patches
{
    static class SendDreamImpact_Patches
	{
        static List<IDreamnailable> dreamnailablesCache = new List<IDreamnailable>();

		[OnInit]
		static void Init()
		{
            On.SendDreamImpact.OnEnter += SendDreamImpact_OnEnter;
        }

        private static void SendDreamImpact_OnEnter(On.SendDreamImpact.orig_OnEnter orig, SendDreamImpact self)
        {
            orig(self);
            var gm = self.target.GetSafe(self);

            if (gm != null)
            {
                dreamnailablesCache.Clear();
                gm.GetComponents(dreamnailablesCache);
                if (dreamnailablesCache.Count > 0)
                {
                    foreach (var dreamnailable in dreamnailablesCache)
                    {
                        try
                        {
                            int soulAmount = dreamnailable.DreamnailHit(Player.Player1);

                            if (soulAmount > 0)
                            {
                                HeroController.instance.AddMPCharge(soulAmount);
                            }
                        }
                        catch (Exception e)
                        {
                            WeaverLog.LogError($"IDreamnailable {dreamnailable.GetType()} failed with an exception");
                            WeaverLog.LogException(e);
                        }
                    }
                }
            }
        }
    }
}
