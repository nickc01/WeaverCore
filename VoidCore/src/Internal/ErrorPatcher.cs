using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoidCore.Internal
{
    static class ErrorPatcher
    {
        [ModStart(typeof(VoidCore))]
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

        [ModEnd(typeof(VoidCore))]
        static void End()
        {
            On.MenuButtonList.DoSelect -= DoSelect;
        }
    }
}
