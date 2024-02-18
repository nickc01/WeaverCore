using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WeaverCore.Utilities;
using System.Linq;
using System.IO;
using WeaverCore.Editor.Utilities;
using UnityEditorInternal;
/*public class RemoveAllObjectsFromAsset : EditorWindow
{
	[MenuItem("WeaverCore/Tools/Remove All Objects From Asset")]
	public static void Convert()
	{
		Display();
	}

	UnityEngine.Object obj;
	bool closed = false;

	public static RemoveAllObjectsFromAsset Display()
	{
		var window = GetWindow<RemoveAllObjectsFromAsset>();
		window.titleContent = new GUIContent("Remove All Objects From Asset");
		window.Show();

		return window;
	}

    private void OnGUI()
    {
		obj = EditorGUILayout.ObjectField(new GUIContent("Asset", "The asset to remove all embedded objects from"), obj, typeof(UnityEngine.Object), true);

		if (GUILayout.Button("Remove Objects"))
		{
			closed = true;
			Close();
			RemoveObjects();
			//UnboundCoroutine.Start(Convert(spriteList, destroyOriginalTextures, outputAtlasName, cropTextures));
		}

		EditorGUILayout.EndScrollView();
	}

	void RemoveObjects()
    {
        if (obj != null)
        {
			var path = AssetDatabase.GetAssetPath(obj);
			var internalObjects = AssetDatabase.LoadAllAssetsAtPath(path);

			var assetUniquePath = AssetDatabase.GenerateUniqueAssetPath(path);

			foreach (var internalObject in internalObjects)
            {
                if (internalObject != obj)
                {
					AssetDatabase.Remove
                }
            }
        }
    }
}*/

public class CropTextures : EditorWindow
{
    [MenuItem("WeaverCore/Tools/Crop Textures")]
    public static void Convert()
    {
        Display();
    }

    public List<Texture2D> textureList;
    ReorderableList textures = null;
    //bool destroyOriginalTextures = false;
    //string outputAtlasName = "NEW_ATLAS";
    //bool cropTextures = true;

    SerializedObject serializedObject;
    bool closed = false;

    Vector2 scrollPosition;

    public static CropTextures Display()
    {
        var window = GetWindow<CropTextures>();
        window.titleContent = new GUIContent("Crop Textures");
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

        EditorGUILayout.LabelField("Add the textures you want to crop to the list below");
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

        //destroyOriginalTextures = EditorGUILayout.Toggle(new GUIContent("Destroy Original Textures"), destroyOriginalTextures);
        //outputAtlasName = EditorGUILayout.TextField(new GUIContent("Output Atlas Name"), outputAtlasName);
        //cropTextures = EditorGUILayout.Toggle(new GUIContent("Crop Textures", "Crops the textures so they can fit more tightly into the atlas"), cropTextures);

        if (GUILayout.Button("Crop"))
        {
            closed = true;
            Close();
            UnboundCoroutine.Start(Convert(textureList));
        }

        EditorGUILayout.EndScrollView();
        if (!closed)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }

