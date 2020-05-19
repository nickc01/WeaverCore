using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Interfaces;

namespace WeaverCore.Awaiters
{
	public class WaitTillTrue : IUAwaiter
	{
		Func<bool> Delegate;

		public WaitTillTrue(Func<bool> Delegate)
		{
			this.Delegate = Delegate;
		}

		public bool KeepWaiting()
		{
			return !Delegate();
		}
	}
}
