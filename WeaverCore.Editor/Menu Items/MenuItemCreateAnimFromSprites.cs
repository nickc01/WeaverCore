using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using WeaverCore.Editor.Compilation;
using WeaverCore.Utilities;

public class MenuItemCreateAnimFromSprites : EditorWindow
{
    const float DefaultClipFps = 24f;
    const string PersistentSettingsKey = "WeaverCore.CreateAnimDataFromSprites.Settings";

    static readonly Regex FrameSuffixRegex = new Regex(@"^(?<base>.+)_(?<index>\d+)$", RegexOptions.Compiled);
    static readonly Regex LowerUpperSplitRegex = new Regex(@"(?<=[a-z])(?=[A-Z])", RegexOptions.Compiled);
    static readonly Regex AcronymWordSplitRegex = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z])", RegexOptions.Compiled);
    static readonly Regex LetterDigitSplitRegex = new Regex(@"(?<=[A-Za-z])(?=\d)", RegexOptions.Compiled);
    static readonly Regex DigitLetterSplitRegex = new Regex(@"(?<=\d)(?=[A-Za-z])", RegexOptions.Compiled);
    static readonly Regex WhitespaceRegex = new Regex(@"\s+", RegexOptions.Compiled);

    [SerializeField]
    List<Sprite> spriteList = new List<Sprite>();

    [SerializeField]
    string outputPath = "Assets/New Weaver Animation Data.asset";

    [SerializeField]
    bool addSpaces = true;

    [SerializeField]
    WeaverAnimationData targetAnimData = null;

    [SerializeField]
    bool overwriteClips = false;

    [SerializeField]
    float clipFps = DefaultClipFps;

    ReorderableList sprites;
    SerializedObject serializedObject;
    Vector2 scrollPosition;

    struct SpriteEntry
    {
        public Sprite Sprite;
        public int Index;
    }

    [Serializable]
    class PersistentSettings
    {
        public float ClipFps = DefaultClipFps;
    }

    [MenuItem("WeaverCore/Tools/Create AnimData from Sprites")]
    public static void OpenWindow()
    {
        Display();
    }

    public static MenuItemCreateAnimFromSprites Display(WeaverAnimationData existingAnimData = null)
    {
        var window = GetWindow<MenuItemCreateAnimFromSprites>();
        window.titleContent = new GUIContent("Create AnimData from Sprites");
        window.targetAnimData = existingAnimData;
        window.Show();
        return window;
    }

    void OnEnable()
    {
        serializedObject = new SerializedObject(this);
        InitializeSpriteList();
        LoadPersistentSettings();
    }

    void InitializeSpriteList()
    {
        if (spriteList == null)
        {
            spriteList = new List<Sprite>();
        }

        sprites = new ReorderableList(serializedObject, serializedObject.FindProperty(nameof(spriteList)), true, true, true, true);

        sprites.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Sprites");
        sprites.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            rect.y += 2f;
            rect.height = EditorGUIUtility.singleLineHeight;

            var property = serializedObject.FindProperty(nameof(spriteList)).GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, property, new GUIContent($"Sprite {index}"));
        };
    }

    void OnGUI()
    {
        if (serializedObject == null)
        {
            serializedObject = new SerializedObject(this);
            InitializeSpriteList();
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        serializedObject.Update();

        EditorGUILayout.LabelField("Add sprites to build a WeaverAnimationData asset. Only names ending in _<number> are used.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space();

        sprites.DoLayoutList();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent("Add Selected Sprites", "Adds selected sprites, plus all sprites from selected textures.")))
        {
            AddSelectedSprites();
        }

        if (GUILayout.Button("Clear"))
        {
            spriteList.Clear();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        var targetAnimDataProperty = serializedObject.FindProperty(nameof(targetAnimData));
        EditorGUILayout.PropertyField(
            targetAnimDataProperty,
            new GUIContent("Target AnimData", "If set, generated clips are applied to this existing WeaverAnimationData asset."));
        EditorGUILayout.EndHorizontal();
        targetAnimData = targetAnimDataProperty.objectReferenceValue as WeaverAnimationData;

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        using (new EditorGUI.DisabledScope(targetAnimDataProperty.objectReferenceValue != null))
        {
            outputPath = EditorGUILayout.TextField(new GUIContent("Output Path"), outputPath);
            if (GUILayout.Button("Browse", GUILayout.Width(80f)))
            {
                BrowseForOutputPath();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        addSpaces = EditorGUILayout.Toggle(new GUIContent("Add Spaces"), addSpaces);
        overwriteClips = EditorGUILayout.Toggle(
            new GUIContent("Overwrite Clips", "When enabled, clips with matching names are replaced. When disabled, new clips are suffixed as _001, _002, etc."),
            overwriteClips);
        var newClipFps = EditorGUILayout.FloatField(
            new GUIContent("Clip FPS", "FPS applied to all generated clips."),
            clipFps);
        var sanitizedClipFps = Mathf.Max(0.01f, newClipFps);
        if (!Mathf.Approximately(sanitizedClipFps, clipFps))
        {
            clipFps = sanitizedClipFps;
            SavePersistentSettings();
        }

        EditorGUILayout.Space();

        EditorGUI.BeginDisabledGroup(spriteList == null || spriteList.Count == 0);
        if (GUILayout.Button("Generate"))
        {
            GenerateAnimData();
        }
        EditorGUI.EndDisabledGroup();

        serializedObject.ApplyModifiedProperties();
        EditorGUILayout.EndScrollView();
    }

    void BrowseForOutputPath()
    {
        var suggestedName = string.IsNullOrWhiteSpace(outputPath)
            ? "New Weaver Animation Data"
            : Path.GetFileNameWithoutExtension(outputPath);

        if (string.IsNullOrWhiteSpace(suggestedName))
        {
            suggestedName = "New Weaver Animation Data";
        }

        var selectedPath = EditorUtility.SaveFilePanelInProject(
            "Create AnimData",
            suggestedName,
            "asset",
            "Select where to create the WeaverAnimationData asset");

        if (!string.IsNullOrWhiteSpace(selectedPath))
        {
            outputPath = selectedPath.Replace('\\', '/');
        }
    }

    void AddSelectedSprites()
    {
        var foundSprites = new HashSet<Sprite>(Selection.objects.OfType<Sprite>());

        foreach (var texture in Selection.objects.OfType<Texture2D>())
        {
            var texturePath = AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrWhiteSpace(texturePath))
            {
                continue;
            }

            foreach (var sprite in AssetDatabase.LoadAllAssetsAtPath(texturePath).OfType<Sprite>())
            {
                foundSprites.Add(sprite);
            }
        }

        if (foundSprites.Count == 0)
        {
            return;
        }

        var existingSprites = new HashSet<Sprite>(spriteList.Where(s => s != null));
        var listProperty = serializedObject.FindProperty(nameof(spriteList));

        foreach (var sprite in foundSprites)
        {
            if (!existingSprites.Add(sprite))
            {
                continue;
            }

            listProperty.arraySize++;
            listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1).objectReferenceValue = sprite;
        }
    }

    void GenerateAnimData()
    {
        var usingExistingTarget = targetAnimData != null;
        var validatedOutputPath = string.Empty;

        if (!usingExistingTarget && !TryGetValidatedOutputPath(out validatedOutputPath))
        {
            return;
        }

        var spriteGroups = new Dictionary<string, List<SpriteEntry>>();
        int excludedSpriteCount = 0;

        foreach (var sprite in spriteList)
        {
            if (sprite == null)
            {
                continue;
            }

            var frameMatch = FrameSuffixRegex.Match(sprite.name);
            if (!frameMatch.Success)
            {
                excludedSpriteCount++;
                continue;
            }

            var clipKey = GetClipKey(frameMatch.Groups["base"].Value);
            if (string.IsNullOrWhiteSpace(clipKey))
            {
                excludedSpriteCount++;
                continue;
            }

            if (!int.TryParse(frameMatch.Groups["index"].Value, out var frameIndex))
            {
                excludedSpriteCount++;
                continue;
            }

            if (!spriteGroups.TryGetValue(clipKey, out var entries))
            {
                entries = new List<SpriteEntry>();
                spriteGroups.Add(clipKey, entries);
            }

            entries.Add(new SpriteEntry
            {
                Sprite = sprite,
                Index = frameIndex
            });
        }

        if (spriteGroups.Count == 0)
        {
            EditorUtility.DisplayDialog("No Valid Sprites", "No sprites matched the required naming pattern: <name>_<number>", "OK");
            return;
        }

        var outputAsset = usingExistingTarget
            ? targetAnimData
            : ScriptableObject.CreateInstance<WeaverAnimationData>();

        int overwrittenClipCount = 0;
        int renamedClipCount = 0;

        foreach (var group in spriteGroups.OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase))
        {
            var clipName = PrettifyClipName(group.Key, addSpaces);
            var resolvedClipName = clipName;
            var wrapMode = GetWrapModeForClipName(clipName);

            if (outputAsset.HasClip(clipName))
            {
                if (overwriteClips)
                {
                    outputAsset.RemoveClip(clipName);
                    overwrittenClipCount++;
                }
                else
                {
                    resolvedClipName = GetUniqueClipName(clipName, outputAsset);
                    renamedClipCount++;
                }
            }

            var orderedFrames = group.Value
                .OrderBy(entry => entry.Index)
                .ThenBy(entry => entry.Sprite.name, StringComparer.Ordinal)
                .Select(entry => entry.Sprite);

            outputAsset.AddClip(new WeaverAnimationData.Clip(
                resolvedClipName,
                clipFps,
                wrapMode,
                orderedFrames));
        }

        if (!usingExistingTarget)
        {
            var existingAsset = AssetDatabase.LoadMainAssetAtPath(validatedOutputPath);
            if (existingAsset != null)
            {
                var overwrite = EditorUtility.DisplayDialog(
                    "Overwrite Existing Asset?",
                    $"An asset already exists at:\n{validatedOutputPath}\n\nOverwrite it?",
                    "Overwrite",
                    "Cancel");

                if (!overwrite)
                {
                    DestroyImmediate(outputAsset);
                    return;
                }

                if (!AssetDatabase.DeleteAsset(validatedOutputPath))
                {
                    DestroyImmediate(outputAsset);
                    EditorUtility.DisplayDialog("Unable to Overwrite", $"Failed to delete existing asset at:\n{validatedOutputPath}", "OK");
                    return;
                }
            }

            AssetDatabase.CreateAsset(outputAsset, validatedOutputPath);
        }
        
        EditorUtility.SetDirty(outputAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = outputAsset;
        EditorGUIUtility.PingObject(outputAsset);

        EditorUtility.DisplayDialog(
            "AnimData Generated",
            $"Processed {spriteGroups.Count} clip(s) for:\n{GetAssetDescription(outputAsset, validatedOutputPath, usingExistingTarget)}\n\n" +
            $"Overwritten clips: {overwrittenClipCount}\n" +
            $"Renamed clips: {renamedClipCount}\n" +
            $"Excluded {excludedSpriteCount} sprite(s) that did not end in _<number>.",
            "OK");
    }

    static string GetClipKey(string frameBaseName)
    {
        if (string.IsNullOrWhiteSpace(frameBaseName))
        {
            return string.Empty;
        }

        var delimiterIndex = frameBaseName.IndexOf('-');
        if (delimiterIndex > 0)
        {
            return frameBaseName.Substring(0, delimiterIndex);
        }

        return frameBaseName;
    }

    static string PrettifyClipName(string clipKey, bool addSpaces = true)
    {
        if (string.IsNullOrWhiteSpace(clipKey))
        {
            return clipKey;
        }

        if (!addSpaces)
        {
            return clipKey;
        }

        var output = clipKey.Replace('_', ' ').Replace('-', ' ');
        output = LowerUpperSplitRegex.Replace(output, " ");
        output = AcronymWordSplitRegex.Replace(output, " ");
        output = LetterDigitSplitRegex.Replace(output, " ");
        output = DigitLetterSplitRegex.Replace(output, " ");
        output = WhitespaceRegex.Replace(output, " ").Trim();

        return output;
    }

    static string GetUniqueClipName(string clipName, WeaverAnimationData targetAsset)
    {
        var suffix = 1;
        var candidate = clipName;

        while (targetAsset.HasClip(candidate))
        {
            candidate = $"{clipName}_{suffix:000}";
            suffix++;
        }

        return candidate;
    }

    static WeaverAnimationData.WrapMode GetWrapModeForClipName(string clipName)
    {
        return EndsWithLoopWord(clipName)
            ? WeaverAnimationData.WrapMode.Loop
            : WeaverAnimationData.WrapMode.Once;
    }

    static bool EndsWithLoopWord(string clipName)
    {
        if (string.IsNullOrWhiteSpace(clipName))
        {
            return false;
        }

        var trimmedName = clipName.Trim();
        if (!trimmedName.EndsWith("Loop", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (trimmedName.Length == 4)
        {
            return true;
        }

        var suffixStartIndex = trimmedName.Length - 4;
        var previousCharacter = trimmedName[suffixStartIndex - 1];
        var suffixStartCharacter = trimmedName[suffixStartIndex];

        return !char.IsLetter(previousCharacter) || (char.IsLower(previousCharacter) && char.IsUpper(suffixStartCharacter));
    }

    static string GetAssetDescription(WeaverAnimationData outputAsset, string outputPath, bool usingExistingTarget)
    {
        if (!usingExistingTarget)
        {
            return outputPath;
        }

        var targetPath = AssetDatabase.GetAssetPath(outputAsset);
        return string.IsNullOrWhiteSpace(targetPath)
            ? outputAsset.name
            : targetPath;
    }

    void LoadPersistentSettings()
    {
        if (PersistentData.TryGetData(PersistentSettingsKey, out PersistentSettings settings) && settings != null)
        {
            var persistedFps = Mathf.Max(0.01f, settings.ClipFps);
            clipFps = persistedFps;
        }
        else
        {
            clipFps = DefaultClipFps;
        }
    }

    void SavePersistentSettings()
    {
        PersistentData.StoreData(new PersistentSettings
        {
            ClipFps = clipFps
        }, PersistentSettingsKey);
    }

    bool TryGetValidatedOutputPath(out string validatedOutputPath)
    {
        validatedOutputPath = (outputPath ?? string.Empty).Trim().Replace('\\', '/');

        if (string.IsNullOrWhiteSpace(validatedOutputPath))
        {
            EditorUtility.DisplayDialog("Invalid Output Path", "Please choose an output path.", "OK");
            return false;
        }

        if (!validatedOutputPath.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
        {
            validatedOutputPath += ".asset";
        }

        if (!validatedOutputPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
        {
            EditorUtility.DisplayDialog("Invalid Output Path", "Output path must be inside the project Assets folder.", "OK");
            return false;
        }

        var outputDirectory = Path.GetDirectoryName(validatedOutputPath)?.Replace('\\', '/');
        if (string.IsNullOrWhiteSpace(outputDirectory) || !AssetDatabase.IsValidFolder(outputDirectory))
        {
            EditorUtility.DisplayDialog("Invalid Output Path", "The output directory does not exist. Use Browse to choose a valid location.", "OK");
            return false;
        }

        outputPath = validatedOutputPath;
        return true;
    }
}
