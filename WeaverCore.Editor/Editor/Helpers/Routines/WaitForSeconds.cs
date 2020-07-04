using System;
using WeaverCore.Editor.Helpers;

namespace WeaverCore.Editor.Routines
{
	public class WaitForSeconds : IEditorWaiter
	{
		float AmountOfTime;
		float ElapsedTime = 0;

		public WaitForSeconds(float amountOfTime)
		{
			AmountOfTime = amountOfTime;
		}

		public bool KeepWaiting(float dt)
		{
			ElapsedTime += dt;
			return ElapsedTime < AmountOfTime;
		}
	}
}
