using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Game.Implementations
{
	public class G_Player_I : WeaverCore.Implementations.Player_I
    {
        public override void Initialize()
        {
            //Debugger.Log("PLAYER START GAME");
        }

        public override void SoulGain()
        {
            HeroController.instance.SoulGain();
        }
    }
}
