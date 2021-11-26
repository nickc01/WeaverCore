using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.TK2D;
using WeaverCore.Utilities;

namespace WeaverCore.Editor
{
	public static class TK2DUnpacker
	{
		/// <summary>
		/// A list of recently unpacked sprites
		/// </summary>
		public static List<Sprite> UnpackedSprites = new List<Sprite>();

		public const bool FlipTK2DSpritesVertically = false;
		//static Vector2 uvOffset = new Vector2(0.001f, 0.001f);

		public static readonly List<string> ImageExtensions = new List<string>
		{
			".png",
			".jpg",
			".jpeg"
		};

		public static void UnpackTK2DAnimation()
		{
			UnpackedSprites.Clear();
			var openedFile = TK2dCommon.OpenAnimationMap(out var import, out var path);
			if (!openedFile)
			{
				Debug.LogError($"Unable to open file [{path}]");
			}

			UnpackTK2DAnimation(import, path);
		}

		private static void UnpackTK2DAnimation(AnimationMapImport import, string path)
		{
			var info = new FileInfo(path);
			var directory = info.Directory.AddSlash();

			Dictionary<string, List<Texture2D>> importedTextures = new Dictionary<string, List<Texture2D>>();

			//List<Texture2D> importedTextures = new List<Texture2D>();
			bool foundAllTextures = true;

			foreach (var collection in import.collectionTextures)
			{
				var texList = new List<Texture2D>();
				foreach (var name in collection.TextureNames)
				{
					if (texList.Any(t => t.name == name))
					{
						continue;
					}
					bool found = false;
					foreach (var ext in ImageExtensions)
					{
						var texFilePath = directory + name + ext;
						if (File.Exists(texFilePath) && TK2dCommon.OpenImage(texFilePath, out var tex))
						{
							texList.Add(tex);
							found = true;
							break;
						}
					}
					if (!found)
					{
						Debug.LogError($"Unable to find the texture [{name}]. Make sure it's in the same folder as the spritemap file");
						foundAllTextures = false;
					}
				}
				importedTextures.Add(collection.collection.spriteCollectionName, texList);
			}

			

			if (!foundAllTextures)
			{
				return;
			}
			/*List<Texture2D> importedTextures = new List<Texture2D>();

			bool foundAllTextures = true;

			foreach (var name in import.TextureNames)
			{
				bool found = false;
				foreach (var ext in ImageExtensions)
				{
					var texFilePath = directory + name + ext;
					if (File.Exists(texFilePath) && TK2dCommon.OpenImage(texFilePath, out var tex))
					{
						importedTextures.Add(tex);
						found = true;
						break;
					}
				}
				if (!found)
				{
					Debug.LogError($"Unable to find the texture [{name}]. Make sure it's in the same folder as the spritemap file");
					foundAllTextures = false;
				}
			}

			if (!foundAllTextures)
			{
				return;
			}*/


			UnboundCoroutine.Start(UnpackTK2DAnimationAsync(info.Name, import, importedTextures));
		}

