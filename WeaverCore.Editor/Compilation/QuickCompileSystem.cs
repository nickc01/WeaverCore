using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.Settings;
using WeaverCore.Editor.Utilities;
using WeaverCore.Utilities;
using WeaverBuildTools.Commands;
using WeaverBuildTools.Enums;
using System.IO.Compression;

namespace WeaverCore.Editor.Compilation
{
    /// <summary>
    /// Provides quick compilation functionality that reuses existing asset bundles
    /// and only recompiles code assemblies, significantly reducing build time for code-only changes.
    /// </summary>
    public static class QuickCompileSystem
    {
        /// <summary>
        /// Represents an extracted embedded resource that can be re-embedded after compilation
        /// </summary>
        private class ExtractedResource
        {
            public string ResourceName { get; set; }
            public string TempFilePath { get; set; }
            public string SourceAssemblyPath { get; set; }
            public BuildTarget Platform { get; set; }
            public string Hash { get; set; }
            public bool IsCompressed { get; set; }
            public CompressionMethod OriginalCompression { get; set; }
        }

        /// <summary>
        /// Performs a quick compile of the mod, reusing existing asset bundles
        /// </summary>
        /// <param name="outputPath">The output path for the compiled mod</param>
        public static void QuickCompileMod(FileInfo outputPath)
        {
            UnboundCoroutine.Start(QuickCompileModRoutine(outputPath));
        }

