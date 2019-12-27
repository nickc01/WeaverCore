using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViridianCore.Internal
{
    static class ErrorPatcher
    {
        [ModStart(typeof(ViridianCore))]
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

        [ModEnd(typeof(ViridianCore))]
        static void End()
        {
            On.MenuButtonList.DoSelect -= DoSelect;
        }
    }
}
