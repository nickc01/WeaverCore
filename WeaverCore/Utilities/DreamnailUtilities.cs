using WeaverCore.Implementations;

namespace WeaverCore.Utilities
{
    public static class DreamnailUtilities
	{
		static DreamnailUtilities_I _impl;

		static DreamnailUtilities_I Impl => _impl ??= ImplFinder.GetImplementation<DreamnailUtilities_I>();

		public static void DisplayEnemyDreamnailMessage(int convoAmount, string convoTitle)
		{
			Impl.DisplayEnemyDreamMessage(convoAmount, convoTitle);

        }
	}
}
