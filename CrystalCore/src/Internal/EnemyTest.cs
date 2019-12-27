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
using CrystalCore.Helpers;
using CrystalCore.Hooks;
using CrystalCore.Machine;

class EnemyTest : EnemyHook<CrystalCore.CrystalCore>
{
	void Start()
	{
		ObjectDebugger.DebugObject(gameObject, nameof(CrystalCore.CrystalCore));
	}
}