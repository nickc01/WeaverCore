using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WeaverCore.Utilities;

public class SpriteDecomposer : EditorWindow
{
	Texture2D sourceTexture;

	string folder;



	public static SpriteDecomposer Display()
	{
		var window = GetWindow<SpriteDecomposer>();
		window.Init();
		window.Show();

		return window;
	}

	void Init()
	{

	}

	void OnGUI()
	{
		//TODO
		sourceTexture = (Texture2D)EditorGUILayout.ObjectField("Texture to Decompose", sourceTexture, typeof(Texture2D), false);

		/*EditorGUILayout.BeginHorizontal();

		folder = EditorGUILayout.TextField("Dump Location", folder);
		if (GUILayout.Button("...", GUILayout.MaxWidth(30)))
		{
			//string exeLocation = EditorUtility.OpenFilePanel("Find where to dump the textures", Environment.CurrentDirectory, "exe");
			string location = EditorUtility.OpenFolderPanel("Find where to dump the textures", Environment.CurrentDirectory, "");
			if (location != null && location != "")
			{
				var fileInfo = new FileInfo(location);

				folder = fileInfo.Directory.FullName;
			}
		}

		EditorGUILayout.EndHorizontal();*/


		if (GUILayout.Button("Decompose"))
		{
			//ACTION
			Close();
			Decompose(sourceTexture,folder);
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


	void Decompose(Texture2D texture, string destFolder)
	{
		if (string.IsNullOrEmpty(destFolder))
		{
			destFolder = new DirectoryInfo("Assets\\" + texture.name + "_DUMP").FullName;
		}

		new DirectoryInfo(destFolder).Create();

		destFolder = "Assets/" + PathUtilities.ConvertToAssetPath(destFolder);

		//Debug.Log("Dest Folder = " + destFolder);

		//TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));

		try
		{
			AssetDatabase.StartAssetEditing();
			foreach (var sprite in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(texture)).OfType<Sprite>())
			{
				//Debug.Log("Sprite = " + sprite.name);
				//Debug.Log("Sprite Rect = " + sprite.rect);
				//Debug.Log("Sprite UV = " + sprite.uv);
				//Debug.Log("Sprite Border = " + sprite.border);
				//sprite.
				Texture2D dumpTexture = new Texture2D(Mathf.RoundToInt(sprite.rect.width), Mathf.RoundToInt(sprite.rect.height),TextureFormat.RGBA32,false);
				//RenderTexture dumpTexture = new RenderTexture(Mathf.RoundToInt(sprite.rect.width), Mathf.RoundToInt(sprite.rect.height),0,RenderTextureFormat.ARGB32);

				Vector2Int BottomLeftCorner = new Vector2Int(Mathf.RoundToInt(sprite.rect.x), Mathf.RoundToInt(sprite.rect.y));

				for (int x = Mathf.RoundToInt(sprite.rect.x); x < BottomLeftCorner.x + Mathf.RoundToInt(sprite.rect.width); x++)
				{
					for (int y = Mathf.RoundToInt(sprite.rect.y); y < BottomLeftCorner.y + Mathf.RoundToInt(sprite.rect.height); y++)
					{
						dumpTexture.SetPixel(x - BottomLeftCorner.x, y - BottomLeftCorner.y, sprite.texture.GetPixel(x, y));
						//Graphics.Blit()
					}
				}

				//Graphics.Blit(sprite.texture, dumpTexture, Vector2.one * 2f, Vector2.zero);
				//Graphics.Blit(sprite.texture, dumpTexture, Vector2.one, sprite.rect.min);

				//var finalTexture = ReadToTexture(dumpTexture);

				//Texture2D finalTexture = new Texture2D(Mathf.RoundToInt(sprite.rect.width), Mathf.RoundToInt(sprite.rect.height), TextureFormat.ARGB32, false);

				//dumpTexture

				//finalTexture.SetPixels(dumpTexture.)

				dumpTexture.Apply();

				var dumpTextFile = new FileInfo(destFolder + "/" + sprite.name + ".png");

				using (var handle = dumpTextFile.Create())
				{
					var pngData = ImageConversion.EncodeToPNG(dumpTexture);
					handle.Write(pngData, 0, pngData.GetLength(0));
					handle.Close();
				}

				AssetDatabase.ImportAsset(destFolder + "/" + sprite.name + ".png");

				//AssetDatabase.CreateAsset(dumpTexture, destFolder + "/" + )
			}
		}
		finally
		{
			AssetDatabase.StopAssetEditing();
		}
	}
}


public class DecomposeSpritesToTextures : MonoBehaviour 
{
	[MenuItem("WeaverCore/Tools/Decompose Sprites to Textures")]
	public static void DecomposeSprites()
	{
		SpriteDecomposer.Display();
		//var decomposer = EditorWindow.GetWindow<SpriteDecomposer>();
		//decomposer.Show();
		/*var packer = GetWindow<TexturePacker>();
		packer.Init();
		packer.Show();*/
	}
}
