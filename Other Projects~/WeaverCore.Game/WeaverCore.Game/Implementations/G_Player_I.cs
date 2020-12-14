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

        public override void RefreshSoulUI()
        {
            GameCameras.instance.soulVesselFSM.SendEvent("MP RESERVE DOWN");
            GameCameras.instance.soulVesselFSM.SendEvent("MP RESERVE UP");
            //GameCameras.instance.soulOrbFSM.SendEvent("MP LOSE");
            GameCameras.instance.soulOrbFSM.SendEvent("MP SET");
        }

        public override void SoulGain()
        {
            HeroController.instance.SoulGain();
        }
    }
}
