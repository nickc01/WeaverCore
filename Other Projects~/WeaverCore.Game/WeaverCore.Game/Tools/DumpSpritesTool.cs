using RuntimeInspectorNamespace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore.Assets;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Tools
{
	/// <summary>
	/// Used in the WeaverCore Debug Tools to dump the sprites of an in-game object (only the sprites, not the sprite maps)
	/// </summary>
	public class DumpSpritesTool : ToolsArea.ToolAction
	{
		public string ToolName => "Dump Sprites On Object";

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

			DumpObjectSprites(sprite,ToolsCommon.DumpLocation.CreateSubdirectory($"{sprite.name} - {new System.Random().Next()}"));
		}

		public static void DumpObjectSprites(tk2dSprite sprite, DirectoryInfo dumpLocation)
		{
			DumpSpriteMapsTool.DumpSpriteCollectionMap(sprite, dumpLocation);
			DumpObjectSprites(sprite.Collection, dumpLocation);
		}

		public static void DumpObjectSprites(tk2dSpriteCollectionData collection, DirectoryInfo dumpLocation)
		{
			foreach (var texture in collection.textures)
			{
				if (texture is Texture2D t2d)
				{
					//var format = RenderTextureFormat.ARGB32;

					var rTex = RenderTexture.GetTemporary(t2d.width, t2d.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
					Graphics.Blit(texture, rTex);
					var result = ToTexture2D(rTex);
					RenderTexture.ReleaseTemporary(rTex);
					var data = result.EncodeToPNG();
					File.WriteAllBytes(dumpLocation.AddSlash() + collection.spriteCollectionName + "_" + t2d.name + ".png", data);
				}
			}
		}

		static Texture2D ToTexture2D(RenderTexture rTex)
		{
			Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.ARGB32, false);
			// ReadPixels looks at the active RenderTexture.
			RenderTexture.active = rTex;
			tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
			tex.Apply();
			return tex;
		}

		public bool OnGameObjectUpdated(GameObject selection)
		{
			return selection.GetComponent<tk2dSprite>() != null;
		}
	}
}
