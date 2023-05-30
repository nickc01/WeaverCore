using WeaverCore.Attributes;
using WeaverCore.Components;

namespace WeaverCore.Game.Patches
{
    public static class BeginSceneTransition_Patches
	{
        [OnInit]
        static void Init()
        {
            On.HutongGames.PlayMaker.Actions.BeginSceneTransition.OnEnter += BeginSceneTransition_OnEnter;
        }

        private static void BeginSceneTransition_OnEnter(On.HutongGames.PlayMaker.Actions.BeginSceneTransition.orig_OnEnter orig, HutongGames.PlayMaker.Actions.BeginSceneTransition self)
        {
            string oldSceneName = null;
            if (GameManager.instance.sm is WeaverSceneManager wsm && !string.IsNullOrEmpty(wsm.DreamReturnGateName) && self.entryGateName.Value == "door_dreamReturn")
            {
                oldSceneName = self.entryGateName.Value;
                self.entryGateName.Value = wsm.DreamReturnGateName;
            }

            orig(self);

            if (oldSceneName != null)
            {
                self.entryGateName.Value = oldSceneName;
            }
        }
    }
}
