using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Implementations;

namespace WeaverCore.Utilities
{
	/// <summary>
	/// Used for starting/stopping coroutines that aren't bound to a specific GameObject
	/// </summary>
    public class UnboundCoroutine
	{
		static UnboundCoroutine_I impl = ImplFinder.GetImplementation<UnboundCoroutine_I>();

		protected UnboundCoroutine() { }

		/// <summary>
		/// Starts a coroutine that isn't bound to a specific GameObject
		/// </summary>
		/// <param name="routine">The coroutine function to start</param>
		/// <returns>Returns an unbound coroutine object. Use <see cref="Stop(UnboundCoroutine)"/> to stop the coroutine</returns>
		public static UnboundCoroutine Start(IEnumerator routine)
		{
			return impl.Start(routine);
		}


		/// <summary>
		/// Stops an unbound coroutine
		/// </summary>
		/// <param name="coroutine">The coroutine to stop</param>
		public static void Stop(UnboundCoroutine coroutine)
		{
			impl.Stop(coroutine);
		}

		/// <summary>
		/// Stops this unbound coroutine
		/// </summary>
		public void Stop()
		{
			Stop(this);
		}

		/// <summary>
		/// Used for accessing Time.deltaTime. This also works when not in play mode
		/// </summary>
		public static float DT
		{
			get
			{
				return impl.DT;
			}
		}
	}
}
