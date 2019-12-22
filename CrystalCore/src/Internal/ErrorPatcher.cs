using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrystalCore.Internal
{
    static class ErrorPatcher
    {
        [ModStart(typeof(CrystalCore))]
        static void Start()
        {
            On.MenuButtonList.DoSelect += DoSelect;
        }

        private static void DoSelect(On.MenuButtonList.orig_DoSelect orig, MenuButtonList self)
        {
            if (self.gameObject.activeSelf)
            {
                orig(self);
            }
        }

        [ModEnd(typeof(CrystalCore))]
        static void End()
        {
            On.MenuButtonList.DoSelect -= DoSelect;
        }
    }
}
