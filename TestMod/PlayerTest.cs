using ViridianCore.Helpers;
using ViridianCore.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class PlayerTest : PlayerHook<TestMod.TestMod>
{
    void Start()
    {
        ObjectDebugger.DebugObject(gameObject, nameof(ViridianCore.ViridianCore));
    }
}
