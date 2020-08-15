using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Implementations;

namespace WeaverCore.Utilities
{
	public class UnboundCoroutine
	{
		static UnboundCoroutine_I impl = ImplFinder.GetImplementation<UnboundCoroutine_I>();

		protected UnboundCoroutine()
		{

		}


		public static UnboundCoroutine Start(IEnumerator routine)
		{
			return impl.Start(routine);
		}

		public static void Stop(UnboundCoroutine coroutine)
		{
			impl.Stop(coroutine);
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
	}
}
