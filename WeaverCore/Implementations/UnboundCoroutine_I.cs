using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Implementations
{
	public abstract class UnboundCoroutine_I : IImplementation
	{
		public abstract UnboundCoroutine Start(IEnumerator routine);
		public abstract void Stop(UnboundCoroutine routine);
		public abstract float DT { get; }
	}
}
