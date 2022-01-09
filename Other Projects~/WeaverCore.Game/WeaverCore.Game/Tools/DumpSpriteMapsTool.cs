using Newtonsoft.Json;
using RuntimeInspectorNamespace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using WeaverCore.Assets;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Tools
{
	/// <summary>
	/// Used in the WeaverCore Debug Tools to dump the sprites of an in-game object
	/// </summary>
	public class DumpSpriteMapsTool : ToolsArea.ToolAction
	{
		[Serializable]
		class Dump
		{
			public tk2dSpriteCollectionData collection;
			public List<string> TextureNames;
		}


		public string ToolName => "Dump Sprites Maps On Object";

		public void OnToolExecute()
		{
			var hierarchy = RuntimeHierarchy.Instance;
			if (hierarchy == null)
			{
				WeaverLog.LogError($"{ToolName} : Couldn't find hiearchy");
				return;
			}

			if (hierarchy.CurrentSelection == null)
			{
				WeaverLog.LogError($"{ToolName} : No object selected in hierarchy");
				return;
			}

			var sprite = hierarchy.CurrentSelection.GetComponent<tk2dSprite>();
			if (sprite == null)
			{
				WeaverLog.LogError($"{ToolName} : There is no tk2dSprite component on the object");
				return;
			}

			DumpSpriteCollectionMap(sprite, ToolsCommon.DumpLocation.CreateSubdirectory($"{sprite.gameObject.name} - {new System.Random().Next()}"));
			WeaverLog.LogError($"{ToolName} : Sprite Map for {sprite.gameObject.name} has been dumped successfully");
		}

		public static void DumpSpriteCollectionMap(tk2dSprite sprite, DirectoryInfo dumpLocation)
		{
			dumpLocation.Create();
			var file = dumpLocation.AddSlash() + $"{sprite.gameObject.name}.spritemap";
			var sourceData = new Dump { collection = sprite.Collection, TextureNames = sprite.Collection.textures.Select(t => sprite.Collection.spriteCollectionName + "_" + t.name).ToList() };
			var result = JsonConvert.SerializeObject(sourceData, null, Formatting.Indented, new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				PreserveReferencesHandling = PreserveReferencesHandling.Objects
			});
			File.WriteAllText(file,result);
		}

		public bool OnGameObjectUpdated(GameObject selection)
		{
			return selection.GetComponent<tk2dSprite>() != null;
		}
	}
}
