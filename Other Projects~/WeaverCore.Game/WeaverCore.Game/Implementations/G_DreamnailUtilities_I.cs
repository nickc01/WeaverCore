using HutongGames.PlayMaker;
using WeaverCore.Implementations;

namespace WeaverCore.Game.Implementations
{
    public class G_DreamnailUtilities_I : DreamnailUtilities_I
    {
        public override void DisplayEnemyDreamMessage(int convoAmount, string convoTitle)
        {
            PlayMakerFSM playMakerFSM = PlayMakerFSM.FindFsmOnGameObject(FsmVariables.GlobalVariables.GetFsmGameObject("Enemy Dream Msg").Value, "Display");
            playMakerFSM.FsmVariables.GetFsmInt("Convo Amount").Value = convoAmount;
            playMakerFSM.FsmVariables.GetFsmString("Convo Title").Value = convoTitle;
            playMakerFSM.SendEvent("DISPLAY ENEMY DREAM");
        }
    }
}