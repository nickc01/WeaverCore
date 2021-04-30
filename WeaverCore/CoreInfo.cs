/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore
{
	public static class CoreInfo
	{
#if UNITY_EDITOR
		public static readonly RunningState LoadState = ImplFinder.State;
#else
		public const RunningState LoadState = ImplFinder.State;
#endif
	}
}
*/