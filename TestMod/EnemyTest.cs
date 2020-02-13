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
using HutongGames.PlayMaker;
using UnityEngine;
using ViridianCore.Helpers;
using ViridianCore.Hooks;
using ViridianCore.Machine;

class EnemyTest : EnemyHook<TestMod.TestMod>
{
	void Start()
	{
		ObjectDebugger.DebugObject(gameObject, nameof(ViridianCore.ViridianCore));
		/*ObjectDebugger.DebugObject(gameObject, nameof(ViridianCore.ViridianCore));
		foreach (var gm in GameObject.FindObjectsOfType<GameObject>())
		{
			var collider = gm.GetComponent<Collider2D>();
			if (collider == null)
			{
				collider = gm.GetComponentInChildren<Collider2D>();
			}
			if (collider != null)
			{
				Modding.Logger.Log("GameObject = " + gm.name);
				Modding.Logger.Log("Collider = " + collider.name);
				Modding.Logger.Log("GameObject Layer = " + gm.layer);
				Modding.Logger.Log("GameObject Layer Name = " + LayerMask.LayerToName(gm.layer));
				Modding.Logger.Log("Collider Object = " + collider.gameObject.name);
				Modding.Logger.Log("Collider Layer = " + collider.gameObject.layer);
				Modding.Logger.Log("Collider Layer Name = " + LayerMask.LayerToName(collider.gameObject.layer));
				var bounds = collider.bounds;
				Modding.Logger.Log($"Collider Bounds = [Width = {bounds.size.x}, Height = {bounds.size.y}, Center = {bounds.center}, BottomLeft = {bounds.min}, TopRight = {bounds.max} ]");
			}
		}*/
	}
}