        /// <summary>
        /// Coroutine for quick compiling the mod
        /// </summary>
        private static IEnumerator QuickCompileModRoutine(FileInfo outputPath)
        {
            Debug.Log("<b>Starting Quick Compile...</b>");
            
            // 1. Call BeforeQuickCompile hook
            if (BuildPipelineCustomizer.TryGetCurrentCustomizer(out var customizer))
            {
                Debug.Log("Running BeforeQuickCompile customizer hook...");
                var beforeTask = customizer.BeforeQuickCompile();
                yield return new WaitUntil(() => beforeTask.IsCompleted);
                
                if (!beforeTask.Result)
                {
                    Debug.LogError("BeforeQuickCompile customizer hook failed.");
                    yield break;
                }
            }
            
            // 2. Validate existing mod assemblies exist
            if (!HasExistingBuild(outputPath))
            {
                Debug.LogError("No existing build found with embedded asset bundles. Please run a full build first.");
                yield break;
            }

            // 3. Extract existing embedded resources
            Debug.Log("Extracting existing embedded resources...");
            List<ExtractedResource> extractedResources = null;
            
            try
            {
                extractedResources = ExtractEmbeddedResources(outputPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to extract embedded resources: {e.Message}");
                Debug.LogException(e);
                yield break;
            }
            
            if (extractedResources.Count == 0)
            {
                Debug.LogError("No embedded resources found in existing build. Please run a full build first.");
                yield break;
            }

            Debug.Log($"Extracted {extractedResources.Count} embedded resources");

            // 4. Compile only the code assemblies
            Debug.Log("Compiling code assemblies...");
            List<FileInfo> newAssemblies = null;
            bool compilationSuccessful = false;
            
            yield return CompileAssembliesOnlyRoutine(outputPath, result => 
            {
                newAssemblies = result;
                compilationSuccessful = result != null && result.Count > 0;
            });

            if (!compilationSuccessful)
            {
                Debug.LogError("No assemblies were compiled successfully.");
                CleanupTemporaryFiles(extractedResources);
                yield break;
            }

            // 5. Force garbage collection to release assembly references
            Debug.Log("Releasing assembly references...");
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            yield return new WaitForSeconds(1f); // Give time for file handles to release

            // 6. Re-embed the extracted resources
            Debug.Log("Re-embedding resources...");
            bool reEmbedSuccess = false;
            yield return ReEmbedResourcesRoutine(newAssemblies, extractedResources, success => reEmbedSuccess = success);
            
            if (!reEmbedSuccess)
            {
                Debug.LogError("Failed to re-embed resources");
                CleanupTemporaryFiles(extractedResources);
                yield break;
            }

            // 7. Call AfterQuickCompile hook
            if (customizer != null)
            {
                Debug.Log("Running AfterQuickCompile customizer hook...");
                var afterTask = customizer.AfterQuickCompile();
                yield return new WaitUntil(() => afterTask.IsCompleted);
                
                if (!afterTask.Result)
                {
                    Debug.LogError("AfterQuickCompile customizer hook failed.");
                    CleanupTemporaryFiles(extractedResources);
                    yield break;
                }
            }

            // 8. Cleanup temporary files
            CleanupTemporaryFiles(extractedResources);

            Debug.Log("<b>Quick Compile Complete!</b>");
        }

        /// <summary>
        /// Performs a quick compile of WeaverCore only, reusing existing asset bundles
        /// </summary>
        /// <param name="outputPath">The output path for WeaverCore.dll</param>
        public static void QuickCompileWeaverCore(FileInfo outputPath)
        {
            UnboundCoroutine.Start(QuickCompileWeaverCoreRoutine(outputPath));
        }

        /// <summary>
        /// Coroutine for quick compiling WeaverCore
        /// </summary>
        private static IEnumerator QuickCompileWeaverCoreRoutine(FileInfo outputPath)
        {
            Debug.Log("<b>Starting Quick Compile WeaverCore...</b>");
            
            // 1. Validate existing WeaverCore assembly exists
            if (!HasExistingWeaverCoreBuild(outputPath))
            {
                Debug.LogError("No existing WeaverCore build found with embedded asset bundles. Please run a full build first.");
                yield break;
            }

            // 2. Extract existing embedded resources
            Debug.Log("Extracting existing WeaverCore embedded resources...");
            List<ExtractedResource> extractedResources = null;
            
            try
            {
                extractedResources = ExtractWeaverCoreEmbeddedResources(outputPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to extract WeaverCore embedded resources: {e.Message}");
                Debug.LogException(e);
                yield break;
            }
            
            if (extractedResources.Count == 0)
            {
                Debug.LogError("No embedded resources found in existing WeaverCore build. Please run a full build first.");
                yield break;
            }

            Debug.Log($"Extracted {extractedResources.Count} embedded resources");

            // 3. Compile only WeaverCore assembly
            Debug.Log("Compiling WeaverCore assembly...");
            List<FileInfo> newAssemblies = null;
            bool compilationSuccessful = false;
            
            yield return CompileWeaverCoreOnlyRoutine(outputPath, result => 
            {
                newAssemblies = result;
                compilationSuccessful = result != null && result.Count > 0;
            });

            if (!compilationSuccessful)
            {
                Debug.LogError("WeaverCore assembly was not compiled successfully.");
                CleanupTemporaryFiles(extractedResources);
                yield break;
            }

            // 4. Re-embed the extracted resources
            Debug.Log("Re-embedding resources...");
            bool reEmbedSuccess = false;
            yield return ReEmbedResourcesRoutine(newAssemblies, extractedResources, success => reEmbedSuccess = success);
            
            if (!reEmbedSuccess)
            {
                Debug.LogError("Failed to re-embed resources");
                CleanupTemporaryFiles(extractedResources);
                yield break;
            }

            // 5. Rebuild WeaverCore.Game assembly
            Debug.Log("Rebuilding WeaverCore.Game assembly...");
            bool weaverGameBuildSuccess = false;
            yield return RebuildWeaverCoreGameRoutine(success => weaverGameBuildSuccess = success);
            
            if (!weaverGameBuildSuccess)
            {
                Debug.LogError("Failed to rebuild WeaverCore.Game assembly");
                CleanupTemporaryFiles(extractedResources);
                yield break;
            }

            // 6. Re-embed WeaverCore system resources
            Debug.Log("Re-embedding WeaverCore system resources...");
            try
            {
                EmbedWeaverCoreResources(outputPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to re-embed WeaverCore system resources: {e.Message}");
                Debug.LogException(e);
                CleanupTemporaryFiles(extractedResources);
                yield break;
            }

            // 7. Cleanup temporary files
            CleanupTemporaryFiles(extractedResources);

            Debug.Log("<b>Quick Compile WeaverCore Complete!</b>");
        }

        /// <summary>
        /// Checks if there's an existing build with embedded asset bundles
        /// </summary>
        private static bool HasExistingBuild(FileInfo outputPath)
        {
            var modAssembly = new FileInfo(Path.Combine(outputPath.Directory.FullName, BuildScreen.BuildSettings.ModName, $"{BuildScreen.BuildSettings.ModName}.dll"));
            var weaverCoreAssembly = new FileInfo(Path.Combine(outputPath.Directory.FullName, "WeaverCore", "WeaverCore.dll"));
            
            if (!modAssembly.Exists || !weaverCoreAssembly.Exists)
                return false;
            
            // Check if assemblies contain asset bundles
            return HasEmbeddedBundles(modAssembly) || HasEmbeddedBundles(weaverCoreAssembly);
        }

        /// <summary>
        /// Checks if there's an existing WeaverCore build with embedded asset bundles
        /// </summary>
        private static bool HasExistingWeaverCoreBuild(FileInfo outputPath)
        {
            if (!outputPath.Exists)
                return false;
            
            return HasEmbeddedBundles(outputPath);
        }

        /// <summary>
        /// Checks if an assembly contains embedded resources using Mono.Cecil
        /// </summary>
        private static bool HasEmbeddedBundles(FileInfo assemblyFile)
        {
            try
            {
                // Use Mono.Cecil to read assembly without loading it into memory
                using (var assemblyDefinition = Mono.Cecil.AssemblyDefinition.ReadAssembly(assemblyFile.FullName))
                {
                    var embeddedResources = assemblyDefinition.MainModule.Resources
                        .OfType<Mono.Cecil.EmbeddedResource>()
                        .ToArray();
                    
                    // Check for any embedded resources (not just bundles)
                    return embeddedResources.Length > 0;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to check embedded resources in {assemblyFile.Name}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Extracts embedded resources from existing mod assemblies
        /// </summary>
        private static List<ExtractedResource> ExtractEmbeddedResources(FileInfo outputPath)
        {
            var extractedResources = new List<ExtractedResource>();
            var tempDir = Path.Combine(Path.GetTempPath(), "QuickCompile_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            // Extract from both mod and WeaverCore assemblies
            var modAssembly = new FileInfo(Path.Combine(outputPath.Directory.FullName, BuildScreen.BuildSettings.ModName, $"{BuildScreen.BuildSettings.ModName}.dll"));
            var weaverCoreAssembly = new FileInfo(Path.Combine(outputPath.Directory.FullName, "WeaverCore", "WeaverCore.dll"));

            var assemblies = new[] { modAssembly, weaverCoreAssembly }.Where(f => f?.Exists == true);

            foreach (var assemblyFile in assemblies)
            {
                ExtractResourcesFromAssembly(assemblyFile, tempDir, extractedResources);
            }

            return extractedResources;
        }

        /// <summary>
        /// Extracts embedded resources from existing WeaverCore assembly
        /// </summary>
        private static List<ExtractedResource> ExtractWeaverCoreEmbeddedResources(FileInfo outputPath)
        {
            var extractedResources = new List<ExtractedResource>();
            var tempDir = Path.Combine(Path.GetTempPath(), "QuickCompile_WeaverCore_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            ExtractResourcesFromAssembly(outputPath, tempDir, extractedResources);

            return extractedResources;
        }

        /// <summary>
        /// Extracts all embedded resources from a specific assembly using Mono.Cecil
        /// </summary>
        private static void ExtractResourcesFromAssembly(FileInfo assemblyFile, string tempDir, List<ExtractedResource> extractedResources)
        {
            try
            {
                // Use Mono.Cecil to read assembly without loading it into memory
                using (var assemblyDefinition = Mono.Cecil.AssemblyDefinition.ReadAssembly(assemblyFile.FullName))
                {
                    var embeddedResources = assemblyDefinition.MainModule.Resources
                        .OfType<Mono.Cecil.EmbeddedResource>()
                        .Where(r => !r.Name.EndsWith("_meta")) // Skip meta files, they'll be handled separately
                        .ToList();

                    foreach (var embeddedResource in embeddedResources)
                    {
                        var tempFile = Path.Combine(tempDir, embeddedResource.Name);
                        
                        // Check for corresponding meta file
                        var metaResource = assemblyDefinition.MainModule.Resources
                            .OfType<Mono.Cecil.EmbeddedResource>()
                            .FirstOrDefault(r => r.Name == embeddedResource.Name + "_meta");
                        
                        bool isCompressed = false;
                        string originalHash = null;
                        CompressionMethod originalCompression = CompressionMethod.NoCompression;
                        
                        if (metaResource != null)
                        {
                            try
                            {
                                using (var metaStream = metaResource.GetResourceStream())
                                {
                                    var meta = ResourceMetaData.FromStream(metaStream);
                                    isCompressed = meta.compressed;
                                    originalHash = meta.hash;
                                    originalCompression = meta.compressed ? CompressionMethod.UseCompression : CompressionMethod.NoCompression;
                                }
                            }
                            catch (Exception metaEx)
                            {
                                Debug.LogWarning($"Failed to read meta for {embeddedResource.Name}: {metaEx.Message}");
                            }
                        }
                        
                        // Extract and decompress the resource if necessary
                        using (var resourceStream = embeddedResource.GetResourceStream())
                        {
                            if (isCompressed)
                            {
                                // Decompress the resource before writing to temp file
                                using (var decompressedStream = new GZipStream(resourceStream, CompressionMode.Decompress))
                                using (var fileStream = File.Create(tempFile))
                                {
                                    decompressedStream.CopyTo(fileStream);
                                }
                            }
                            else
                            {
                                // Write resource as-is
                                using (var fileStream = File.Create(tempFile))
                                {
                                    resourceStream.CopyTo(fileStream);
                                }
                            }
                        }
                        
                        // Calculate hash of the decompressed file if not available from meta
                        if (string.IsNullOrEmpty(originalHash))
                        {
                            originalHash = GetFileHash(tempFile);
                        }

                        extractedResources.Add(new ExtractedResource
                        {
                            ResourceName = embeddedResource.Name,
                            TempFilePath = tempFile,
                            SourceAssemblyPath = assemblyFile.FullName,
                            Platform = GetPlatformFromResourceName(embeddedResource.Name),
                            Hash = originalHash,
                            IsCompressed = isCompressed,
                            OriginalCompression = originalCompression
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to extract resources from {assemblyFile.Name}: {e.Message}");
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Gets the build target platform from a resource name
        /// </summary>
        private static BuildTarget GetPlatformFromResourceName(string resourceName)
        {
            if (resourceName.Contains(".bundle.win"))
                return BuildTarget.StandaloneWindows64;
            else if (resourceName.Contains(".bundle.mac"))
                return BuildTarget.StandaloneOSX;
            else if (resourceName.Contains(".bundle.unix"))
                return BuildTarget.StandaloneLinux64;
            else
                return BuildTarget.StandaloneWindows64; // Default
        }

        /// <summary>
        /// Gets a hash for a file
        /// </summary>
        private static string GetFileHash(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return WeaverCore.Utilities.HashUtilities.GetHash(stream);
            }
        }

        /// <summary>
        /// Compiles only the code assemblies without asset bundles
        /// </summary>
        private static IEnumerator CompileAssembliesOnlyRoutine(FileInfo outputPath, Action<List<FileInfo>> onComplete)
        {
            var compiledAssemblies = new List<FileInfo>();
            
            // Build HollowKnight assemblies (if needed)
            var hkFile = new FileInfo(Path.Combine(outputPath.Directory.FullName, "WeaverCore", "Assembly-CSharp.dll"));
            var hkFirstPassFile = new FileInfo(Path.Combine(outputPath.Directory.FullName, "WeaverCore", "Assembly-CSharp-firstpass.dll"));
            
            if (NeedsRecompile(hkFile) || NeedsRecompile(hkFirstPassFile))
            {
                Debug.Log("Building HollowKnight assemblies...");
                var hkTask = BuildTools.BuildHollowKnightAsm(hkFile);
                yield return new WaitUntil(() => hkTask.Completed);
                if (hkTask.Result.Success)
                {
                    compiledAssemblies.AddRange(hkTask.Result.OutputFiles);
                }
                else
                {
                    Debug.LogError("Failed to build HollowKnight assemblies");
                    onComplete(null);
                    yield break;
                }
            }
            else
            {
                // Add existing HK assemblies to the list
                if (hkFile.Exists) compiledAssemblies.Add(hkFile);
                if (hkFirstPassFile.Exists) compiledAssemblies.Add(hkFirstPassFile);
            }
            
            // Build WeaverCore (without asset bundles)
            var weaverCoreFile = new FileInfo(Path.Combine(outputPath.Directory.FullName, "WeaverCore", "WeaverCore.dll"));
            Debug.Log("Building WeaverCore assembly...");
            var weaverCoreTask = BuildTools.BuildPartialWeaverCore(weaverCoreFile);
            yield return new WaitUntil(() => weaverCoreTask.Completed);
            if (weaverCoreTask.Result.Success)
            {
                compiledAssemblies.AddRange(weaverCoreTask.Result.OutputFiles);
            }
            else
            {
                Debug.LogError("Failed to build WeaverCore assembly");
                onComplete(null);
                yield break;
            }
            
            // Build mod assembly
            var modFile = new FileInfo(Path.Combine(outputPath.Directory.FullName, BuildScreen.BuildSettings.ModName, $"{BuildScreen.BuildSettings.ModName}.dll"));
            Debug.Log("Building mod assembly...");
            var modTask = BuildModAssemblyOnly(modFile, weaverCoreTask.Result.OutputFiles);
            yield return new WaitUntil(() => modTask.Completed);
            if (modTask.Result.Success)
            {
                compiledAssemblies.AddRange(modTask.Result.OutputFiles);
            }
            else
            {
                Debug.LogError("Failed to build mod assembly");
                onComplete(null);
                yield break;
            }

            onComplete(compiledAssemblies);
        }

        /// <summary>
        /// Compiles only WeaverCore assembly without asset bundles
        /// </summary>
        private static IEnumerator CompileWeaverCoreOnlyRoutine(FileInfo outputPath, Action<List<FileInfo>> onComplete)
        {
            var compiledAssemblies = new List<FileInfo>();
            
            // Build WeaverCore (without asset bundles)
            Debug.Log("Building WeaverCore assembly...");
            var weaverCoreTask = BuildTools.BuildPartialWeaverCore(outputPath);
            yield return new WaitUntil(() => weaverCoreTask.Completed);
            if (weaverCoreTask.Result.Success)
            {
                compiledAssemblies.AddRange(weaverCoreTask.Result.OutputFiles);
            }
            else
            {
                Debug.LogError("Failed to build WeaverCore assembly");
                onComplete(null);
                yield break;
            }

            onComplete(compiledAssemblies);
        }

        /// <summary>
        /// Rebuilds WeaverCore.Game assembly
        /// </summary>
        private static IEnumerator RebuildWeaverCoreGameRoutine(Action<bool> onComplete)
        {
            Debug.Log("Building WeaverCore.Game assembly...");
            
            // Use a simple approach - start the build and monitor a file watcher or timeout
            var sep = Path.DirectorySeparatorChar;
            var weaverGameOutputPath = new FileInfo(BuildTools.WeaverCoreFolder.AddSlash() + $"Other Projects~{sep}WeaverCore.Game{sep}WeaverCore.Game{sep}bin{sep}WeaverCore.Game.dll");
            
            // Store the last write time to detect when build completes
            DateTime lastWriteTime;
            bool buildStarted = false;
            
            try
            {
                lastWriteTime = weaverGameOutputPath.Exists ? weaverGameOutputPath.LastWriteTime : DateTime.MinValue;
                
                // Start the build asynchronously
                BuildTools.BuildWeaverCoreGameAsm(null);
                buildStarted = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while starting WeaverCore.Game build: {e.Message}");
                Debug.LogException(e);
                onComplete(false);
                yield break;
            }
            
            if (!buildStarted)
            {
                onComplete(false);
                yield break;
            }
            
            // Wait for the build to complete by monitoring the output file
            var timeout = 60f; // 60 second timeout
            var elapsed = 0f;
            
            while (elapsed < timeout)
            {
                yield return new WaitForSeconds(1f);
                elapsed += 1f;
                
                try
                {
                    weaverGameOutputPath.Refresh();
                    
                    if (weaverGameOutputPath.Exists && weaverGameOutputPath.LastWriteTime > lastWriteTime)
                    {
                        // File was updated, build completed
                        Debug.Log("WeaverCore.Game assembly built successfully");
                        onComplete(true);
                        yield break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Exception while monitoring WeaverCore.Game build: {e.Message}");
                    Debug.LogException(e);
                    onComplete(false);
                    yield break;
                }
            }
            
            // Timeout occurred
            Debug.LogError("WeaverCore.Game build timed out after 60 seconds");
            onComplete(false);
        }

        /// <summary>
        /// Builds only the mod assembly without asset bundles
        /// </summary>
        private static IAsyncBuildTask<BuildTools.BuildOutput> BuildModAssemblyOnly(FileInfo outputPath, List<FileInfo> dependencies)
        {
            var task = new BuildTask<BuildTools.BuildOutput>();
            UnboundCoroutine.Start(BuildModAssemblyRoutine(outputPath, dependencies, task));
            return task;
        }

        /// <summary>
        /// Coroutine for building mod assembly
        /// </summary>
        private static IEnumerator BuildModAssemblyRoutine(FileInfo outputPath, List<FileInfo> dependencies, BuildTask<BuildTools.BuildOutput> task)
        {
            var parameters = new BuildParameters
            {
                BuildPath = outputPath,
                Scripts = ScriptFinder.FindAssemblyScripts("Assembly-CSharp"),
                Defines = new List<string> { "GAME_BUILD" },
                ExcludedReferences = new List<string>
                {
                    "Library/ScriptAssemblies/HollowKnight.dll",
                    "Library/ScriptAssemblies/HollowKnight.FirstPass.dll",
                    "Library/ScriptAssemblies/JUNK.dll",
                    "Library/ScriptAssemblies/WeaverCore.dll",
                    "Library/ScriptAssemblies/Assembly-CSharp.dll"
                }
            };

            foreach (var dependency in dependencies)
            {
                parameters.AssemblyReferences.Add(dependency.FullName);
            }

            yield return BuildAssembly(parameters, BuildPresetType.Game, task);
        }

        /// <summary>
        /// Build task implementation
        /// </summary>
        private class BuildTask<T> : IAsyncBuildTask<T>
        {
            public T Result { get; set; }
            public bool Completed { get; set; }
            public IAsyncBuildTask<T> PreviousTask { get; set; }
            object IAsyncBuildTask.Result { get { return Result; } set { Result = (T)value; } }
            IAsyncBuildTask IAsyncBuildTask.PreviousTask { get { return PreviousTask; } set { PreviousTask = (IAsyncBuildTask<T>)value; } }
        }

        /// <summary>
        /// Build parameters class
        /// </summary>
        private class BuildParameters
        {
            public FileInfo BuildPath { get; set; }
            public List<string> Scripts { get; set; } = new List<string>();
            public List<string> AssemblyReferences { get; set; } = new List<string>();
            public List<string> ExcludedReferences { get; set; } = new List<string>();
            public List<string> Defines { get; set; } = new List<string>();
            public BuildTarget Target { get; set; } = BuildTarget.StandaloneWindows;
            public BuildTargetGroup Group { get; set; } = BuildTargetGroup.Standalone;
            
            public DirectoryInfo BuildDirectory => BuildPath.Directory;
            public string FileName => BuildPath.Name;
        }

        /// <summary>
        /// Builds an assembly using the existing BuildTools methodology
        /// </summary>
        private static IEnumerator BuildAssembly(BuildParameters parameters, BuildPresetType presetType, BuildTask<BuildTools.BuildOutput> task)
        {
            task.Completed = false;
            if (task.Result == null)
            {
                task.Result = new BuildTools.BuildOutput();
            }
            else
            {
                task.Result.Success = false;
            }

            var builder = new AssemblyCompiler();
            builder.BuildDirectory = parameters.BuildDirectory;
            builder.FileName = parameters.FileName;
            builder.Scripts = parameters.Scripts;
            builder.Target = parameters.Target;
            builder.TargetGroup = parameters.Group;
            builder.References = parameters.AssemblyReferences;
            builder.ExcludedReferences = parameters.ExcludedReferences;
            builder.Defines = parameters.Defines;

            if (builder.Scripts == null || builder.Scripts.Count == 0)
            {
                Debug.LogError("There are no scripts to build");
                task.Result.Success = false;
                task.Completed = true;
                yield break;
            }

            if (presetType == BuildPresetType.Game || presetType == BuildPresetType.Editor)
            {
                builder.AddUnityReferences();
            }
            if (presetType == BuildPresetType.Game)
            {
                builder.RemoveEditorReferences();
            }

            if (!parameters.BuildPath.Directory.Exists)
            {
                parameters.BuildPath.Directory.Create();
            }
            if (parameters.BuildPath.Exists)
            {
                parameters.BuildPath.Delete();
            }

            AssemblyCompiler.OutputDetails output = new AssemblyCompiler.OutputDetails();
            yield return builder.Build(output);
            if (output.Success)
            {
                task.Result.OutputFiles.Add(parameters.BuildPath);
            }
            task.Result.Success = output.Success;
            task.Completed = true;
        }


        /// <summary>
        /// Checks if an assembly needs recompilation
        /// </summary>
        private static bool NeedsRecompile(FileInfo assemblyFile)
        {
            // For Quick Compile, we generally don't want to rebuild HK assemblies
            // Only rebuild if they don't exist
            return !assemblyFile.Exists;
        }

        /// <summary>
        /// Re-embeds all extracted resources into the newly compiled assemblies using batch embedding
        /// </summary>
        private static IEnumerator ReEmbedResourcesRoutine(List<FileInfo> newAssemblies, List<ExtractedResource> extractedResources, System.Action<bool> onComplete)
        {
            var validResources = extractedResources
                .Where(r => newAssemblies.Any(a => Path.GetFileName(a.FullName).Equals(Path.GetFileName(r.SourceAssemblyPath), StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (validResources.Count == 0)
            {
                Debug.LogWarning("No resources to embed");
                onComplete(true);
                yield break;
            }

            var totalResources = validResources.Count;
            var completedAssemblies = 0;
            var errors = new ConcurrentBag<Exception>();
            
            // Create assembly lookup for faster access, handling duplicates by taking the first occurrence
            var assemblyLookup = newAssemblies
                .GroupBy(a => Path.GetFileName(a.FullName), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            // Group resources by target assembly for batch processing
            var resourcesByAssembly = validResources
                .GroupBy(r => Path.GetFileName(r.SourceAssemblyPath), StringComparer.OrdinalIgnoreCase)
                .Where(g => assemblyLookup.ContainsKey(g.Key))
                .ToList();

            var totalAssemblies = resourcesByAssembly.Count;

            try
            {
                // Process each assembly in parallel using batch embedding
                var tasks = resourcesByAssembly.Select(assemblyGroup => Task.Run(() =>
                {
                    var assemblyFileName = assemblyGroup.Key;
                    var targetAssembly = assemblyLookup[assemblyFileName];
                    
                    try
                    {
                        // Convert to batch format, excluding meta files (they'll be auto-generated)
                        var batchResources = assemblyGroup
                            .Where(r => !r.ResourceName.EndsWith("_meta"))
                            .Select(r => new EmbedResourceBatchCMD.ResourceToEmbed
                            {
                                ResourceName = r.ResourceName,
                                FilePath = r.TempFilePath,
                                Hash = r.Hash,
                                Compression = r.OriginalCompression
                            }).ToList();

                        // Embed all resources for this assembly in one operation
                        EmbedResourceBatchCMD.EmbedResourcesBatch(targetAssembly.FullName, batchResources);
                        
                        Interlocked.Increment(ref completedAssemblies);
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex);
                        Debug.LogError($"Failed to batch embed resources into {assemblyFileName}: {ex.Message}");
                        Interlocked.Increment(ref completedAssemblies);
                    }
                })).ToList();

                Debug.Log($"Started {tasks.Count} batch embedding tasks for {totalResources} resources across {totalAssemblies} assemblies");

                // Wait for all tasks to complete while updating progress
                while (completedAssemblies < totalAssemblies)
                {
                    var progressPercent = (float)completedAssemblies / totalAssemblies;
                    
                    EditorUtility.DisplayProgressBar(
                        "Batch Embedding Resources", 
                        $"Completed {completedAssemblies}/{totalAssemblies} assemblies ({totalResources} total resources)", 
                        progressPercent);
                    
                    yield return new WaitForSeconds(0.1f); // Update every 100ms
                }

                // Wait for all tasks to fully complete
                yield return new WaitUntil(() => tasks.All(t => t.IsCompleted));
                
                // Final progress update
                EditorUtility.DisplayProgressBar("Batch Embedding Resources", $"Completed all {totalAssemblies} assemblies", 1.0f);
                yield return new WaitForSeconds(0.1f); // Brief pause to show completion
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            bool success = errors.IsEmpty;
            if (!success)
            {
                Debug.LogError($"Failed to embed resources in {errors.Count} assemblies");
            }
            
            onComplete(success);
        }


        /// <summary>
        /// Embeds WeaverCore resources (assemblies and native libraries)
        /// </summary>
        private static void EmbedWeaverCoreResources(FileInfo weaverCoreDLL)
        {
            var sep = Path.DirectorySeparatorChar;

            var weaverGameLocation = new FileInfo(BuildTools.WeaverCoreFolder.AddSlash() + $"Other Projects~{sep}WeaverCore.Game{sep}WeaverCore.Game{sep}bin{sep}WeaverCore.Game.dll");
            var harmonyLocation = new FileInfo(BuildTools.WeaverCoreFolder.AddSlash() + $"Libraries{sep}in-game~{sep}0Harmony.dll");

            var ktxUnityWindows = new FileInfo($"{BuildTools.WeaverCoreFolder.AddSlash()}Other Tools{sep}KtxUnity{sep}Runtime{sep}Plugins{sep}x86_64{sep}ktx_unity.dll");
            var ktxUnityMac = new FileInfo($"{BuildTools.WeaverCoreFolder.AddSlash()}Other Tools{sep}KtxUnity{sep}Runtime{sep}Plugins{sep}x86_64{sep}ktx_unity.bundle{sep}Contents{sep}MacOS{sep}ktx_unity");
            var ktxUnityLinux = new FileInfo($"{BuildTools.WeaverCoreFolder.AddSlash()}Other Tools{sep}KtxUnity{sep}Runtime{sep}Plugins{sep}x86_64{sep}libktx_unity.so");

            EmbedResourceCMD.EmbedResource(weaverCoreDLL.FullName, weaverGameLocation.FullName, "WeaverCore.Game", compression: CompressionMethod.NoCompression);
            EmbedResourceCMD.EmbedResource(weaverCoreDLL.FullName, harmonyLocation.FullName, "0Harmony", compression: CompressionMethod.NoCompression);

            EmbedResourceCMD.EmbedResource(weaverCoreDLL.FullName, ktxUnityWindows.FullName, "ktx_unity.windows.dll", compression: CompressionMethod.NoCompression);
            EmbedResourceCMD.EmbedResource(weaverCoreDLL.FullName, ktxUnityMac.FullName, "ktx_unity.mac.dylib", compression: CompressionMethod.NoCompression);
            EmbedResourceCMD.EmbedResource(weaverCoreDLL.FullName, ktxUnityLinux.FullName, "ktx_unity.linux.so", compression: CompressionMethod.NoCompression);
        }

        /// <summary>
        /// Cleans up temporary files
        /// </summary>
        private static void CleanupTemporaryFiles(List<ExtractedResource> extractedResources)
        {
            try
            {
                var tempDirectories = extractedResources
                    .Select(r => Path.GetDirectoryName(r.TempFilePath))
                    .Distinct()
                    .ToList();

                foreach (var tempDir in tempDirectories)
                {
                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, true);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to cleanup temporary files: {e.Message}");
            }
        }

        /// <summary>
        /// Validates that a quick compile is possible for the given output path
        /// </summary>
        public static bool CanQuickCompile(FileInfo outputPath)
        {
            return HasExistingBuild(outputPath);
        }

        /// <summary>
        /// Validates that a quick compile is possible for WeaverCore
        /// </summary>
        public static bool CanQuickCompileWeaverCore(FileInfo outputPath)
        {
            return HasExistingWeaverCoreBuild(outputPath);
        }
    }
}
