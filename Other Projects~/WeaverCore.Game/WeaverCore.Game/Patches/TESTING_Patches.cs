using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Patches
{
    class TESTING_Patches
	{
        [OnInit]
        static void Init()
        {
            //On.GameManager.BeginSceneTransition += GameManager_BeginSceneTransition;
            //On.GameManager.SaveGame += GameManager_SaveGame;
            //On.HutongGames.PlayMaker.Actions.SendMessage.OnEnter += SendMessage_OnEnter;
        }

        /*private static void SendMessage_OnEnter(On.HutongGames.PlayMaker.Actions.SendMessage.orig_OnEnter orig, HutongGames.PlayMaker.Actions.SendMessage self)
        {
            WeaverLog.Log("SEND MESSAGE CALLED FROM OBJECT = " + self.gameObject.GameObject);
            WeaverLog.Log("SEND MESSAGE FSM = " + self.Fsm.Name);
            orig(self);
        }

        private static void GameManager_SaveGame(On.GameManager.orig_SaveGame orig, GameManager self)
        {
            WeaverLog.Log("SAVING GAME");
            WeaverLog.Log("STACK TRACE = " + new System.Diagnostics.StackTrace());
            orig(self);
        }

        private static void GameManager_BeginSceneTransition(On.GameManager.orig_BeginSceneTransition orig, GameManager self, GameManager.SceneLoadInfo info)
        {
            WeaverLog.Log("BEGINNING SCENE TRANSITION");
            foreach (var field in info.GetType().GetFields())
            {
                WeaverLog.Log($"{field.Name} = {field.GetValue(info)}");
            }

            orig(self, info);
        }*/
    }
}
