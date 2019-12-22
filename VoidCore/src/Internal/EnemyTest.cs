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
using VoidCore.Helpers;
using VoidCore.Hooks;
using VoidCore.Machine;

class EnemyTest : EnemyHook<VoidCore.VoidCore>
{
	void Start()
	{
		ObjectDebugger.DebugObject(gameObject, "VoidCore");
	}
}