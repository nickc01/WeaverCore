using System;
using ViridianLink.Editor.Helpers;
using ViridianLink.Extras;

namespace ViridianLink.Editor.Routines
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
