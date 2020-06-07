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

namespace WeaverCore.Editor.Visual
{
	public enum Orientation
	{
		Up = 0,
		Right = 90,
		Down = 180,
		Left = 270
	}

	public struct SpriteLocation
	{
		public SpriteSheet Sprite;
		public string FileLocation;
		public int UVWidth;
		public int UVHeight;
		public Vector2Int SpriteDimensions;
	}


	public static class TextureUnraveller
	{
		static float progress = 0.0f;

		static float Progress
		{
			get => progress;
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

		[MenuItem("WeaverCore/Unravel Texture")]
		public static void UnravelTexture()
		{
			URoutine.Start(StartUnravelling());
		}


		static int Difference(int a, int b)
		{
			return Mathf.Max(a, b) - Mathf.Min(a, b);
		}

		static T[,] RotateLeft<T>(T[,] matrix)
		{
			var width = matrix.GetLength(0);
			var height = matrix.GetLength(1);
			T[,] newMatrix = new T[height,width];

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					newMatrix[height - 1 - y, x] = matrix[x, y];
					//newMatrix[y, width - 1 - x] = matrix[x, y];
				}
			}
			return newMatrix;
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

		static string PathAddBackslash(string path)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			path = path.TrimEnd();

			if (PathEndsWithDirectorySeparator())
				return path;

			return path + GetDirectorySeparatorUsedInPath();

			bool PathEndsWithDirectorySeparator()
			{
				if (path.Length == 0)
					return false;

				char lastChar = path[path.Length - 1];
				return lastChar == Path.DirectorySeparatorChar
					|| lastChar == Path.AltDirectorySeparatorChar;
			}

			char GetDirectorySeparatorUsedInPath()
			{
				if (path.Contains(Path.AltDirectorySeparatorChar))
					return Path.AltDirectorySeparatorChar;

				return Path.DirectorySeparatorChar;
			}
		}

		static string RelativeToAssets(string input)
		{
			Uri fullPath = new Uri(input, UriKind.Absolute);
			Uri relRoot = new Uri(PathAddBackslash(new DirectoryInfo("Assets\\..").FullName),UriKind.Absolute);

			string relPath = relRoot.MakeRelativeUri(fullPath).ToString();

			return relPath;
			/*Debugger.Log("Input Test = " + input);
			var match = Regex.Match(input, @"(Assets[\\\/].+)");
			if (match.Success)
			{
				return match.Groups[0].Value;
			}
			else
			{
				return "";
			}*/
		}


