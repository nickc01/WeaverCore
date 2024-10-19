using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using WeaverCore;
using WeaverCore.Editor.Utilities;
using WeaverCore.Utilities;

/// <summary>
/// Used for converting an atlas texture consisting multiple sprites, into multiple textures
/// </summary>
public class AtlasToTexturesConverter : EditorWindow
{
	[MenuItem("WeaverCore/Tools/Convert Atlas to Textures")]
	public static void Convert()
	{
        Display();
	}

	Texture2D sourceTexture;

	string folder;

	bool deleteOriginal = false;

	public static AtlasToTexturesConverter Display()
	{
		var window = GetWindow<AtlasToTexturesConverter>();
		window.titleContent = new GUIContent("Atlas To Textures");
		window.Show();

		return window;
	}

	void Awake()
	{
		deleteOriginal = false;
	}

	void OnGUI()
	{
		sourceTexture = (Texture2D)EditorGUILayout.ObjectField("Atlas to Convert", sourceTexture, typeof(Texture2D), false);
		deleteOriginal = EditorGUILayout.Toggle("Delete Original Atlas", deleteOriginal);

		if (GUILayout.Button("Convert"))
		{
			Close();
			UnboundCoroutine.Start(Convert(sourceTexture,folder, deleteOriginal));
		}
	}

	Texture2D ReadToTexture(RenderTexture rt)
	{
		var oldRT = RenderTexture.active;
		try
		{
			var tex = new Texture2D(rt.width, rt.height);
			RenderTexture.active = rt;
			tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
			tex.Apply();

			RenderTexture.active = oldRT;

			return tex;
		}
		finally
		{
			RenderTexture.active = oldRT;
        }
    }

    static float sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    static bool PointInTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
    {
        float d1, d2, d3;
        bool has_neg, has_pos;
        d1 = sign(pt, v1, v2);
        d2 = sign(pt, v2, v3);
        d3 = sign(pt, v3, v1);

        has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(has_neg && has_pos);
    }

    static bool PixelWithinMesh(Vector2[] verticies, ushort[] triangles, Vector2 point)
	{
		for (int i = 0; i < triangles.Length; i += 3)
		{
			if (PointInTriangle(point,verticies[triangles[i]], verticies[triangles[i + 1]], verticies[triangles[i + 2]]))
			{
				return true;
			}
		}
		return false;
	}

    Vector2 ConvertToVertexCoords(Vector2 pixel, Sprite sprite)
    {
		var rect = sprite.rect;


		var xPercent = Mathf.InverseLerp(rect.xMin, rect.xMax, pixel.x);
		var yPercent = Mathf.InverseLerp(rect.yMin, rect.yMax, pixel.y);

		var bounds = sprite.bounds;

		return new Vector2(Mathf.Lerp(bounds.min.x,bounds.max.x,xPercent), Mathf.Lerp(bounds.min.y,bounds.max.y,yPercent));
    }


    Vector2 ConvertToSpriteCoords(Vector2 vertex, Sprite sprite)
	{
		return vertex;
	}


	IEnumerator Convert(Texture2D texture, string destFolder, bool deleteOriginal)
	{
		if (string.IsNullOrEmpty(destFolder))
		{
			destFolder = new DirectoryInfo("Assets\\" + texture.name + "_DUMP").FullName;
		}

		new DirectoryInfo(destFolder).Create();

		destFolder = "Assets/" + PathUtilities.ConvertToAssetPath(destFolder);

        System.Collections.Generic.List<string> CreatedFilePaths = new System.Collections.Generic.List<string>();

		try
		{
			using (var context = new ReadableTextureContext(texture))
			{
				yield return null;

				texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GetAssetPath(texture));

				AssetDatabase.StartAssetEditing();
				foreach (var sprite in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(texture)).OfType<Sprite>())
				{
					var verticies = sprite.vertices;
					var triangles = sprite.triangles;

					Texture2D dumpTexture = new Texture2D(Mathf.RoundToInt(sprite.rect.width), Mathf.RoundToInt(sprite.rect.height), TextureFormat.RGBA32, false);
					Vector2Int BottomLeftCorner = new Vector2Int(Mathf.RoundToInt(sprite.rect.x), Mathf.RoundToInt(sprite.rect.y));

					for (int x = Mathf.RoundToInt(sprite.rect.x); x < BottomLeftCorner.x + Mathf.RoundToInt(sprite.rect.width); x++)
					{
						for (int y = Mathf.RoundToInt(sprite.rect.y); y < BottomLeftCorner.y + Mathf.RoundToInt(sprite.rect.height); y++)
						{
                            if (PixelWithinMesh(verticies,triangles, ConvertToVertexCoords(new Vector2(x,y), sprite)))
							{
                                dumpTexture.SetPixel(x - BottomLeftCorner.x, y - BottomLeftCorner.y, texture.GetPixel(x, y));
                            }
							else
							{
								dumpTexture.SetPixel(x - BottomLeftCorner.x, y - BottomLeftCorner.y, default);
							}
                        }
					}
					dumpTexture.Apply();

					var dumpTextFile = new FileInfo(destFolder + "/" + sprite.name + ".png");

					using (var handle = dumpTextFile.Create())
					{
						var pngData = ImageConversion.EncodeToPNG(dumpTexture);
						handle.Write(pngData, 0, pngData.GetLength(0));
						handle.Close();
					}

					AssetDatabase.ImportAsset(destFolder + "/" + sprite.name + ".png");
					CreatedFilePaths.Add(destFolder + "/" + sprite.name + ".png");
				}
			}
		}
		finally
		{
			AssetDatabase.StopAssetEditing();
		}

		yield return null;

		try
		{
			AssetDatabase.StartAssetEditing();

			var originalImport = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));
			var originalImportSettings = new TextureImporterSettings();
			originalImport.ReadTextureSettings(originalImportSettings);

			foreach (var spritePath in CreatedFilePaths)
			{
				var newTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(spritePath);

				var importer = (TextureImporter)AssetImporter.GetAtPath(spritePath);
				TextureImporterSettings settings = new TextureImporterSettings();
				importer.ReadTextureSettings(settings);

				var sheet = originalImport.spritesheet.First(s => s.name == newTexture.name);

				settings.spriteAlignment = (int)SpriteAlignment.Custom;
				settings.spritePivot = sheet.pivot;
				settings.spritePixelsPerUnit = originalImport.spritePixelsPerUnit;

				importer.SetTextureSettings(settings);
				importer.SaveAndReimport();
			}
		}
		finally
		{
			AssetDatabase.StopAssetEditing();
		}

		yield return null;

		if (deleteOriginal)
		{
			AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(texture));
		}
	}
}
