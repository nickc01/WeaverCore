using System;
using WeaverCore.Editor.Helpers;

namespace WeaverCore.Editor.Routines
{
	public class WaitTillTrue : IEditorWaiter
	{
		Func<bool> Predicate;

		public WaitTillTrue(Func<bool> predicate)
		{
			Predicate = predicate;
		}

		public bool Continue(float dt)
		{
			return Predicate();
		}
	}
}