		static IEnumerator UnpackTK2DAnimationAsync(string name, AnimationMapImport import, Dictionary<string, List<Texture2D>> importedTextures)
		{
			var fileName = name.Split('.')[0];
			//List<tk2dSpriteCollectionData> collections = new List<tk2dSpriteCollectionData>();

			//collections.AddRange(import.TextureNames.Keys);

			var totalUnpackedSprites = new List<Sprite>();

			List<DirectoryInfo> directories = new List<DirectoryInfo>();
			Dictionary<string, List<Sprite>> ExtractedSprites = new Dictionary<string, List<Sprite>>();
			foreach (var texCollection in import.collectionTextures)
			{
				var dir = new DirectoryInfo($"Assets\\{fileName} Animations\\{texCollection.collection.spriteCollectionName} Unpacked");
				directories.Add(dir);
				yield return UnpackSprites(texCollection.collection.spriteCollectionName, new SpriteMapImport
				{
					collection = texCollection.collection,
					TextureNames = texCollection.TextureNames
				}, importedTextures[texCollection.collection.spriteCollectionName], dir);
				ExtractedSprites.Add(texCollection.collection.spriteCollectionName, UnpackedSprites);
				totalUnpackedSprites.AddRange(UnpackedSprites);
				UnpackedSprites = new List<Sprite>();
			}

			UnpackedSprites = totalUnpackedSprites;


			WeaverAnimationData data = ScriptableObject.CreateInstance<WeaverAnimationData>();

			var animationData = import.animation;

			foreach (var clipRaw in animationData.clips)
			{
				if (string.IsNullOrEmpty(clipRaw.name))
				{
					continue;
				}
				var clip = new WeaverAnimationData.Clip
				{
					FPS = clipRaw.fps,
					LoopStart = clipRaw.loopStart,
					Name = clipRaw.name,
				};
				switch (clipRaw.wrapMode)
				{
					case tk2dSpriteAnimationClip.WrapMode.Loop:
						clip.WrapMode = WeaverAnimationData.WrapMode.Loop;
						break;
					case tk2dSpriteAnimationClip.WrapMode.LoopSection:
						clip.WrapMode = WeaverAnimationData.WrapMode.LoopSection;
						break;
					case tk2dSpriteAnimationClip.WrapMode.Once:
						clip.WrapMode = WeaverAnimationData.WrapMode.Once;
						break;
					case tk2dSpriteAnimationClip.WrapMode.PingPong:
						clip.WrapMode = WeaverAnimationData.WrapMode.PingPong;
						break;
					case tk2dSpriteAnimationClip.WrapMode.RandomFrame:
						clip.WrapMode = WeaverAnimationData.WrapMode.RandomFrame;
						break;
					case tk2dSpriteAnimationClip.WrapMode.RandomLoop:
						clip.WrapMode = WeaverAnimationData.WrapMode.RandomLoop;
						break;
					case tk2dSpriteAnimationClip.WrapMode.Single:
						clip.WrapMode = WeaverAnimationData.WrapMode.SingleFrame;
						break;
				}
				/*switch (clipRaw.wrapMode)
				{
					case WrapMode.Loop:
						clip.WrapMode = WeaverAnimationData.WrapMode.Loop;
						break;
					case WrapMode.LoopSection:
						clip.WrapMode = WeaverAnimationData.WrapMode.LoopSection;
						break;
					case WrapMode.Once:
						clip.WrapMode = WeaverAnimationData.WrapMode.Once;
						break;
					case WrapMode.PingPong:
						clip.WrapMode = WeaverAnimationData.WrapMode.PingPong;
						break;
					case WrapMode.RandomFrame:
						clip.WrapMode = WeaverAnimationData.WrapMode.RandomFrame;
						break;
					case WrapMode.RandomLoop:
						clip.WrapMode = WeaverAnimationData.WrapMode.RandomLoop;
						break;
					case WrapMode.Single:
						clip.WrapMode = WeaverAnimationData.WrapMode.SingleFrame;
						break;
					default:
						break;
				}*/

				foreach (var frameRaw in clipRaw.frames)
				{
					//clip.AddFrame(sprites[frameRaw.SpriteCollectionRaw.GetInstanceID()].GetSprite(frameRaw.SpriteID));
					for (int i = 0; i < ExtractedSprites[frameRaw.spriteCollection.spriteCollectionName].Count; i++)
					{
						Debug.Log($"Extracted Sprite {i} = {ExtractedSprites[frameRaw.spriteCollection.spriteCollectionName][i]}");
					}
					Debug.Log("SpriteID = " + frameRaw.spriteId);
					clip.AddFrame(ExtractedSprites[frameRaw.spriteCollection.spriteCollectionName][frameRaw.spriteId]);
				}

				data.AddClip(clip);
			}

			AssetDatabase.CreateAsset(data, $"Assets\\{fileName} Animations\\{fileName}.asset");

			//AssetDatabase.CreateAsset(data, "Assets/" + animationName + "/" + animationName + ".asset");

			//UnpackSprites(name,import.spr)
		}

		public static void UnpackTK2DSprites()
		{
			UnpackedSprites.Clear();
			var openedFile = TK2dCommon.OpenSpriteMap(out var import, out var path);
			if (!openedFile)
			{
				Debug.LogError($"Unable to open file [{path}]");
			}

			UnpackTK2DSprites(import, path);
		}

		private static void UnpackTK2DSprites(SpriteMapImport import, string path)
		{
			var info = new FileInfo(path);
			var directory = info.Directory.AddSlash();

			List<Texture2D> importedTextures = new List<Texture2D>();

			bool foundAllTextures = true;

			foreach (var name in import.TextureNames)
			{
				bool found = false;
				foreach (var ext in ImageExtensions)
				{
					var texFilePath = directory + name + ext;
					if (File.Exists(texFilePath) && TK2dCommon.OpenImage(texFilePath, out var tex))
					{
						importedTextures.Add(tex);
						found = true;
						break;
					}
				}
				if (!found)
				{
					Debug.LogError($"Unable to find the texture [{name}]. Make sure it's in the same folder as the spritemap file");
					foundAllTextures = false;
				}
			}

			if (!foundAllTextures)
			{
				return;
			}


			UnboundCoroutine.Start(UnpackSprites(info.GetFileName(), import, importedTextures, new DirectoryInfo($"Assets\\{info.GetFileName()} Unpacked")));
		}

