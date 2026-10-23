using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Content;
using WeaverCore.Editor;
using WeaverCore.Editor.Settings;
using WeaverCore.Utilities;

namespace WeaverCore.Editor.Compilation
{
    [Serializable]
    public class SceneBakeBuildData
    {
        public List<SceneBakeBundlePatch> BundlePatches = new List<SceneBakeBundlePatch>();
    }

    [Serializable]
    public class SceneBakeBundlePatch
    {
        public string CloneSceneName;
        public string CloneScenePath;
        public string GameLevelPath;
    }

    [Serializable]
    class SceneBakePersistentState
    {
        public List<SceneBakeReplacementRestoreData> AddedReplacements = new List<SceneBakeReplacementRestoreData>();
        public List<SceneBakeUnionRestoreData> AddedUnions = new List<SceneBakeUnionRestoreData>();
    }

    [Serializable]
    class SceneBakeReplacementRestoreData
    {
        public string SceneRecordPath;
        public string SceneToReplace;
        public string ReplacementScenePath;
    }

    [Serializable]
    class SceneBakeUnionRestoreData
    {
        public string SceneRecordPath;
        public string SceneToUnionize;
        public string UnionScenePath;
    }

    static class SceneBakeTools
    {
        const string PersistentStateKey = "weavercore_scene_bake_temporary_replacements";
        const string GeneratedContentsScenesFolder = "Assets/SceneBakeGenerated";

        sealed class SceneRecordReference
        {
            public string Path;
            public SceneRecord Record;
        }