    static TextureImporterSettings GetImportData(Texture tex)
    {
        var texImport = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex));
        var texSettings = new TextureImporterSettings();
        texImport.ReadTextureSettings(texSettings);
        return texSettings;
    }

    static Texture2D CropTexture(Texture2D input, Vector2 oldPivotCoordinates, out Vector2 newPivotCoordinates, int pixelPadding = 1)
    {
        var width = input.width;
        var height = input.height;
        var halfHeight = height / 2;
        var halfWidth = width / 2;
        var targetColor = default(Color);

        float alphaThreshold = 5 / 255f;

        newPivotCoordinates = oldPivotCoordinates;

        int leftPoint = 0;

        for (int i = 0; i < width; i++)
        {
            for (int y = 0; y < height; y++)
            {
                var color = input.GetPixel(i, y);
                if (color.a >= alphaThreshold)
                {
                    leftPoint = i - pixelPadding;
                    if (leftPoint < 0)
                    {
                        leftPoint = 0;
                    }
                    goto LeftLoopEnd;
                }
            }
        }
    LeftLoopEnd:

        newPivotCoordinates.x -= leftPoint;

        int rightPoint = width - 1;

        for (int i = width - 1; i >= 0; i--)
        {
            for (int y = 0; y < height; y++)
            {
                var color = input.GetPixel(i, y);
                if (color.a >= alphaThreshold)
                {
                    rightPoint = i + pixelPadding;
                    if (rightPoint > width - 1)
                    {
                        rightPoint = width - 1;
                    }
                    goto RightLoopEnd;
                }
            }
        }
    RightLoopEnd:

        int bottomPoint = 0;

        for (int i = 0; i < height; i++)
        {
            for (int x = 0; x < width; x++)
            {
                var color = input.GetPixel(x, i);
                if (color.a >= alphaThreshold)
                {
                    bottomPoint = i - pixelPadding;
                    if (bottomPoint < 0)
                    {
                        bottomPoint = 0;
                    }
                    goto BottomLoopEnd;
                }
            }
        }
    BottomLoopEnd:

        newPivotCoordinates.y -= bottomPoint;

        int topPoint = height - 1;

        for (int i = height - 1; i >= 0; i--)
        {
            for (int x = 0; x < width; x++)
            {
                var color = input.GetPixel(x, i);
                if (color.a >= alphaThreshold)
                {
                    topPoint = i + pixelPadding;
                    if (topPoint > height - 1)
                    {
                        topPoint = height - 1;
                    }
                    goto TopLoopEnd;
                }
            }
        }
    TopLoopEnd:

        var newWidth = rightPoint - leftPoint + 1;
        var newHeight = topPoint - bottomPoint + 1;

        var croppedTexture = new Texture2D(newWidth, newHeight);

        for (int x = leftPoint; x <= rightPoint; x++)
        {
            for (int y = bottomPoint; y <= topPoint; y++)
            {
                var sourcePixel = input.GetPixel(x, y);

                /*if (CheckSimilarity(targetColor, sourcePixel) > backgroundFillPercentage)
				{
					sourcePixel = new Color(0f, 0f, 0f, 0f);
				}*/
                //GColorToAlpha(ref sourcePixel.a, ref sourcePixel.r, ref sourcePixel.g, ref sourcePixel.b, targetColor.r, targetColor.g, targetColor.b,0.5f);

                croppedTexture.SetPixel(x - leftPoint, y - bottomPoint, sourcePixel);
            }
        }

        croppedTexture.Apply();
        return croppedTexture;
    }

    static Vector2 GetPivot(TextureImporterSettings settings)
    {
        switch ((SpriteAlignment)settings.spriteAlignment)
        {
            case SpriteAlignment.Center:
                return new Vector2(0.5f, 0.5f);
            case SpriteAlignment.TopLeft:
                return new Vector2(0f, 1f);
            case SpriteAlignment.TopCenter:
                return new Vector2(0.5f, 1f);
            case SpriteAlignment.TopRight:
                return new Vector2(1f, 1f);
            case SpriteAlignment.LeftCenter:
                return new Vector2(0f, 0.5f);
            case SpriteAlignment.RightCenter:
                return new Vector2(1f, 0.5f);
            case SpriteAlignment.BottomLeft:
                return new Vector2(0f, 0f);
            case SpriteAlignment.BottomCenter:
                return new Vector2(0.5f, 0f);
            case SpriteAlignment.BottomRight:
                return new Vector2(1f, 0f);
            case SpriteAlignment.Custom:
                return new Vector2(settings.spritePivot.x, settings.spritePivot.y);
            default:
                return default;
        }
    }

    static IEnumerator Convert(List<Texture2D> textures)
    {
        if (textures.Count == 0)
        {
            yield break;
        }
        /*if (string.IsNullOrEmpty(outputAtlasName))
        {
            outputAtlasName = "NEW_ATLAS";
        }*/
        //var outputPath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + outputAtlasName + ".png");
        //("NEWLY CREATED ATLAS = " + atlas);
        //Rect[] uvs = null;
        //Vector2 atlasSize = default;

        //float averagePPU = 0;
        List<Vector2> pivots = new List<Vector2>();
        bool editing = false;
        try
        {
            foreach (var tex in textures)
            {
                var texImport = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(tex));
                var texSettings = new TextureImporterSettings();
                texImport.ReadTextureSettings(texSettings);
                //averagePPU += texSettings.spritePixelsPerUnit;
                var spriteRect = new Rect(0f, 0f, tex.width, tex.height);
                var spritePivot = GetPivot(texSettings);
                pivots.Add(new Vector2(LerpUtilities.UnclampedLerp(spriteRect.xMin, spriteRect.xMax, spritePivot.x), LerpUtilities.UnclampedLerp(spriteRect.yMin, spriteRect.yMax, spritePivot.y)));
            }
            //averagePPU /= textures.Count;

            List<Texture2D> resizedTextures = new List<Texture2D>();
            using (var context = new ReadableTextureContext(textures))
            {
                //AssetDatabase.StartAssetEditing();
                //editing = true;
                for (int i = 0; i < textures.Count; i++)
                {
                    resizedTextures.Add(CropTexture(textures[i], pivots[i], out var newPivot));
                    pivots[i] = newPivot;
                }



                //AssetDatabase.StopAssetEditing();
                /*var atlas = new Texture2D(8192, 8192, TextureFormat.RGBA32, false);
                uvs = atlas.PackTextures(cropTextures ? resizedTextures.ToArray() : textures.ToArray(), 0, 8192);
                atlasSize = new Vector2(atlas.width, atlas.height);
                using (var handle = File.Create(outputPath))
                {
                    var pngData = ImageConversion.EncodeToPNG(atlas);
                    handle.Write(pngData, 0, pngData.GetLength(0));
                    handle.Close();
                }*/

                //AssetDatabase.ImportAsset(outputPath);
            }

            AssetDatabase.StartAssetEditing();
			editing = true;

            for (int i = 0; i < textures.Count; i++)
			{
				var path = AssetDatabase.GetAssetPath(textures[i]);

                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);

				var resizedForm = resizedTextures[i];

                //settings.spriteMode = (int)SpriteImportMode.Multiple;
                //settings.spritePixelsPerUnit = averagePPU;

                var sheet = new SpriteMetaData[textures.Count];

                settings.spriteAlignment = (int)SpriteAlignment.Custom;
				settings.spritePivot = new Vector2(LerpUtilities.UnclampedInverseLerp(0f, resizedForm.width, pivots[i].x), LerpUtilities.UnclampedInverseLerp(0f, resizedForm.height, pivots[i].y));//pivots[i];

                /*for (int i = 0; i < textures.Count; i++)
                {
                    var spriteRect = new Rect(uvs[i].x * atlasSize.x, uvs[i].y * atlasSize.y, uvs[i].width * atlasSize.x, uvs[i].height * atlasSize.y);
                    sheet[i] = new SpriteMetaData
                    {
                        alignment = (int)SpriteAlignment.Custom,
                        name = textures[i].name,
                        pivot = new Vector2(LerpUtilities.UnclampedInverseLerp(0f, spriteRect.width, pivots[i].x), LerpUtilities.UnclampedInverseLerp(0f, spriteRect.height, pivots[i].y)),
                        rect = spriteRect,
                        border = Vector4.zero
                    };
                }*/


                importer.SetTextureSettings(settings);

                importer.spritesheet = sheet;

                var platformSettings = importer.GetPlatformTextureSettings("DefaultTexturePlatform");
                platformSettings.maxTextureSize = 8192;
                importer.SetPlatformTextureSettings(platformSettings);

                importer.SaveAndReimport();

				File.WriteAllBytes(path, ImageConversion.EncodeToPNG(resizedTextures[i]));

            }

			editing = false;
            AssetDatabase.StopAssetEditing();

        }
        finally
        {
            if (editing)
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        yield break;
    }
}