		static int MoveTowards(int num, int dest)
		{
			if (num < dest)
			{
				num++;
			}
			else if (num > dest)
			{
				num--;
			}
			return num;
		}

		static bool AreUVsRotated(Vector2[] uvs)
		{
			return uvs[0].x == uvs[1].x && uvs[1].y == uvs[3].y;
		}

		static Color GetTexColor(int x, int y, Texture2D source)
		{
			if (x < 0 || x >= source.width || y < 0 || y >= source.height)
			{
				return default;
			}
			else
			{
				return source.GetPixel(x, y);
			}
		}

		static Rect UVsToRegion(IEnumerable<Vector2> uvs)
		{
			bool ranOnce = false;
			Vector2 min = default;
			Vector2 max = default;

			foreach (var uv in uvs)
			{
				if (!ranOnce)
				{
					ranOnce = true;
					min = uv;
					max = uv;
				}
				else
				{
					if (uv.x < min.x)
					{
						min.x = uv.x;
					}
					if (uv.x > max.x)
					{
						max.x = uv.x;
					}
					if (uv.y < min.y)
					{
						min.y = uv.y;
					}
					if (uv.y > max.y)
					{
						max.y = uv.y;
					}
				}
			}
			return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
		}

		static Texture2D ExtractRegion(Rect region, Texture2D source)
		{
			Texture2D result = new Texture2D(Mathf.RoundToInt(region.width), Mathf.RoundToInt(region.height), source.format,false);
			for (int x = Mathf.RoundToInt(region.xMin); x < Mathf.RoundToInt(region.xMax); x++)
			{
				for (int y = Mathf.RoundToInt(region.yMin); y < Mathf.RoundToInt(region.yMax); y++)
				{
					int relX = x - Mathf.RoundToInt(region.xMin);
					int relY = y - Mathf.RoundToInt(region.yMin);
					result.SetPixel(relX, relY, GetTexColor(x, y, source));
				}
			}
			result.Apply();
			return result;
		}


