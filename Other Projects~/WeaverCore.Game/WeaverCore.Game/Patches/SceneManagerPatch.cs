using System.Reflection;
using WeaverCore.Attributes;

namespace WeaverCore.Game.Patches
{
	static class SceneManagerPatch
	{
		[OnInit]
		static void Init()
		{
			On.SceneManager.Start += SceneManager_Start;
			On.TransitionPoint.Start += TransitionPoint_Start;
		}

		private static void TransitionPoint_Start(On.TransitionPoint.orig_Start orig, TransitionPoint self)
		{
			orig(self);
			WeaverLog.Log("Printing Transition Point Info");
			foreach (var field in typeof(TransitionPoint).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				WeaverLog.Log($"{field.Name} = {field.GetValue(self)}");
			}
		}

		private static void SceneManager_Start(On.SceneManager.orig_Start orig, SceneManager self)
		{
			orig(self);
			WeaverLog.Log("Printing Scene Manager Info");
			foreach (var field in typeof(SceneManager).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				WeaverLog.Log($"{field.Name} = {field.GetValue(self)}");
			}
		}
	}
}
