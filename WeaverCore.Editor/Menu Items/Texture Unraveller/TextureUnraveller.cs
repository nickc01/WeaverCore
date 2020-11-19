using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore.Interfaces;
using WeaverCore.DataTypes;
using System.Collections;
using WeaverCore.Editor.Structures;
using WeaverCore.Editor.Enums;

namespace WeaverCore.Editor
{
	public static class TextureUnraveller
	{
		static float progress = 0.0f;

		static float Progress
		{
			get
			{
				return progress;
			}
			set
			{
				progress = value;
				EditorUtility.DisplayProgressBar("Unravelling", "Unravelling Textures", progress);
				if (progress == 0.0f || progress == 1.0f)
				{
					EditorUtility.ClearProgressBar();
				}
			}
		}

		[MenuItem("WeaverCore/Tools/Unravel Texture")]
		public static void UnravelTexture()
		{
			//WeaverRoutine.Start(StartUnravelling());
			UnboundCoroutine.Start(StartUnravelling());
		}


		static int Difference(int a, int b)
		{
			return Mathf.Max(a, b) - Mathf.Min(a, b);
		}

		static T[,] VerticalFlip<T>(T[,] matrix)
		{
			var width = matrix.GetLength(0);
			var height = matrix.GetLength(1);
			T[,] newMatrix = new T[width, height];

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					newMatrix[x, height - 1 - y] = matrix[x, y];
				}
			}
			return newMatrix;
		}

		static T[,] HorizontalFlip<T>(T[,] matrix)
		{
			var width = matrix.GetLength(0);
			var height = matrix.GetLength(1);
			T[,] newMatrix = new T[width, height];

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					newMatrix[width - 1 - x, y] = matrix[x, y];
				}
			}
			return newMatrix;
		}

		static T[,] RotateLeft<T>(T[,] matrix)
		{
			var width = matrix.GetLength(0);
			var height = matrix.GetLength(1);
			T[,] newMatrix = new T[height, width];

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					newMatrix[height - 1 - y, x] = matrix[x, y];
				}
			}
			return newMatrix;
		}

		static IEnumerator StartUnravelling()
		{
			yield return UnravelWindow.GetUnravelSettings();

			var settings = UnravelWindow.Settings;


			if (settings == null)
			{
				yield break;
			}

			if (settings.UnravelMode == Enums.UnravelMode.TextureMode)
			{
				yield return DoTextureModeUnravelling(settings);
			}
			else
			{
				yield return DoSpriteModeUnravelling(settings);
			}
		}

		static IEnumerable<SpriteData> ReadTexturesFromSheet(TextureSheet sheet, TextureUnravelSettings settings)
		{
			for (int i = 0; i < sheet.Sprites.Count; i++)
			{
				//Debugger.Log("D");
				Progress = i / (float)sheet.Sprites.Count;

				var sprite = sheet.Sprites[i];

				Vector2 uvOffset = new Vector2(0.001f, 0.001f);

				Vector2 PostProcessedBL = Vector2.zero;
				Vector2 PostProcessedTR = Vector2.zero;

				bool rotated = false;

				if (sprite.UVs[0].x == sprite.UVs[1].x && sprite.UVs[2].x == sprite.UVs[3].x)
				{
					rotated = true;
				}


				if (rotated)
				{
					PostProcessedBL = sprite.UVs[1];
					PostProcessedTR = sprite.UVs[2];
				}
				else
				{
					PostProcessedBL = sprite.UVs[0];
					PostProcessedTR = sprite.UVs[3];
				}

				int PreBLx = Mathf.RoundToInt((PostProcessedBL.x * settings.texture.width) - uvOffset.x);
				int PreTRy = Mathf.RoundToInt(((PostProcessedBL.y) * settings.texture.height) - uvOffset.y);
				//float PreTRy = ((-PostProcessedBL.y + 1.0f) * settings.texture.height) - uvOffset.y;
				int PreTRx = Mathf.RoundToInt((PostProcessedTR.x * settings.texture.width) + uvOffset.x);
				int PreBLy = Mathf.RoundToInt(((PostProcessedTR.y) * settings.texture.height) + uvOffset.y);


				int PreWidth = Difference(PreBLx, PreTRx);
				int PreHeight = Difference(PreBLy, PreTRy);

				Orientation orientation = Orientation.Up;

				if (PostProcessedBL.x < PostProcessedTR.x && PostProcessedBL.y < PostProcessedTR.y)
				{
					orientation = Orientation.Up;
				}
				else if (PostProcessedBL.x < PostProcessedTR.x && PostProcessedBL.y > PostProcessedTR.y)
				{
					orientation = Orientation.Right;
				}
				else if (PostProcessedBL.x > PostProcessedTR.x && PostProcessedBL.y > PostProcessedTR.y)
				{
					orientation = Orientation.Down;
				}
				else if (PostProcessedBL.x > PostProcessedTR.x && PostProcessedBL.y < PostProcessedTR.y)
				{
					orientation = Orientation.Left;
				}

				Vector2Int Min = new Vector2Int(Mathf.RoundToInt(Mathf.Min(PreBLx, PreTRx)), Mathf.RoundToInt(Mathf.Min(PreBLy, PreTRy)));

				Vector2Int Max = new Vector2Int(Mathf.RoundToInt(Mathf.Max(PreBLx, PreTRx)), Mathf.RoundToInt(Mathf.Max(PreBLy, PreTRy)));

				Vector2Int SpriteDimensions = new Vector2Int(Difference(Min.x, Max.x) + 1, Difference(Min.y, Max.y) + 1);

				Color[,] colorMatrix = new Color[SpriteDimensions.x, SpriteDimensions.y];

				for (int x = Min.x; x <= Max.x; x++)
				{
					for (int y = Min.y; y <= Max.y; y++)
					{
						Color reading = settings.texture.GetPixel(x, y);
						colorMatrix[x - Min.x, y - Min.y] = reading;
					}
				}

				Texture2D texture = null;
				switch (orientation)
				{
					case Orientation.Up:
						break;
					case Orientation.Right:
						colorMatrix = RotateLeft(colorMatrix);
						break;
					case Orientation.Down:
						colorMatrix = RotateLeft(colorMatrix);
						colorMatrix = RotateLeft(colorMatrix);
						break;
					case Orientation.Left:
						colorMatrix = RotateLeft(colorMatrix);
						colorMatrix = RotateLeft(colorMatrix);
						colorMatrix = RotateLeft(colorMatrix);
						break;
					default:
						break;
				}

				if (sprite.Flipped)
				{
					colorMatrix = HorizontalFlip(colorMatrix);
				}

				texture = new Texture2D(colorMatrix.GetLength(0), colorMatrix.GetLength(1));
				texture.name = sprite.SpriteName;
				for (int x = 0; x < texture.width; x++)
				{
					for (int y = 0; y < texture.height; y++)
					{
						texture.SetPixel(x, y, colorMatrix[x, y]);
					}
				}

				yield return new SpriteData()
				{
					Texture = texture,
					Sheet = sprite,
					UVDimensions = new Vector2Int(PreWidth, PreHeight),
					PixelsPerUnit = texture.width / sprite.WorldSize.x,
					SpriteCoords = new Rect(Min.x, Min.y, Max.x - Min.x + 1, Max.y - Min.y + 1)
				};

				/*var fileName = sprite.SpriteName;
				if (fileName == "")
				{
					fileName = "unknown_" + i;
				}

				var filePath = folder + "/" + fileName + ".png";


				using (var file = File.Create(filePath))
				{
					using (var writer = new BinaryWriter(file))
					{
						var png = texture.EncodeToPNG();
						writer.Write(png);
					}
				}*/
				/*AssetDatabase.ImportAsset(filePath);

				AddedSprites.Add(new SpriteLocation()
				{
					Sprite = sprite,
					FileLocation = filePath,
					UVWidth = PreWidth,
					UVHeight = PreHeight,
					SpriteDimensions = new Vector2Int(colorMatrix.GetLength(0), colorMatrix.GetLength(1))
				});*/

			}
		}

		static IEnumerator DoTextureModeUnravelling(TextureUnravelSettings settings)
		{
			string sheetData = File.ReadAllText(settings.SheetPath);
			var sheet = JsonUtility.FromJson<TextureSheet>(sheetData);

			var folder = EditorUtility.OpenFolderPanel("Select the folder where you want to textures to be dumped to", "Assets", "");

			if (folder == "")
			{
				yield break;
			}

			folder = PathUtilities.MakePathRelative(new DirectoryInfo("Assets\\..").FullName, folder);
			if (folder == "")
			{
				throw new Exception("The folder specified is not within the Assets Folder");
			}


			//Debugger.Log("Folder = " + folder);
			var projectFolder = new DirectoryInfo("Assets\\..\\");

			List<SpriteLocation> AddedSprites = new List<SpriteLocation>();

			try
			{
				AssetDatabase.StartAssetEditing();
				//for (int i = 0; i < sheet.Sprites.Count; i++)
				foreach (var spriteData in ReadTexturesFromSheet(sheet,settings))
				{
					//Debugger.Log("D");
					/*Progress = i / (float)sheet.Sprites.Count;

					var sprite = sheet.Sprites[i];

					Vector2 uvOffset = new Vector2(0.001f, 0.001f);

					Vector2 PostProcessedBL = Vector2.zero;
					Vector2 PostProcessedTR = Vector2.zero;

					bool rotated = false;

					if (sprite.UVs[0].x == sprite.UVs[1].x && sprite.UVs[2].x == sprite.UVs[3].x)
					{
						rotated = true;
					}


					if (rotated)
					{
						PostProcessedBL = sprite.UVs[1];
						PostProcessedTR = sprite.UVs[2];
					}
					else
					{
						PostProcessedBL = sprite.UVs[0];
						PostProcessedTR = sprite.UVs[3];
					}

					int PreBLx = Mathf.RoundToInt((PostProcessedBL.x * settings.texture.width) - uvOffset.x);
					int PreTRy = Mathf.RoundToInt(((PostProcessedBL.y) * settings.texture.height) - uvOffset.y);
					//float PreTRy = ((-PostProcessedBL.y + 1.0f) * settings.texture.height) - uvOffset.y;
					int PreTRx = Mathf.RoundToInt((PostProcessedTR.x * settings.texture.width) + uvOffset.x);
					int PreBLy = Mathf.RoundToInt(((PostProcessedTR.y) * settings.texture.height) + uvOffset.y);


					int PreWidth = Difference(PreBLx, PreTRx);
					int PreHeight = Difference(PreBLy, PreTRy);

					Orientation orientation = Orientation.Up;

					if (PostProcessedBL.x < PostProcessedTR.x && PostProcessedBL.y < PostProcessedTR.y)
					{
						orientation = Orientation.Up;
					}
					else if (PostProcessedBL.x < PostProcessedTR.x && PostProcessedBL.y > PostProcessedTR.y)
					{
						orientation = Orientation.Right;
					}
					else if (PostProcessedBL.x > PostProcessedTR.x && PostProcessedBL.y > PostProcessedTR.y)
					{
						orientation = Orientation.Down;
					}
					else if (PostProcessedBL.x > PostProcessedTR.x && PostProcessedBL.y < PostProcessedTR.y)
					{
						orientation = Orientation.Left;
					}

					Vector2Int Min = new Vector2Int(Mathf.RoundToInt(Mathf.Min(PreBLx, PreTRx)), Mathf.RoundToInt(Mathf.Min(PreBLy, PreTRy)));

					Vector2Int Max = new Vector2Int(Mathf.RoundToInt(Mathf.Max(PreBLx, PreTRx)), Mathf.RoundToInt(Mathf.Max(PreBLy, PreTRy)));

					Vector2Int SpriteDimensions = new Vector2Int(Difference(Min.x, Max.x) + 1, Difference(Min.y, Max.y) + 1);

					Color[,] colorMatrix = new Color[SpriteDimensions.x, SpriteDimensions.y];

					for (int x = Min.x; x <= Max.x; x++)
					{
						for (int y = Min.y; y <= Max.y; y++)
						{
							Color reading = settings.texture.GetPixel(x, y);
							colorMatrix[x - Min.x, y - Min.y] = reading;
						}
					}

					Texture2D texture = null;
					switch (orientation)
					{
						case Orientation.Up:
							break;
						case Orientation.Right:
							colorMatrix = RotateLeft(colorMatrix);
							break;
						case Orientation.Down:
							colorMatrix = RotateLeft(colorMatrix);
							colorMatrix = RotateLeft(colorMatrix);
							break;
						case Orientation.Left:
							colorMatrix = RotateLeft(colorMatrix);
							colorMatrix = RotateLeft(colorMatrix);
							colorMatrix = RotateLeft(colorMatrix);
							break;
						default:
							break;
					}

					if (sprite.Flipped)
					{
						colorMatrix = HorizontalFlip(colorMatrix);
					}

					texture = new Texture2D(colorMatrix.GetLength(0), colorMatrix.GetLength(1));
					for (int x = 0; x < texture.width; x++)
					{
						for (int y = 0; y < texture.height; y++)
						{
							texture.SetPixel(x, y, colorMatrix[x, y]);
						}
					}*/
					var fileName = spriteData.Texture.name;
					if (fileName == "")
					{
						fileName = "unknown_" + AddedSprites.Count;
					}

					var filePath = folder + "/" + fileName + ".png";


					using (var file = File.Create(filePath))
					{
						using (var writer = new BinaryWriter(file))
						{
							var png = spriteData.Texture.EncodeToPNG();
							writer.Write(png);
						}
					}
					AssetDatabase.ImportAsset(filePath);

					AddedSprites.Add(new SpriteLocation()
					{
						Sprite = spriteData.Sheet,
						FileLocation = filePath,
						UVWidth = spriteData.UVDimensions.x,
						UVHeight = spriteData.UVDimensions.y,
						SpriteDimensions = new Vector2Int(spriteData.Texture.width, spriteData.Texture.height)
					});

				}
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
				Progress = 0.0f;
			}

			yield return null;
			yield return null;

			try
			{
				AssetDatabase.StartAssetEditing();
				foreach (var sprite in AddedSprites)
				{
					var importer = (TextureImporter)AssetImporter.GetAtPath(sprite.FileLocation);

					TextureImporterSettings importSettings = new TextureImporterSettings();
					importer.ReadTextureSettings(importSettings);

					importSettings.spritePixelsPerUnit = sprite.SpriteDimensions.x / sprite.Sprite.WorldSize.x;
					importSettings.spriteAlignment = (int)SpriteAlignment.Custom;
					importSettings.spritePivot = new Vector2(sprite.Sprite.Pivot.x, 1 - sprite.Sprite.Pivot.y);

					importer.SetTextureSettings(importSettings);
					importer.SaveAndReimport();
				}
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
			}

			yield return null;
			yield return null;

			WeaverLog.Log("<b>Unravel Complete</b>");


			//Debugger.Log("Project Folder = " + projectFolder.FullName);
		}

		static IEnumerator DoSpriteModeUnravelling(TextureUnravelSettings settings)
		{
			string sheetData = File.ReadAllText(settings.SheetPath);
			var texSheet = JsonUtility.FromJson<TextureSheet>(sheetData);

			var folder = EditorUtility.OpenFolderPanel("Select the folder where you want the sprites to be dumped to", "Assets", "");

			if (folder == "")
			{
				yield break;
			}

			folder = PathUtilities.MakePathRelative(new DirectoryInfo("Assets\\..").FullName, folder);
			if (folder == "")
			{
				throw new Exception("The folder specified is not within the Assets Folder");
			}
			var projectFolder = new DirectoryInfo("Assets\\..\\");

			var selectedDir = new DirectoryInfo(folder);
			var relativeDir = PathUtilities.MakePathRelative(projectFolder.FullName, selectedDir.FullName);
			//Debugger.Log("Folder = " + folder);

			List<Texture2D> spriteTextures = new List<Texture2D>();
			List<SpriteData> sprites = new List<SpriteData>();

			foreach (var spriteData in ReadTexturesFromSheet(texSheet, settings))
			{
				sprites.Add(spriteData);
				spriteTextures.Add(spriteData.Texture);
			}

			Texture2D atlas = new Texture2D(texSheet.Width, texSheet.Height);
			atlas.name = texSheet.TextureName + "_orientated";

			var uvCoords = atlas.PackTextures(spriteTextures.ToArray(), 1,Mathf.Max(texSheet.Width,texSheet.Height),false);
			//WeaverLog.Log("Atlas Size = " + atlas.width + " , " + atlas.height);
			var pngData = atlas.EncodeToPNG();

			using (var fileTest = File.Create(selectedDir.FullName + "\\" + atlas.name + ".png"))
			{
				fileTest.Write(pngData, 0, pngData.GetLength(0));
			}
			AssetDatabase.ImportAsset(relativeDir + "\\" + atlas.name + ".png");

			yield return null;

			//DefaultTexturePlatform

			var importer = (TextureImporter)AssetImporter.GetAtPath(relativeDir + "\\" + atlas.name + ".png");
			var platformSettings = importer.GetPlatformTextureSettings("DefaultTexturePlatform");
			platformSettings.maxTextureSize = Mathf.Max(texSheet.Width, texSheet.Height);
			importer.SetPlatformTextureSettings(platformSettings);

			float averagePPU = 0f;

			foreach (var sprite in sprites)
			{
				averagePPU += sprite.PixelsPerUnit;
			}
			averagePPU /= sprites.Count;


			importer.spriteImportMode = SpriteImportMode.Multiple;
			importer.spritePixelsPerUnit = averagePPU;

			List<SpriteMetaData> metas = new List<SpriteMetaData>();


			//foreach (var sprite in sprites)
			for (int i = 0; i < sprites.Count; i++)
			{
				var sprite = sprites[i];
				var uv = uvCoords[i];
				var textureSize = new Vector2(sprite.Texture.width,sprite.Texture.height);
				//WeaverLog.Log("Sprite Coords for " + sprite.Texture.name + " = " + uvCoords[i]);
				metas.Add(new SpriteMetaData()
				{
					name = sprite.Sheet.SpriteName,
					border = Vector4.zero,
					pivot = new Vector2(sprite.Sheet.Pivot.x, 1 - sprite.Sheet.Pivot.y),
					alignment = (int)SpriteAlignment.Custom,
					rect = new Rect(uv.x * texSheet.Width,uv.y * texSheet.Height,uv.width * texSheet.Width,uv.height * texSheet.Height)
				});
			}
			importer.spritesheet = metas.ToArray();


			importer.SaveAndReimport();
		}

	}
}
