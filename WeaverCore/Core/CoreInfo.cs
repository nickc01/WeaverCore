using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore
{
	public static class CoreInfo
	{
		public static RunningState LoadState
		{
			get
			{
				return ImplFinder.State;
			}
		}
	}
}
