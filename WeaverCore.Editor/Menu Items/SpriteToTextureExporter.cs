using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;
using System.IO;

public class SpriteToTextureExporter : EditorWindow
{
    private List<Sprite> sprites = new List<Sprite>();
    private ReorderableList spriteList;
    private string outputFolderName = "ExportedTextures";

    // Add menu item to open this window
    [MenuItem("WeaverCore/Tools/Sprite To Texture Exporter")]
    public static void ShowWindow()
    {
        GetWindow<SpriteToTextureExporter>("Sprite To Texture Exporter");
    }

    private void OnEnable()
    {
        // Initialize the ReorderableList
        spriteList = new ReorderableList(sprites, typeof(Sprite), true, true, true, true);

        spriteList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Sprites");
        };

        spriteList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            rect.y += 2f;
            rect.height = EditorGUIUtility.singleLineHeight;

            sprites[index] = (Sprite)EditorGUI.ObjectField(rect, sprites[index], typeof(Sprite), false);
        };

        spriteList.onAddCallback = (ReorderableList list) =>
        {
            sprites.Add(null);
        };

        spriteList.onRemoveCallback = (ReorderableList list) =>
        {
            sprites.RemoveAt(list.index);
        };
    }

    private void OnGUI()
    {
        GUILayout.Label("Sprite To Texture Exporter", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        // Draw the ReorderableList
        spriteList.DoLayoutList();

        EditorGUILayout.Space();

        // Button to add selected sprites from Project window
        if (GUILayout.Button("Add Selected Sprites"))
        {
            foreach (var obj in Selection.objects)
            {
                if (obj is Sprite sprite && !sprites.Contains(sprite))
                {
                    sprites.Add(sprite);
                }
            }
        }

        EditorGUILayout.Space();

        // Output folder name
        outputFolderName = EditorGUILayout.TextField("Output Folder Name", outputFolderName);

        EditorGUILayout.Space();

        if (GUILayout.Button("Convert Sprites to Textures"))
        {
            ConvertSpritesToTextures();
        }
    }

    private void ConvertSpritesToTextures()
    {
        if (sprites.Count == 0)
        {
            EditorUtility.DisplayDialog("No Sprites Selected", "Please add at least one sprite.", "OK");
            return;
        }

        string outputFolderPath = "Assets/" + outputFolderName;

        if (!AssetDatabase.IsValidFolder(outputFolderPath))
        {
            AssetDatabase.CreateFolder("Assets", outputFolderName);
        }

        foreach (Sprite sprite in sprites)
        {
            if (sprite == null)
                continue;

            // Get the texture data from the sprite
            Rect spriteRect = sprite.rect;
            Texture2D spriteTexture = sprite.texture;

            // Create a new texture with the dimensions of the sprite
            Texture2D newTexture = new Texture2D((int)spriteRect.width, (int)spriteRect.height, TextureFormat.RGBA32, false);

            // Copy the pixel data from the sprite
            Color[] pixels = spriteTexture.GetPixels(
                (int)spriteRect.x,
                (int)spriteRect.y,
                (int)spriteRect.width,
                (int)spriteRect.height);
            newTexture.SetPixels(pixels);
            newTexture.Apply();

            // Encode texture into PNG
            byte[] bytes = newTexture.EncodeToPNG();

            // Create output path
            string fullFolderPath = Path.Combine(Application.dataPath, outputFolderName);
            if (!Directory.Exists(fullFolderPath))
            {
                Directory.CreateDirectory(fullFolderPath);
            }

            string fileName = sprite.name + ".png";
            string fullPath = Path.Combine(fullFolderPath, fileName);

            // Write the PNG file
            File.WriteAllBytes(fullPath, bytes);

            // Import the new texture asset
            string assetPath = outputFolderPath + "/" + fileName;
            AssetDatabase.ImportAsset(assetPath);

            // Apply the same settings as the original sprite
            TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            TextureImporter originalTextureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(spriteTexture)) as TextureImporter;

            if (textureImporter != null && originalTextureImporter != null)
            {
                // Get original settings
                TextureImporterSettings originalSettings = new TextureImporterSettings();
                originalTextureImporter.ReadTextureSettings(originalSettings);

                // Apply settings to new texture
                textureImporter.SetTextureSettings(originalSettings);

                // Set pivot
                TextureImporterSettings newSettings = new TextureImporterSettings();
                textureImporter.ReadTextureSettings(newSettings);
                newSettings.spriteAlignment = (int)SpriteAlignment.Custom;
                newSettings.spritePivot = sprite.pivot;
                textureImporter.SetTextureSettings(newSettings);

                // Copy wrap mode and filter mode
                textureImporter.wrapMode = originalTextureImporter.wrapMode;
                textureImporter.filterMode = originalTextureImporter.filterMode;

                // Save and reimport
                textureImporter.SaveAndReimport();
            }

            // Clean up
            DestroyImmediate(newTexture);
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Conversion Complete", "Sprites have been converted to Textures.", "OK");
    }
}
