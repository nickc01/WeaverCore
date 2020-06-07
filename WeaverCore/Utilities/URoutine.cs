using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;

namespace WeaverCore.Utilities
{
	/// <summary>
	/// A URoutine (or Universal Routine) is a coroutine system that works in both hollow knight and in the unity editor, and can be called at any time
	/// </summary>
	public class URoutine
	{
		static URoutine_I impl = ImplFinder.GetImplementation<URoutine_I>();

		URoutine_I.URoutineData data;

		URoutine(URoutine_I.URoutineData Data)
		{
			Data = data;
		}

		public static URoutine Start(IEnumerator<IUAwaiter> function)
		{
			return new URoutine(impl.Start(function));
		}

		public static void Stop(URoutine routine)
		{
			impl.Stop(routine.data);
		}

		public void Stop()
		{
			Stop(this);
		}

		public static float DT => impl.DT;


	}
}