		static IEnumerator<IUAwaiter> StartUnravelling()
		{
			yield return UnravelWindow.GetUnravelSettings();

			var settings = UnravelWindow.Settings;

			//Debugger.Log("Settings = " + settings);

			if (settings == null)
			{
				yield break;
			}


			string sheetData = File.ReadAllText(settings.SheetPath);

			//var sheet = Json.Deserialize<TextureSheet>(sheetData);
			var sheet = JsonUtility.FromJson<TextureSheet>(sheetData);

			var folder = EditorUtility.OpenFolderPanel("Select the folder where you want to textures to be dumped to", "Assets", "");

			if (folder == "")
			{
				yield break;
			}

			folder = RelativeToAssets(folder);
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




					//float PreBLy = ((-PostProcessedTR.y + 1.0f) * settings.texture.height) + uvOffset.y;

					int PreWidth = Difference(PreBLx, PreTRx);
					int PreHeight = Difference(PreBLy,PreTRy);


					//Debugger.Log($"Post BL = {PostProcessedBL.x}, {PostProcessedBL.y}");
					//Debugger.Log($"Post TR = {PostProcessedTR.x}, {PostProcessedTR.y}");
					//Debugger.Log($"Post Dimensions -> Width = {PostProcessedTR.x - PostProcessedBL.x}, Height = {PostProcessedTR.y - PostProcessedBL.y}");

					//Debugger.Log($"Pre BL = {PreBLx}, {PreBLy}");
					//Debugger.Log($"Pre TR = {PreTRx}, {PreTRy}");
					//Debugger.Log($"Dimensions -> Width = {PreWidth}, Height = {PreHeight}");

					//Debugger.Log("Sprite Name = " + sprite.SpriteName);

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

					//Debugger.Log("Orientation = " + orientation);

					Vector2Int Min = new Vector2Int(Mathf.RoundToInt(Mathf.Min(PreBLx, PreTRx)), Mathf.RoundToInt(Mathf.Min(PreBLy, PreTRy)));

					Vector2Int Max = new Vector2Int(Mathf.RoundToInt(Mathf.Max(PreBLx, PreTRx)), Mathf.RoundToInt(Mathf.Max(PreBLy, PreTRy)));

					//Texture2D texture = new Texture2D(Difference(Min.x, Max.x) + 1, Difference(Min.y, Max.y) + 1);

					Vector2Int SpriteDimensions = new Vector2Int(Difference(Min.x, Max.x) + 1, Difference(Min.y, Max.y) + 1);

					Color[,] colorMatrix = new Color[SpriteDimensions.x, SpriteDimensions.y];

					//Debugger.Log("STARTING");
					//Debugger.Log("Color Matrix = " + colorMatrix);

					for (int x = Min.x; x <= Max.x; x++)
					{
						for (int y = Min.y; y <= Max.y; y++)
						{
							Color reading = settings.texture.GetPixel(x, y);
							//Debugger.Log("X = " + (x - Min.x));
							//Debugger.Log("Y = " + (y - Min.y));
							colorMatrix[x - Min.x, y - Min.y] = reading;
							//texture.SetPixel(x - Min.x, y - Min.y, reading);
						}
					}

					//Debugger.Log("F");

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

					//Debugger.Log("G");
					if (sprite.Flipped)
					{
						colorMatrix = HorizontalFlip(colorMatrix);
					}

					//Debugger.Log("H");
					/*if (orientation == Orientation.Right)
					{
						colorMatrix = RotateRight(colorMatrix);
						colorMatrix = RotateRight(colorMatrix);
						colorMatrix = RotateRight(colorMatrix);
						//texture = new Texture2D(Difference(Min.y, Max.y) + 1, Difference(Min.x, Max.x) + 1);
					}
					else if (orientation == Orientation.Down)
					{
						colorMatrix = RotateRight(colorMatrix);
						colorMatrix = RotateRight(colorMatrix);
						//colorMatrix = RotateRight(colorMatrix);
						//texture = new Texture2D(Difference(Min.x, Max.x) + 1, Difference(Min.y, Max.y) + 1);
					}
					else if (orientation == Orientation.Left)
					{
						colorMatrix = RotateRight(colorMatrix);
						//colorMatrix = RotateLeft(colorMatrix);
						//colorMatrix = RotateLeft(colorMatrix);
						//texture = new Texture2D(Difference(Min.y, Max.y) + 1, Difference(Min.x, Max.x) + 1);
					}
					else
					{
						//colorMatrix = RotateRight(colorMatrix);
					}
					colorMatrix = RotateRight(colorMatrix);
					colorMatrix = RotateRight(colorMatrix);
					colorMatrix = RotateRight(colorMatrix);*/

					//if (sprite.Flipped)
					//{
					//	colorMatrix = VerticalFlip(colorMatrix);
					//}

					texture = new Texture2D(colorMatrix.GetLength(0), colorMatrix.GetLength(1));
					/*else
					{
						texture = new Texture2D(Difference(Min.x, Max.x) + 1, Difference(Min.y, Max.y) + 1);
					}*/

					//Debugger.Log("I");

					for (int x = 0; x < texture.width; x++)
					{
						for (int y = 0; y < texture.height; y++)
						{
							texture.SetPixel(x, y, colorMatrix[x,y]);
						}
					}

					//Debugger.Log("J");

					/*Debugger.Log("Before Transformation Begin");
					string output = "";
					for (int y = 0; y < colorMatrix.GetLength(1); y++)
					{
						string yAxis = "{ ";

						for (int x = 0; x < colorMatrix.GetLength(0); x++)
						{
							yAxis += colorMatrix[x, y] + ", ";
						}
						yAxis += " }";

						output += yAxis + "\n";
						//Debugger.Log(yAxis);
					}
					Debugger.Log("Output = " + output);
					Debugger.Log("Before Transformation End");

					colorMatrix = RotateLeft(colorMatrix);

					output = "";*/

					/*Debugger.Log("After Transformation Begin");
					for (int y = 0; y < colorMatrix.GetLength(1); y++)
					{
						string yAxis = "{ ";

						for (int x = 0; x < colorMatrix.GetLength(0); x++)
						{
							yAxis += colorMatrix[x, y] + ", ";
						}
						yAxis += " }";

						output += yAxis + "\n";
						//Debugger.Log(yAxis);
					}
					Debugger.Log("Output = " + output);
					Debugger.Log("After Transformation End");*/

					//(PostProcessedBL.x * settings.texture.width) - uvOffset.x = uvRegion.x;

					// ((-PostProcessedBL.y + 1.0f) * settings.texture.height) - uvOffset.y = uvRegion.y + uvRegion.height;

					//(PostProcessedTR.x * settings.texture.width) + uvOffset.x = uvRegion.x + uvRegion.width;

					// ((-PostProcessedTR.y + 1.0f) * settings.texture.height) + uvOffset.y = uvRegion.y;


					//PostProcessedBL.x = (uvRegion.x + uvOffset.x) / fwidth;
					//PostProcessedBL.y = 1.0f - (uvRegion.y + uvRegion.height + uvOffset.y) / fheight;

					//PostProcessedTR.x = (uvRegion.x + uvRegion.width - uvOffset.x) / fwidth;
					//PostProcessedTR.y = 1.0f - (uvRegion.y - uvOffset.y) / fheight;

					/*Vector2 uvOffset = new Vector2(0.001f, 0.001f);

					Debugger.Log("Before Bottom Left = " + new Vector2(sprite.BottomLeftX, sprite.BottomLeftY));
					Debugger.Log("Before Top Right = " + new Vector2(sprite.TopRightX, sprite.TopRightY));

					int BottomLeftX = Mathf.RoundToInt((sprite.BottomLeftX * settings.texture.width) - uvOffset.x);
					int TopRightY = Mathf.RoundToInt(((sprite.BottomLeftY - 1.0f) * -settings.texture.height) - uvOffset.y);

					int TopRightX = Mathf.RoundToInt((sprite.TopRightX * settings.texture.width) + uvOffset.x);
					int BottomLeftY = Mathf.RoundToInt(((sprite.TopRightY - 1.0f) * -settings.texture.height) + uvOffset.y);

					Debugger.Log("Bottom Left = " + new Vector2Int(BottomLeftX,BottomLeftY));
					Debugger.Log("Top Right = " + new Vector2Int(TopRightX,TopRightY));*/
					//Vector2 BottomLeft = new Vector2((sprite.BottomLeftX * settings.texture.width) - uvOffset.x,(-sprite.BottomLeftY * settings.texture.height) + 1.0f - uvOffset.y);

					//uvRegion.y

					//1.0f - (uvRegion.y + uvRegion.height + uvOffset.y)
					//-1.0f + (uvRegion.y + uvRegion.height + uvOffset.y)
					//uvRegion.y + uvRegion.height

					//Vector2 v0 = new Vector2((uvRegion.x + uvOffset.x) / fwidth, 1.0f - ((uvRegion.y + uvRegion.height + uvOffset.y) / fheight));
					//Vector2 v1 = new Vector2((uvRegion.x + uvRegion.width - uvOffset.x) / fwidth, 1.0f - ((uvRegion.y - uvOffset.y) / fheight));

					/*int width = PreWidth;
					int height = PreHeight;

					if ((PreTRx > PreBLx && PreTRy < PreBLy) || (PreTRx < PreBLx && PreTRy > PreBLy))
					{
						width = PreHeight;
						height = PreWidth;
					}

					Texture2D texture = new Texture2D(width, height);


					ReadTexture(new Vector2Int(PreBLx, PreBLy), new Vector2Int(PreTRx, PreTRy), settings.texture, (c, x, y) =>
					{
						texture.SetPixel(x, y, c);
					});*/

					var fileName = sprite.SpriteName;
					if (fileName == "")
					{
						fileName = "unknown_" + i;
					}

					var filePath = folder + "/" + fileName + ".png";

					//Debugger.Log("K");

					using (var file = File.Create(filePath))
					{
						using (var writer = new BinaryWriter(file))
						{
							var png = texture.EncodeToPNG();
							writer.Write(png);
						}
					}

					//Debugger.Log("L");

					//Debugger.Log("File Path = " + filePath);


					AssetDatabase.ImportAsset(filePath);

					AddedSprites.Add(new SpriteLocation()
					{
						Sprite = sprite,
						FileLocation = filePath,
						UVWidth = PreWidth,
						UVHeight = PreHeight,
						SpriteDimensions = new Vector2Int(colorMatrix.GetLength(0), colorMatrix.GetLength(1))
					});

					/*var assetImporter = AssetImporter.GetAtPath(filePath);

					Debugger.Log("Asset Importer = " + assetImporter);

					Debugger.Log("Importer Type + " + assetImporter.GetType());

					var importer = (TextureImporter)assetImporter;

					Debugger.Log("Pixels Per Unit = " + (PreWidth / sprite.WorldSize.x));

					importer.spritePixelsPerUnit = PreWidth / sprite.WorldSize.x;

					Debugger.Log("A");

					Debugger.Log("Pivot = " + sprite.Pivot);

					importer.spritePivot = sprite.Pivot;

					Debugger.Log("A");

					importer.SaveAndReimport();

					Debugger.Log("B");*/

					//TODO

					//importer.spritePivot = sprite.Pivot;
					//importer.spritePixelsPerUnit = sprite.PixelsPerUnit;

					//importer.SaveAndReimport();
					//AssetDatabase.CreateAsset(texture, folder + "/" + sprite.SpriteName + ".png");

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
					Debugger.Log("Sprite = " + sprite.Sprite.SpriteName);
					Debugger.Log("UV Width = " + sprite.UVWidth);
					Debugger.Log("World Size = " + sprite.Sprite.WorldSize);
					Debugger.Log("Sprite Dimensions = " + sprite.SpriteDimensions);


					var importer = (TextureImporter)AssetImporter.GetAtPath(sprite.FileLocation);

					//importer.spritePixelsPerUnit = sprite.UVWidth / sprite.Sprite.WorldSize.x;
					TextureImporterSettings importSettings = new TextureImporterSettings();
					importer.ReadTextureSettings(importSettings);

					importSettings.spritePixelsPerUnit = sprite.SpriteDimensions.x / sprite.Sprite.WorldSize.x;
					importSettings.spriteAlignment = (int)SpriteAlignment.Custom;
					importSettings.spritePivot = new Vector2(sprite.Sprite.Pivot.x, 1 - sprite.Sprite.Pivot.y);//new Vector2(sprite.Sprite.Pivot.x, sprite.Sprite.Pivot.y);

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

			Debugger.Log("<b>Unravel Complete</b>");


			//Debugger.Log("Project Folder = " + projectFolder.FullName);
		}

	}
}