		static Texture2D ExtractSpriteFromTex(Texture2D source, tk2dSpriteDefinition sprite)
		{
			Vector2 vector2;
			Vector2 vector3;

			bool rotated = AreUVsRotated(sprite.uvs);

			if (rotated)
			{
				vector2 = new Vector2(sprite.uvs[0].x,sprite.uvs[1].y);
				vector3 = new Vector2(sprite.uvs[2].x,sprite.uvs[2].y);
			}
			else
			{
				vector2 = new Vector2(sprite.uvs[0].x, sprite.uvs[0].y);
				vector3 = new Vector2(sprite.uvs[3].x, sprite.uvs[3].y);
			}

			float x = source.width;
			float y = source.height;

			//Rect r = new Rect();

			//Vector2 vector = new Vector2(0.001f, 0.001f);

			//Vector2 vector2 = new Vector2((uvRegion.x + vector.x) / x, 1f - (uvRegion.y + uvRegion.height + vector.y) / y);
			//Vector2 vector3 = new Vector2((uvRegion.x + uvRegion.width - vector.x) / x, 1f - (uvRegion.y - vector.y) / y);


			//Vector2 vector2 = new Vector2(, );
			//Vector2 vector3 = new Vector2(, );

			//(uvRegion.x + vector.x);

			//(uvRegion.y);

			//(uvRegion.y + uvRegion.height + vector.y);

			//uvRegion.y = ((-(vector3.y * y)) + 1f) + vector.y;
			//r.y = (vector3.y * y) + vector.y;
			//r.x = (vector2.x * x) - vector.x;
			//r.width = (vector3.x * x) + vector.x - r.x;
			//r.height = (vector2.y * y) - vector.y - r.y;

			//Vector2 corner1 = new Vector2((vector2.x * x) - vector.x, (vector3.y * y) + vector.y);
			//Vector2 corner2 = new Vector2((vector3.x * x) + vector.x, (vector2.y * y) - vector.y);

			/*List<Vector2> multipliedUVs = new List<Vector2>();
			foreach (var uv in sprite.uvs)
			{
				multipliedUVs.Add(new Vector2(Mathf.RoundToInt(uv.x * x),Mathf.RoundToInt(uv.y * y)));
			}*/

			var multipliedUVs = sprite.uvs.Select(u => new Vector2(Mathf.RoundToInt(u.x * x), Mathf.RoundToInt(u.y * y)));

			//Debug.Log("Name = " + sprite.name);
			//Debug.Log("Flipped = " + sprite.flipped + " Rotated = " + AreUVsRotated(sprite.uvs));
			//Debug.Log("Rotated = " + AreUVsRotated(sprite.uvs));

			/*for (int i = 0; i < multipliedUVs.Count; i++)
			{
				Debug.Log($"UV {i} = [x = {multipliedUVs[i].x}, y = {multipliedUVs[i].y}]");
			}*/

			var spriteTexture = ExtractRegion(UVsToRegion(multipliedUVs), source);
			spriteTexture.name = sprite.name;

			if (rotated)
			{
				TextureUtilities.Rotate(spriteTexture, WeaverCore.Enums.RotationType.Right);
			}

			if (sprite.flipped == FlipMode.Tk2d || sprite.flipped == FlipMode.TPackerCW)
			{
				TextureUtilities.FlipHorizontally(spriteTexture);
			}

			return spriteTexture;
			//Debug.Log("UV Region = " + r);
			//Debug.Log($"Corner 1 = {corner1.x},{corner1.y}");
			//Debug.Log($"Corner 2 = {corner2.x},{corner2.y}");




			//sprite.uvs
			/*Vector2 PostProcessedBL = Vector2.zero;
			Vector2 PostProcessedTR = Vector2.zero;

			bool rotated = false;

			if (sprite.uvs[0].x == sprite.uvs[1].x && sprite.uvs[2].x == sprite.uvs[3].x)
			{
				rotated = true;
			}*/


			/*if (rotated)
			{
				PostProcessedBL = sprite.uvs[1];
				PostProcessedTR = sprite.uvs[2];
			}
			else
			{
				PostProcessedBL = sprite.uvs[0];
				PostProcessedTR = sprite.uvs[3];
			}

			List<Vector2> TexUVs = new List<Vector2>();

			foreach (var uv in sprite.uvs)
			{
				TexUVs.Add(new Vector2());
			}

			int PreBLx = Mathf.RoundToInt((PostProcessedBL.x * source.width) - uvOffset.x);
			int PreTRy = source.height - Mathf.RoundToInt(((PostProcessedBL.y) * source.height) - uvOffset.y);
			int PreTRx = Mathf.RoundToInt((PostProcessedTR.x * source.width) + uvOffset.x);
			int PreBLy = source.height - Mathf.RoundToInt(((PostProcessedTR.y) * source.height) + uvOffset.y);

			Vector2Int BottomLeft = new Vector2Int(PreBLx,PreBLy);
			Vector2Int TopRight = new Vector2Int(PreTRx,PreTRy);*/

			//int PreBLx2 = Mathf.RoundToInt((PostProcessedBL.x * source.width));
			//int PreTRy2 = Mathf.RoundToInt(((PostProcessedBL.y) * source.height));
			//int PreTRx2 = Mathf.RoundToInt((PostProcessedTR.x * source.width));
			//int PreBLy2 = Mathf.RoundToInt(((PostProcessedTR.y) * source.height));

			//Debug.Log("A = " + PreBLx);
			//Debug.Log("B = " + PreTRy);
			//Debug.Log("C = " + PreTRx);
			//Debug.Log("D = " + PreBLy);

			//Debug.Log("A2 = " + PreBLx2);
			//Debug.Log("B2 = " + PreTRy2);
			//Debug.Log("C2 = " + PreTRx2);
			//Debug.Log("D2 = " + PreBLy2);
			/*for (int i = 0; i < definition.uvs.GetLength(0); i++)
			{
				var uv = definition.uvs[i];

			}*/

			//return null;
		}

