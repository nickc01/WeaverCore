using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WeaverCore.Implementations;
using Harmony;
using WeaverCore.Utilities;
using WeaverCore.Internal;
using HutongGames.PlayMaker.Actions;
using WeaverCore.Game.Implementations;
using WeaverCore.Interfaces;
using System.Reflection;
using System.Linq;

namespace WeaverCore.Game
{
    class GameInitializer : IInit
    {
        void IInit.OnInit()
        {
            ModLoader.LoadAllMods();
			RuntimeInitializer.RuntimeInit();
        }
	}
}
