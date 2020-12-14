using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WeaverCore.Editor.Utilities
{
	public sealed class ResizeTextureContext : IDisposable
	{
		bool disposed = false;

		public readonly List<int> PreviousSizes;
		public readonly List<Texture> Textures;

		public ResizeTextureContext(List<Texture> textures)
		{
			PreviousSizes = ResizeToActualSize(textures);
			Textures = textures;
		}

		public ResizeTextureContext(params Texture[] textures)
		{
			Textures = textures.ToList();
			PreviousSizes = ResizeToActualSize(Textures);
		}


		public static Vector2Int GetActualSizeOfImage(Texture texture)
		{
			return GetActualSizeOfImage(AssetDatabase.GetAssetPath(texture));
		}

		public static Vector2Int GetActualSizeOfImage(string imagePath)
		{
			FileInfo info = new FileInfo(imagePath);
			Texture2D tex = null;
			byte[] fileData;

			if (File.Exists(info.FullName))
			{
				fileData = File.ReadAllBytes(info.FullName);
				tex = new Texture2D(2, 2);
				tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.

				return new Vector2Int(tex.width,tex.height);
			}

			throw new Exception("The texture " + imagePath + " does not exist");
			/*FileInfo info = new FileInfo(imagePath);

			using (var stream = info.OpenRead())
			{

				byte[] imageMemory = new byte[24];

				stream.Read(imageMemory, 0, 24);

				if (!(imageMemory[1] == 'P' && imageMemory[2] == 'N' && imageMemory[3] == 'G'))
				{
					throw new Exception("The file ( " + imagePath + " ) is not a valid PNG file");
				}

				int width = (imageMemory[16] << 6) | (imageMemory[17] << 4) | (imageMemory[18] << 2) | (imageMemory[19]);
				int height = (imageMemory[20] << 6) | (imageMemory[21] << 4) | (imageMemory[22] << 2) | (imageMemory[23]);

				return new Vector2Int(width, height);
			}*/
		}

		public static List<int> ResizeToActualSize(List<Texture> textures)
		{
			try
			{
				AssetDatabase.StartAssetEditing();

				List<int> originalSizes = new List<int>();


				foreach (var texture in textures)
				{
					var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));


					int originalSize = importer.maxTextureSize;

					var actualSizeDim = GetActualSizeOfImage(texture);

					int actualSize = Mathf.Max(actualSizeDim.x, actualSizeDim.y);

					//importer.maxTextureSize = Mathf.Max(actualSize.x, actualSize.y);
					//importer.SaveAndReimport();

					//Debug.Log("Texture = " + texture.name);
					//Vector2Int originalSize = new Vector2Int(texture.width, texture.height);
					//Vector2Int actualSize = GetActualSizeOfImage(texture);

					//Debug.Log("Current Size = " + originalSize);
					//Debug.Log("Actual Size = " + actualSize);

					if (actualSize > originalSize)
					{
						var platformSettings = importer.GetPlatformTextureSettings("DefaultTexturePlatform");
						platformSettings.maxTextureSize = actualSize;
						importer.SetPlatformTextureSettings(platformSettings);
						importer.maxTextureSize = actualSize;
						importer.SaveAndReimport();
						//var platformSettings = importer.GetPlatformTextureSettings("DefaultTexturePlatform");
						//platformSettings.maxTextureSize = Mathf.Max(actualSize.x, actualSize.y);
						//importer.SetPlatformTextureSettings(platformSettings);
					}

					originalSizes.Add(actualSize);

				}

				return originalSizes;
			}
			finally
			{

				AssetDatabase.StopAssetEditing();
			}
		}

		public static void RevertSizes(List<Texture> textures, List<int> previousSizes)
		{
			try
			{
				AssetDatabase.StartAssetEditing();


				for (int i = 0; i < textures.Count; i++)
				{
					var texture = textures[i];
					var previousSize = previousSizes[i];

					var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));

					if (importer.maxTextureSize != previousSize)
					{
						var platformSettings = importer.GetPlatformTextureSettings("DefaultTexturePlatform");
						platformSettings.maxTextureSize = previousSize;
						importer.SetPlatformTextureSettings(platformSettings);
						importer.maxTextureSize = previousSize;

						importer.SaveAndReimport();
					}

					/*if (texture.width != previousSize.x || texture.height != previousSize.y)
					{
						

						importer.maxTextureSize = Mathf.Max(previousSize.x, previousSize.y);
						importer.SaveAndReimport();
						//var platformSettings = importer.GetPlatformTextureSettings("DefaultTexturePlatform");
						//platformSettings.maxTextureSize = Mathf.Max(previousSize.x, previousSize.y);
						//importer.SetPlatformTextureSettings(platformSettings);
					}*/
				}
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
			}
		}


		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;
				RevertSizes(Textures, PreviousSizes);
			}
		}


		~ResizeTextureContext()
		{
			Dispose();
		}
	}
}