        public static void PrepareForBuild(BundleTools.BundleBuildData buildData)
        {
            CleanupTemporaryReplacements();

            var sceneRecords = GetSceneRecords().ToList();
            if (sceneRecords.Count == 0)
            {
                buildData.SceneBakeData = null;
                return;
            }

            var recordsWithInverseUnions = sceneRecords
                .Where(record => record.Record.InverseSceneUnions.Any())
                .ToList();

            if (recordsWithInverseUnions.Count == 0)
            {
                buildData.SceneBakeData = null;
                return;
            }

            var dataFolder = GetHollowKnightDataFolder();
            var sceneLookup = LoadSceneLookup(dataFolder);

            var bakeData = new SceneBakeBuildData();
            var persistentState = new SceneBakePersistentState();
            bool assetsChanged = false;

            foreach (var recordReference in recordsWithInverseUnions)
            {
                foreach (var inverseSceneUnion in recordReference.Record.InverseSceneUnions)
                {
                    var bundlePatch = CreateBundlePatch(recordReference.Path, inverseSceneUnion, sceneLookup, dataFolder);
                    bakeData.BundlePatches.Add(bundlePatch);

                    var cloneSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(bundlePatch.CloneScenePath);
                    if (cloneSceneAsset == null)
                    {
                        throw new InvalidOperationException($"Could not load clone scene at {bundlePatch.CloneScenePath}");
                    }

                    var generatedContentsScenePath = CreateGeneratedContentsScene(bundlePatch.CloneScenePath);

                    var contentsSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(generatedContentsScenePath);
                    if (contentsSceneAsset == null)
                    {
                        throw new InvalidOperationException($"Could not load generated contents scene at {generatedContentsScenePath}");
                    }

                    if (recordReference.Record.AddSceneReplacement(bundlePatch.CloneSceneName, cloneSceneAsset))
                    {
                        persistentState.AddedReplacements.Add(new SceneBakeReplacementRestoreData
                        {
                            SceneRecordPath = recordReference.Path,
                            SceneToReplace = bundlePatch.CloneSceneName,
                            ReplacementScenePath = bundlePatch.CloneScenePath
                        });
                        assetsChanged = true;
                    }

                    if (recordReference.Record.AddSceneUnion(bundlePatch.CloneScenePath, contentsSceneAsset))
                    {
                        persistentState.AddedUnions.Add(new SceneBakeUnionRestoreData
                        {
                            SceneRecordPath = recordReference.Path,
                            SceneToUnionize = bundlePatch.CloneScenePath,
                            UnionScenePath = generatedContentsScenePath
                        });
                        assetsChanged = true;
                    }
                }
            }

            buildData.SceneBakeData = bakeData.BundlePatches.Count == 0 ? null : bakeData;

            if (persistentState.AddedReplacements.Count > 0 || persistentState.AddedUnions.Count > 0)
            {
                PersistentData.StoreData(persistentState, PersistentStateKey);
            }
            else
            {
                PersistentData.RemoveData(PersistentStateKey);
            }

            if (assetsChanged)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        public static void CleanupTemporaryReplacements()
        {
            var hasPersistentState = PersistentData.TryGetData(PersistentStateKey, out SceneBakePersistentState persistentState) && persistentState != null;
            bool assetsChanged = false;

            if (hasPersistentState)
            {
                foreach (var union in persistentState.AddedUnions)
                {
                    var record = AssetDatabase.LoadAssetAtPath<SceneRecord>(union.SceneRecordPath);
                    var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(union.UnionScenePath);

                    if (record == null || sceneAsset == null)
                    {
                        continue;
                    }

                    assetsChanged |= record.RemoveSceneUnion(union.SceneToUnionize, sceneAsset);
                }

                foreach (var replacement in persistentState.AddedReplacements)
                {
                    var record = AssetDatabase.LoadAssetAtPath<SceneRecord>(replacement.SceneRecordPath);
                    var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(replacement.ReplacementScenePath);

                    if (record == null || sceneAsset == null)
                    {
                        continue;
                    }

                    assetsChanged |= record.RemoveSceneReplacement(replacement.SceneToReplace, sceneAsset);
                }

                PersistentData.RemoveData(PersistentStateKey);
            }

            if (AssetDatabase.IsValidFolder(GeneratedContentsScenesFolder))
            {
                foreach (var recordReference in GetSceneRecords())
                {
                    var generatedUnions = recordReference.Record.SceneUnions
                        .Where(union => union.SceneUnion.StartsWith(GeneratedContentsScenesFolder + "/", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    foreach (var generatedUnion in generatedUnions)
                    {
                        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(generatedUnion.SceneUnion);
                        if (sceneAsset == null)
                        {
                            continue;
                        }

                        assetsChanged |= recordReference.Record.RemoveSceneUnion(generatedUnion.SceneToUnionize, sceneAsset);
                    }
                }

                assetsChanged |= AssetDatabase.DeleteAsset(GeneratedContentsScenesFolder);
            }

            if (!assetsChanged)
            {
                return;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void PatchBuiltSceneBundles(IEnumerable<BuiltAssetBundle> builtBundles, SceneBakeBuildData buildData)
        {
            if (buildData == null || buildData.BundlePatches.Count == 0)
            {
                return;
            }

            var patchedScenes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var builtBundleFiles = builtBundles
                .Select(bundle => bundle.File)
                .Where(file => file != null)
                .ToList();

            var sceneBundles = builtBundleFiles
                .Where(file => file.Exists && IsSceneBundle(file))
                .Concat(builtBundleFiles
                    .Select(file => file.Directory)
                    .Where(directory => directory != null && directory.Exists)
                    .GroupBy(directory => directory.FullName, StringComparer.OrdinalIgnoreCase)
                    .Select(group => group.First())
                    .SelectMany(directory => directory.GetFiles("*", SearchOption.AllDirectories))
                    .Where(IsSceneBundle))
                .GroupBy(file => file.FullName, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();

            foreach (var sceneBundle in sceneBundles)
            {
                foreach (var patchedScene in PatchSceneBundle(sceneBundle.FullName, buildData.BundlePatches))
                {
                    patchedScenes.Add(NormalizeScenePath(patchedScene));
                }
            }

            var missingScenes = buildData.BundlePatches
                .Select(scene => scene.CloneScenePath)
                .Where(scene => !patchedScenes.Contains(NormalizeScenePath(scene)))
                .Distinct()
                .ToList();

            if (missingScenes.Count > 0)
            {
                var bundleNames = sceneBundles.Count == 0
                    ? "<none discovered>"
                    : string.Join(", ", sceneBundles.Select(bundle => bundle.FullName));
                throw new InvalidOperationException(
                    $"The built scene bundle did not contain the expected clone scenes: " +
                    $"{string.Join(", ", missingScenes)}. Scene bundles searched: {bundleNames}");
            }
        }

        static bool IsSceneBundle(FileInfo file)
        {
            return file != null &&
                file.Exists &&
                Path.GetFileNameWithoutExtension(file.Name)
                    .EndsWith("_scenes_bundle", StringComparison.OrdinalIgnoreCase);
        }

        static string NormalizeScenePath(string scenePath)
        {
            return string.IsNullOrEmpty(scenePath)
                ? string.Empty
                : scenePath.Replace('\\', '/');
        }

        static IEnumerable<SceneRecordReference> GetSceneRecords()
        {
            var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var registry in RegistryTools.GetAllRegistries())
            {
                if (registry == null)
                {
                    continue;
                }

                foreach (var record in registry.Features.OfType<SceneRecord>())
                {
                    if (record == null)
                    {
                        continue;
                    }

                    var path = AssetDatabase.GetAssetPath(record);
                    if (string.IsNullOrEmpty(path) || !seenPaths.Add(path))
                    {
                        continue;
                    }

                    yield return new SceneRecordReference
                    {
                        Path = path,
                        Record = record
                    };
                }
            }
        }

        static string CreateGeneratedContentsScene(string cloneScenePath)
        {
            EnsureFolderExists(GeneratedContentsScenesFolder);

            var cloneSceneName = Path.GetFileNameWithoutExtension(cloneScenePath);
            var generatedScenePath = AssetDatabase.GenerateUniqueAssetPath($"{GeneratedContentsScenesFolder}/{cloneSceneName}__CONTENTS__.unity");

            if (!AssetDatabase.CopyAsset(cloneScenePath, generatedScenePath))
            {
                throw new InvalidOperationException($"Could not copy clone scene from {cloneScenePath} to {generatedScenePath}");
            }

            AssetDatabase.ImportAsset(generatedScenePath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.DontDownloadFromCacheServer);
            return generatedScenePath;
        }

        static void EnsureFolderExists(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var parts = folderPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0 || parts[0] != "Assets")
            {
                throw new InvalidOperationException($"Generated scene folder must live under Assets. Invalid path: {folderPath}");
            }

            var currentPath = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                var nextPath = $"{currentPath}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, parts[i]);
                }

                currentPath = nextPath;
            }
        }

        static SceneBakeBundlePatch CreateBundlePatch(string sceneRecordPath, (string DestinationScene, string GameSceneToMerge) inverseSceneUnion, Dictionary<string, int> sceneLookup, string dataFolder)
        {
            if (!sceneLookup.TryGetValue(inverseSceneUnion.GameSceneToMerge, out var levelIndex))
            {
                throw new InvalidOperationException($"Could not find Hollow Knight scene index for {inverseSceneUnion.GameSceneToMerge} in {sceneRecordPath}");
            }

            var cloneScenePath = inverseSceneUnion.DestinationScene;
            var levelPath = Path.Combine(dataFolder, $"level{levelIndex}");

            if (!File.Exists(levelPath))
            {
                throw new FileNotFoundException($"Could not find Hollow Knight scene data at {levelPath}", levelPath);
            }

            return new SceneBakeBundlePatch
            {
                CloneSceneName = Path.GetFileNameWithoutExtension(cloneScenePath),
                CloneScenePath = cloneScenePath,
                GameLevelPath = levelPath
            };
        }

        static IEnumerable<string> PatchSceneBundle(string sceneBundlePath, IReadOnlyList<SceneBakeBundlePatch> bundlePatches)
        {
            var assetsManager = new AssetsManager();
            assetsManager.LoadClassPackage(BuildTools.WeaverCoreFolder.AddSlash() + $"Libraries{Path.DirectorySeparatorChar}classdata.tpk");

            var bundleInstance = assetsManager.LoadBundleFile(sceneBundlePath);
            if (bundleInstance == null)
            {
                throw new InvalidOperationException($"Could not load scene bundle {sceneBundlePath}");
            }

            var sceneHashes = LoadSceneHashes(sceneBundlePath, bundleInstance, assetsManager);
            var bundleReplacers = new List<BundleReplacer>();
            var patchedScenePaths = new List<string>();

            foreach (var bundlePatch in bundlePatches)
            {
                if (!sceneHashes.TryGetValue(NormalizeScenePath(bundlePatch.CloneScenePath), out var sceneHash))
                {
                    continue;
                }

                var levelBytes = File.ReadAllBytes(bundlePatch.GameLevelPath);
                bundleReplacers.Add(new BundleReplacerFromMemory(sceneHash, sceneHash, true, levelBytes, levelBytes.LongLength));
                patchedScenePaths.Add(bundlePatch.CloneScenePath);
            }

            if (bundleReplacers.Count == 0)
            {
                return patchedScenePaths;
            }

            var tempOutputPath = sceneBundlePath + ".cloned";

            using (var memoryStream = new HugeMemoryStream())
            {
                using (var writer = new AssetsFileWriter(memoryStream))
                {
                    bundleInstance.file.Write(writer, bundleReplacers);
                }

                memoryStream.Position = 0;
                using (var reader = new AssetsFileReader(memoryStream))
                {
                    var modifiedBundle = new AssetBundleFile();
                    modifiedBundle.Read(reader);

                    using (var outputStream = File.Open(tempOutputPath, FileMode.Create, FileAccess.Write))
                    using (var writer = new AssetsFileWriter(outputStream))
                    {
                        bundleInstance.file.Pack(modifiedBundle.reader, writer, BuildScreen.BuildSettings.CompressionType);
                    }
                }
            }

            File.Copy(tempOutputPath, sceneBundlePath, true);
            File.Delete(tempOutputPath);
            return patchedScenePaths;
        }

        static Dictionary<string, string> LoadSceneHashes(string sceneBundlePath, BundleFileInstance bundleInstance, AssetsManager assetsManager)
        {
            var sceneHashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var bundle = bundleInstance.file;
            var entryCount = bundle.bundleInf6.dirInf.GetLength(0);

            for (int i = 0; i < entryCount; i++)
            {
                var entryName = bundle.bundleInf6.dirInf[i].name;
                if (entryName.EndsWith(".resS", StringComparison.OrdinalIgnoreCase) || entryName.EndsWith(".resource", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var assetData = BundleHelper.LoadAssetDataFromBundle(bundle, i);
                var assetsFileInstance = assetsManager.LoadAssetsFile(new MemoryStream(assetData), SceneBundleKey(sceneBundlePath, entryName), false);
                assetsManager.LoadClassDatabaseFromPackage(assetsFileInstance.file.typeTree.unityVersion);

                foreach (var info in assetsFileInstance.table.assetFileInfo)
                {
                    if (info.curFileType != (int)AssetClassID.AssetBundle)
                    {
                        continue;
                    }

                    var baseField = assetsManager.GetTypeInstance(assetsFileInstance.file, info).GetBaseField();
                    var sceneHashArray = baseField.Get("m_SceneHashes").Get("Array");
                    for (int sceneIndex = 0; sceneIndex < sceneHashArray.childrenCount; sceneIndex++)
                    {
                        var sceneEntry = sceneHashArray[sceneIndex];
                        var scenePath = sceneEntry.Get("first").GetValue().AsString();
                        var sceneHash = sceneEntry.Get("second").GetValue().AsString();

                        if (!string.IsNullOrEmpty(scenePath) && !string.IsNullOrEmpty(sceneHash))
                        {
                            sceneHashes[NormalizeScenePath(scenePath)] = sceneHash;
                        }
                    }

                    return sceneHashes;
                }
            }

            throw new InvalidOperationException($"Could not find AssetBundle metadata in built scene bundle {sceneBundlePath}");
        }

        static Dictionary<string, int> LoadSceneLookup(string dataFolder)
        {
            var assetsManager = new AssetsManager();
            assetsManager.LoadClassPackage(BuildTools.WeaverCoreFolder.AddSlash() + $"Libraries{Path.DirectorySeparatorChar}classdata.tpk");

            var globalGameManagersPath = Path.Combine(dataFolder, "globalgamemanagers");
            if (!File.Exists(globalGameManagersPath))
            {
                throw new FileNotFoundException($"Could not find Hollow Knight globalgamemanagers at {globalGameManagersPath}", globalGameManagersPath);
            }

            var assetsFile = assetsManager.LoadAssetsFile(globalGameManagersPath, false);
            assetsManager.LoadClassDatabaseFromPackage(assetsFile.file.typeTree.unityVersion);

            var lookup = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var info in assetsFile.table.assetFileInfo)
            {
                if (info.curFileType != (int)AssetClassID.BuildSettings)
                {
                    continue;
                }

                var baseField = assetsManager.GetTypeInstance(assetsFile.file, info).GetBaseField();
                var levels = baseField.Get("scenes").Get("Array");

                for (int i = 0; i < levels.childrenCount; i++)
                {
                    var scenePath = GetBuildSettingsScenePath(levels[i]);
                    if (string.IsNullOrEmpty(scenePath))
                    {
                        continue;
                    }

                    lookup[Path.GetFileNameWithoutExtension(scenePath)] = i;
                }

                return lookup;
            }

            throw new InvalidOperationException($"Could not find BuildSettings data in {globalGameManagersPath}");
        }

        static string GetBuildSettingsScenePath(AssetTypeValueField sceneField)
        {
            if (sceneField == null)
            {
                return null;
            }

            try
            {
                var directValue = sceneField.GetValue();
                if (directValue != null)
                {
                    var directString = directValue.AsString();
                    if (!string.IsNullOrEmpty(directString))
                    {
                        return directString;
                    }
                }
            }
            catch
            {
            }

            foreach (var fieldName in new[] { "path", "m_path", "scenePath" })
            {
                try
                {
                    var child = sceneField.Get(fieldName);
                    if (child == null)
                    {
                        continue;
                    }

                    var value = child.GetValue();
                    if (value == null)
                    {
                        continue;
                    }

                    var text = value.AsString();
                    if (!string.IsNullOrEmpty(text))
                    {
                        return text;
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        static string GetHollowKnightDataFolder()
        {
            var hollowKnightLocation = GameBuildSettings.Settings.HollowKnightLocation;
            if (string.IsNullOrWhiteSpace(hollowKnightLocation))
            {
                throw new InvalidOperationException("The Hollow Knight install location is not configured in the build settings.");
            }

            var lowerCasePath = Path.Combine(hollowKnightLocation, "hollow_knight_Data");
            if (Directory.Exists(lowerCasePath))
            {
                return lowerCasePath;
            }

            var titleCasePath = Path.Combine(hollowKnightLocation, "Hollow Knight_Data");
            if (Directory.Exists(titleCasePath))
            {
                return titleCasePath;
            }

            throw new DirectoryNotFoundException($"Could not find the Hollow Knight data folder under {hollowKnightLocation}");
        }

        static string SceneBundleKey(string sceneBundlePath, string entryName)
        {
            return $"{sceneBundlePath}::{entryName}";
        }
    }
}
