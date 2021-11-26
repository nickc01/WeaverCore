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
	public class DumpAnimationMapsTool : ToolsArea.ToolAction
	{
		[Serializable]
		class Dump
		{
			public class CollectionTextures
			{
				public tk2dSpriteCollectionData collection;
				public List<string> TextureNames;
			}

			public tk2dSpriteAnimation animation;
			public List<CollectionTextures> collectionTextures;
		}

		public string ToolName => "Dump Animation Maps On Object";

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

			var animator = hierarchy.CurrentSelection.GetComponent<tk2dSpriteAnimator>();
			if (animator == null)
			{
				WeaverLog.LogError($"{ToolName} : There is no tk2dSpriteAnimator component on the object");
				return;
			}

			DumpAnimatorMap(animator, ToolsCommon.DumpLocation.CreateSubdirectory($"{animator.gameObject.name} - {new System.Random().Next()}"));
			WeaverLog.LogError($"{ToolName} : Animation Map for {animator.gameObject.name} has been dumped successfully");
		}

		public static void DumpAnimatorMap(tk2dSpriteAnimator animator, DirectoryInfo dumpLocation)
		{
			/*var sprite = animator.GetComponent<tk2dSprite>();
			if (sprite != null)
			{
				DumpSpritesTool.DumpObjectSprites(sprite, dumpLocation);
			}*/
			dumpLocation.Create();
			var file = dumpLocation.AddSlash() + $"{animator.gameObject.name}.animmap";

			HashSet<tk2dSpriteCollectionData> collections = new HashSet<tk2dSpriteCollectionData>();

			foreach (var clip in animator.Library.clips)
			{
				foreach (var frame in clip.frames)
				{
					collections.Add(frame.spriteCollection);
				}
			}

			//Dictionary<tk2dSpriteCollectionData, List<string>> textureNames = new Dictionary<tk2dSpriteCollectionData, List<string>>();

			//List<tk2dSpriteCollectionData> collections = new List<tk2dSpriteCollectionData>();
			//List<string> textureNames = new List<string>();

			List<Dump.CollectionTextures> collectionTextures = new List<Dump.CollectionTextures>();

			foreach (var collection in collections)
			{
				collectionTextures.Add(new Dump.CollectionTextures
				{
					collection = collection,
					TextureNames = collection.textures.Select(t => collection.spriteCollectionName + "_" + t.name).ToList()
				});
				//textureNames.AddRange(collection.textures.Select(t => t.name));
			}

			var sourceData = new Dump
			{
				animation = animator.Library,
				collectionTextures = collectionTextures
			};
			/*var sourceData = new Dump
			{
				animation = animator.Library,
				TextureNames = 
		};*/
			//var result = JsonUtility.ToJson(sourceData);
			var result = JsonConvert.SerializeObject(sourceData, null, Formatting.Indented, new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				PreserveReferencesHandling = PreserveReferencesHandling.Objects,
			});
			File.WriteAllText(file, result);

			foreach (var collection in collections)
			{
				DumpSpritesTool.DumpObjectSprites(collection, dumpLocation);
			}
		}

		public bool OnGameObjectUpdated(GameObject selection)
		{
			return selection.GetComponent<tk2dSpriteAnimator>() != null;
		}
	}
}
