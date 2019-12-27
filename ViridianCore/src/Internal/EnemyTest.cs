using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using Harmony;
using HutongGames.PlayMaker;
using UnityEngine;
using ViridianCore.Helpers;
using ViridianCore.Hooks;
using ViridianCore.Machine;

class EnemyTest : EnemyHook<ViridianCore.ViridianCore>
{
	void Start()
	{
		ObjectDebugger.DebugObject(gameObject, nameof(ViridianCore.ViridianCore));
	}
}