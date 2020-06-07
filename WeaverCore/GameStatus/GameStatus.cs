using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore.GameStatus
{
	public static class GameStatus
	{
		public static RunningState GameState => ImplFinder.State;
	}
}
