using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WeaverCore.Utilities;
using System.Linq;
using System.IO;
using WeaverCore.Editor.Utilities;
using UnityEditorInternal;

/// <summary>
/// Used for converting multiple textures, into a single texture consisting of multiple sprites
/// </summary>
public class TexturesToAtlasConverter : EditorWindow
{
	[MenuItem("WeaverCore/Tools/Convert Textures to Atlas")]
	public static void Convert()
	{
		Display();
	}

	public List<Texture2D> textureList;
	ReorderableList textures = null;
	bool destroyOriginalTextures = false;
	string outputAtlasName = "NEW_ATLAS";

	SerializedObject serializedObject;
	bool closed = false;

	Vector2 scrollPosition;

	public static TexturesToAtlasConverter Display()
	{
		var window = GetWindow<TexturesToAtlasConverter>();
		window.titleContent = new GUIContent("Textures To Atlas");
		window.Show();

		return window;
	}

	private void OnEnable()
	{
		serializedObject = new SerializedObject(this);

		textures = new ReorderableList(serializedObject, serializedObject.FindProperty("textureList"), false, true, true, true);

		textures.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Textures");
		textures.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
		{
			rect.y += 2f;
			rect.height = EditorGUIUtility.singleLineHeight;

			GUIContent objectLabel = new GUIContent($"Texture {index}");
			EditorGUI.PropertyField(rect, serializedObject.FindProperty("textureList").GetArrayElementAtIndex(index), objectLabel);
		};
	}

	private void OnGUI()
	{
		if (serializedObject == null || closed)
		{
			return;
		}
		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
		serializedObject.Update();

		EditorGUILayout.LabelField("Add the textures you want in the atlas to the list below");
		EditorGUILayout.Space();
		textures.DoLayoutList();

		EditorGUILayout.Space();
		if (GUILayout.Button(new GUIContent("Add Selected Textures", "Adds all the textures that are highlighted in the \"Project\" Window")))
		{
			foreach (var tex in Selection.objects.OfType<Texture2D>())
			{
				var list = serializedObject.FindProperty("textureList");
				list.arraySize++;
				list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = tex;
			}
		}

		destroyOriginalTextures = EditorGUILayout.Toggle(new GUIContent("Destroy Original Textures"), destroyOriginalTextures);
		outputAtlasName = EditorGUILayout.TextField(new GUIContent("Output Atlas Name"), outputAtlasName);

		if (GUILayout.Button("Convert"))
		{
			closed = true;
			Close();
			UnboundCoroutine.Start(Convert(textureList, destroyOriginalTextures, outputAtlasName));
		}

		EditorGUILayout.EndScrollView();
		if (!closed)
		{
			serializedObject.ApplyModifiedProperties();
		}
	}

	static IEnumerator Convert(List<Texture2D> textures, bool destroyOriginalTextures, string outputAtlasName)
	{
		if (textures.Count == 0)
		{
			yield break;
		}
		if (string.IsNullOrEmpty(outputAtlasName))
		{
			outputAtlasName = "NEW_ATLAS";
		}
		var outputPath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + outputAtlasName + ".png");

		var atlas = new Texture2D(8192, 8192, textures[0].format, false);
		Rect[] uvs = null;
		Vector2 atlasSize = default;

		try
		{
			using (var context = new ReadableTextureContext(textures))
			{
				AssetDatabase.StartAssetEditing();
				uvs = atlas.PackTextures(textures.ToArray(), 0,8192);
				atlasSize = new Vector2(atlas.width,atlas.height);
				using (var handle = File.Create(outputPath))
				{
					var pngData = ImageConversion.EncodeToPNG(atlas);
					handle.Write(pngData, 0, pngData.GetLength(0));
					handle.Close();
				}

				AssetDatabase.ImportAsset(outputPath);
			}
		}
		finally
		{
			AssetDatabase.StopAssetEditing();
		}

		try
		{
			AssetDatabase.StartAssetEditing();
			float averagePPU = 0;
			List<Vector2> pivots = new List<Vector2>();
			foreach (var tex in textures)
			{
				var texImport = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex));
				var texSettings = new TextureImporterSettings();
				texImport.ReadTextureSettings(texSettings);
				averagePPU = +texSettings.spritePixelsPerUnit;
				pivots.Add(texSettings.spritePivot);
			}
			averagePPU /= textures.Count;

			var importer = (TextureImporter)AssetImporter.GetAtPath(outputPath);
			var settings = new TextureImporterSettings();
			importer.ReadTextureSettings(settings);

			settings.spriteMode = (int)SpriteImportMode.Multiple;
			settings.spritePixelsPerUnit = averagePPU;

			var sheet = new SpriteMetaData[textures.Count];

			for (int i = 0; i < textures.Count; i++)
			{
				sheet[i] = new SpriteMetaData
				{
					alignment = (int)SpriteAlignment.Custom,
					name = textures[i].name,
					pivot = pivots[i],
					rect = new Rect(uvs[i].x * atlasSize.x, uvs[i].y * atlasSize.y, uvs[i].width * atlasSize.x, uvs[i].height * atlasSize.y),
					border = Vector4.zero
				};
			}


			importer.SetTextureSettings(settings);

			importer.spritesheet = sheet;

			var platformSettings = importer.GetPlatformTextureSettings("DefaultTexturePlatform");
			platformSettings.maxTextureSize = Mathf.RoundToInt(Mathf.Max(atlasSize.x, atlasSize.y));
			importer.SetPlatformTextureSettings(platformSettings);

			importer.SaveAndReimport();

			if (destroyOriginalTextures)
			{
				foreach (var tex in textures)
				{
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
