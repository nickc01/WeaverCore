using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Interfaces;

namespace WeaverCore.Awaiters
{
	/*public class WeaverYield : IWeaverAwaiter
	{
		IWeaverAwaiter[] Awaiters;
		int currentAwaiter = 0;

		public WeaverYield(params IWeaverAwaiter[] awaiters)
		{
			Awaiters = awaiters;
		}

		bool IWeaverAwaiter.KeepWaiting()
		{
			if (currentAwaiter == Awaiters.GetLength(0))
			{
				return false;
			}
			while (!Awaiters[currentAwaiter].KeepWaiting())
			{
				currentAwaiter++;
				if (currentAwaiter == Awaiters.GetLength(0))
				{
					return false;
				}
			}

			return true;
		}
	}*/
}