		static IEnumerator UnpackSprites(string name, SpriteMapImport spriteMap, List<Texture2D> importedTextures, DirectoryInfo outputDir)
		{
			//var outputDir = ;
			outputDir.Create();
			var relativeDir = PathUtilities.ConvertToProjectPath(outputDir.FullName);

			if (FlipTK2DSpritesVertically)
			{
				foreach (var tex in importedTextures)
				{
					TextureUtilities.FlipVertically(tex);
				}
			}

			EditorUtility.DisplayProgressBar("Unpacking Sprites", "", 0f);
			List<string> spritePaths = new List<string>();
			List<Texture2D> spriteTextures = new List<Texture2D>();
			try
			{
				AssetDatabase.StartAssetEditing();
				//foreach (var map in spriteMap.collection.spriteDefinitions)
				for (int i = 0; i < spriteMap.collection.spriteDefinitions.GetLength(0); i++)
				{
					var map = spriteMap.collection.spriteDefinitions[i];
					//Debug.Log("Sprite Name = " + map.name);
					if (map.name == "")
					{
						spritePaths.Add(null);
						spriteTextures.Add(null);
						continue;
					}
					EditorUtility.DisplayProgressBar("Unpacking Sprites", map.name, (float)i / spriteMap.collection.spriteDefinitions.GetLength(0));

					var texture = importedTextures[map.materialId];
					var spriteTexture = ExtractSpriteFromTex(texture, map);
					var spritePath = $"{relativeDir}\\{map.name}.png";
					spritePaths.Add(spritePath);
					spriteTextures.Add(spriteTexture);
					File.WriteAllBytes(spritePath, spriteTexture.EncodeToPNG());
					AssetDatabase.ImportAsset(spritePath);
				}
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
				EditorUtility.ClearProgressBar();
			}
			yield return PostUnpackSprites(name, spriteMap, importedTextures, spritePaths, spriteTextures, outputDir);
		}

		static float UnclampedLerp(float A, float B, float T)
		{
			return A + ((B - A) * T);
		}

		static float UnclampedInverseLerp(float A, float B, float C)
		{
			return (C - A) / (B - A);
		}

		static IEnumerator PostUnpackSprites(string name, SpriteMapImport spriteMap, List<Texture2D> importedTextures, List<string> spritePaths, List<Texture2D> spriteTextures, DirectoryInfo outputDir)
		{
			var relativeDir = PathUtilities.ConvertToProjectPath(outputDir.FullName);
			yield return null;
			//yield return new WaitForSeconds(0.5f);

			AssetDatabase.StartAssetEditing();

			try
			{
				for (int i = 0; i < spritePaths.Count; i++)
				{
					var path = spritePaths[i];
					if (path == null)
					{
						continue;
					}
					var info = new FileInfo(path);
					var texture = spriteTextures[i];

					var definition = spriteMap.collection.spriteDefinitions.First(d => $"{d.name}{info.Extension}" == info.Name);
					var relativePath = path;
					Debug.Log("Relative Path = " + relativePath);
					var importer = (TextureImporter)AssetImporter.GetAtPath(relativePath);
					var settings = new TextureImporterSettings();
					importer.ReadTextureSettings(settings);



					var blPos = definition.positions[0];
					var trPos = definition.positions[3];

					var horizontalppu = texture.width / (((double)trPos.x) - blPos.x);
					var verticalppu = texture.height / ((double)trPos.y - blPos.y);

					var averagePPU = (horizontalppu + verticalppu) / 2.0;
					settings.spriteAlignment = (int)SpriteAlignment.Custom;
					settings.spritePixelsPerUnit = (float)averagePPU;
					settings.filterMode = FilterMode.Bilinear;
					settings.spritePivot = new Vector2(UnclampedInverseLerp(blPos.x,trPos.x,0f),UnclampedInverseLerp(blPos.y,trPos.y,0f));

					if (UnpackTK2DWindow.Settings.UnpackMode == UnpackTK2DWindow.UnpackMode.ToSprite)
					{
						settings.readable = true;
						importer.isReadable = true;
					}

					importer.SetTextureSettings(settings);
					importer.SaveAndReimport();
				}
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
			}

			yield return null;

			if (UnpackTK2DWindow.Settings.UnpackMode == UnpackTK2DWindow.UnpackMode.ToTextures)
			{
				foreach (var spritePath in spritePaths)
				{
					if (spritePath == null)
					{
						UnpackedSprites.Add(null);
					}
					else
					{
						UnpackedSprites.AddRange(AssetDatabase.LoadAllAssetsAtPath(spritePath).OfType<Sprite>());
					}
				}
			}
			else if (UnpackTK2DWindow.Settings.UnpackMode == UnpackTK2DWindow.UnpackMode.ToSprite)
			{
				var outputPath = $"{relativeDir}\\{name}.png";
				var sprites = spritePaths.Select(s => s == null ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(s)).ToList();

				yield return TexturePacker.PackTextures(new FileInfo(outputPath), sprites, deleteOld: true);

				var finalSprites = AssetDatabase.LoadAllAssetsAtPath(outputPath).OfType<Sprite>().ToList();

				var orderedSprites = new Sprite[spritePaths.Count];
				for (int i = 0; i < finalSprites.Count; i++)
				{
					var index = spriteTextures.FindIndex(t => t != null && t.name == finalSprites[i].name);
					orderedSprites[index] = finalSprites[i];
				}

				UnpackedSprites.AddRange(orderedSprites);
			}
		}
	}
}
