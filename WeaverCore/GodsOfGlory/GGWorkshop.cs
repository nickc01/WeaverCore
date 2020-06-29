using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Implementations;
using WeaverCore.Utilities;

namespace WeaverCore.GodsOfGlory
{
	public static class GGWorkshop
	{
		static GGWorkshop_I impl = ImplFinder.GetImplementation<GGWorkshop_I>();


		public static void ChangeStatue(string statueGMName, string bossName = "", string bossDescription = "")
		{
			impl.ChangeStatue(statueGMName, bossName, bossDescription);
		}
	}
}
