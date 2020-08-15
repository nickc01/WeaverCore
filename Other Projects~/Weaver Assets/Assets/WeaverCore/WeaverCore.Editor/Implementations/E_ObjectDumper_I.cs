using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Implementations;

namespace WeaverCore.Editor.Implementations
{
	public class E_ObjectDumper_I : ObjectDumper_I
	{
		public override void Dump(GameObject obj)
		{
			Debug.LogError("Dumping GameObjects in the Unity Editor is not allowed");
		}
	}
}
