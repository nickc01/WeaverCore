using WeaverCore.Attributes;

namespace WeaverCore.Game.Patches
{
    static class FadeGroup_Patches
	{
		[OnInit]
		static void Init()
		{
            On.FadeGroup.FadeUp += FadeGroup_FadeUp;
            On.FadeGroup.FadeDown += FadeGroup_FadeDown;
            On.FadeGroup.FadeDownFast += FadeGroup_FadeDownFast;
		}

        private static void FadeGroup_FadeDownFast(On.FadeGroup.orig_FadeDownFast orig, FadeGroup self)
        {
            CheckAnimatorField(self);
            orig(self);
        }

        private static void FadeGroup_FadeDown(On.FadeGroup.orig_FadeDown orig, FadeGroup self)
        {
            CheckAnimatorField(self);
            orig(self);
        }

        private static void FadeGroup_FadeUp(On.FadeGroup.orig_FadeUp orig, FadeGroup self)
        {
            CheckAnimatorField(self);
            orig(self);
        }

        static void CheckAnimatorField(FadeGroup self)
        {
            if (self.animators == null)
            {
                self.animators = new InvAnimateUpAndDown[];
            }
        }
    }
}
