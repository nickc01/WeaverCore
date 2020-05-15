using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore
{
	public interface IUAwaiter
	{
		bool KeepWaiting();
	}
}
