using CrystalCore.Helpers;
using CrystalCore.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class PlayerTest : PlayerHook<CrystalCore.CrystalCore>
{
    void Start()
    {
        ObjectDebugger.DebugObject(gameObject, nameof(CrystalCore.CrystalCore));
    }
}
