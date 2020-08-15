using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;

namespace WeaverCore.Utilities
{
	/// <summary>
	/// WeaverRoutine is a special type of coroutine system. It's main benefits are:
	/// 1: It does not have to be bound to a specific GameObject to run
	/// 2: It can be run even when not in play mode
	/// </summary>
	/*public class WeaverRoutine
	{
		static WeaverRoutine_I impl = ImplFinder.GetImplementation<WeaverRoutine_I>();

		WeaverRoutine_I.URoutineData data;

		WeaverRoutine(WeaverRoutine_I.URoutineData Data)
		{
			Data = data;
		}

		public static WeaverRoutine Start(IEnumerator<IWeaverAwaiter> function)
		{
			return new WeaverRoutine(impl.Start(function));
		}

		public static void Stop(WeaverRoutine routine)
		{
			impl.Stop(routine.data);
		}

		public void Stop()
		{
			Stop(this);
		}

		public static float DT
		{
			get
			{
				return impl.DT;
			}
		}


	}*/
}
