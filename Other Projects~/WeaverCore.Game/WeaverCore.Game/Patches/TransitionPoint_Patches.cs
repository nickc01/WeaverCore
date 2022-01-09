using System.Reflection;
using WeaverCore.Attributes;

namespace WeaverCore.Game.Patches
{
	static class TransitionPoint_Patches
	{
		static FieldInfo gmField;

		[OnInit]
		static void Init()
		{
			On.TransitionPoint.Awake += TransitionPoint_Awake;
			gmField = typeof(TransitionPoint).GetField("gm", BindingFlags.Instance | BindingFlags.NonPublic);
		}

		private static void TransitionPoint_Awake(On.TransitionPoint.orig_Awake orig, TransitionPoint self)
		{
			gmField.SetValue(self,GameManager.instance);
			orig(self);
		}
	}
}
