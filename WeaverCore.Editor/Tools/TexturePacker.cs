using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Editor.Utilities;
using WeaverCore.Utilities;

namespace WeaverCore.Editor
{
	public static class TexturePacker
	{
		/// <summary>
		/// Checks if the "<paramref name="test"/>" number is near enough to "<paramref name="num"/>", with a specific margin of "<paramref name="error"/>"
		/// </summary>
		static bool NearbyNumber(float num, float test, float error = 1f)
		{
			return num >= test - error || num <= test + error;
		}

		/// <summary>
		/// Used to pack multiple textures into one
		/// </summary>
		/// <param name="path">The output path of the final texture</param>
		/// <param name="textures">The textures to pack</param>
		/// <param name="padding">The amount of padding between sprites in the final texture</param>
		/// <param name="deleteOld">Should the original texture be deleted when done?</param>
		public static IEnumerator PackTextures(FileInfo path, List<Texture2D> textures, int padding = 0, bool deleteOld = false)
		{
			var atlas = new Texture2D(2, 2,TextureFormat.ARGB32,false);
			atlas.name = path.GetFileName();
			Rect[] uvs = null;
			int nonNullTexturesSize = 0;
			using (var context = new ReadableTextureContext(textures))
			{
				yield return new WaitForSeconds(1f);

				var reimportedTextures = textures.Where(t => t != null).Select(t => AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GetAssetPath(t))).ToArray();
				nonNullTexturesSize = reimportedTextures.GetLength(0);

				uvs = atlas.PackTextures(reimportedTextures, padding, 16384);
			}

			var atlasSize = new Vector2(atlas.width, atlas.height);

			var relativePath = PathUtilities.ConvertToProjectPath(path.FullName);

			File.WriteAllBytes(path.FullName,atlas.EncodeToPNG());
			AssetDatabase.ImportAsset(relativePath);
			yield return null;

			atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(relativePath);
			AssetDatabase.StartAssetEditing();
			try
			{
				float averagePPU = 0;
				int amountWithinAverage = 0;
				float averageMarginPPU = 64;
				List<Vector2> pivots = new List<Vector2>();
				foreach (var tex in textures)
				{
					if (tex == null)
					{
						continue;
					}
					var texImport = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex));
					var texSettings = new TextureImporterSettings();
					texImport.ReadTextureSettings(texSettings);
					averagePPU += texSettings.spritePixelsPerUnit;

					if (NearbyNumber(texSettings.spritePixelsPerUnit, averageMarginPPU))
					{
						amountWithinAverage++;
					}

					pivots.Add(texSettings.spritePivot);
				}
				averagePPU /= nonNullTexturesSize;

				var importer = (TextureImporter)AssetImporter.GetAtPath(relativePath);
				var settings = new TextureImporterSettings();
				importer.ReadTextureSettings(settings);

				settings.spriteMode = (int)SpriteImportMode.Multiple;

				if (amountWithinAverage >= textures.Count / 2)
				{
					settings.spritePixelsPerUnit = averageMarginPPU;
				}
				else
				{
					settings.spritePixelsPerUnit = averagePPU;
				}

				List<SpriteMetaData> sheet = new List<SpriteMetaData>();

				int uvIndex = -1;
				for (int i = 0; i < textures.Count; i++)
				{
					if (textures[i] == null)
					{
						continue;
					}
					uvIndex++;
					sheet.Add(new SpriteMetaData
					{
						alignment = (int)SpriteAlignment.Custom,
						name = textures[i].name,
						pivot = pivots[uvIndex],
						rect = new Rect(uvs[uvIndex].x * atlasSize.x, uvs[uvIndex].y * atlasSize.y, uvs[uvIndex].width * atlasSize.x, uvs[uvIndex].height * atlasSize.y),
						border = Vector4.zero
					});
				}


				importer.SetTextureSettings(settings);

				importer.spritesheet = sheet.ToArray();

				var platformSettings = importer.GetPlatformTextureSettings("DefaultTexturePlatform");
				platformSettings.maxTextureSize = Mathf.RoundToInt(Mathf.Max(atlasSize.x,atlasSize.y));
				importer.SetPlatformTextureSettings(platformSettings);

				importer.SaveAndReimport();

				if (deleteOld)
				{
					foreach (var tex in textures)
					{
						if (tex == null)
						{
							continue;
						}
						var assetPath = AssetDatabase.GetAssetPath(tex);
						if (!string.IsNullOrEmpty(assetPath))
						{
							AssetDatabase.DeleteAsset(assetPath);
						}
					}
				}
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
			}

			yield break;
		}
	}
}
