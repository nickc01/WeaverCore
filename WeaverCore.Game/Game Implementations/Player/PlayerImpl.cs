using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Helpers;

namespace WeaverCore.Game.Implementations
{
	public class GamePlayerImplementation : WeaverCore.Implementations.PlayerImplementation
    {
        public override void Initialize()
        {
            Debugger.Log("PLAYER START GAME");
        }
    }
}
