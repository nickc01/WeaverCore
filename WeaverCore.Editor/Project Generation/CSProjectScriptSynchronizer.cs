using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;

namespace WeaverCore.Editor
{
    public sealed class CSProjectScriptSynchronizer : AssetPostprocessor
    {
        private const string CompileItemPattern = @"<Compile\s+Include=""([^""]+)""\s*/?>";

        private static bool synchronizationQueued;
        private static bool fullSynchronizationRequested;
        private static Dictionary<string, List<string>> scriptPathsByAssembly;
        private static readonly HashSet<string> pendingScriptPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        [InitializeOnLoadMethod]
        private static void QueueSynchronizationOnLoad()
        {
            QueueSynchronization(true);
        }

        [MenuItem("WeaverCore/Sync C# Project Files")]
        private static void SynchronizeFromMenu()
        {
            SynchronizeProjectFiles();
        }

        /// <summary>
        /// Unity calls this whenever it generates a .csproj.
        /// </summary>
        public static string OnGeneratedCSProject(string path, string content)
        {
            string assemblyName = Path.GetFileNameWithoutExtension(path);
            Dictionary<string, List<string>> pathsByAssembly = GetScriptPathsByAssembly();

            return AddMissingScripts(
                assemblyName,
                content,
                pathsByAssembly.TryGetValue(assemblyName, out List<string> scriptPaths)
                    ? scriptPaths
                    : Enumerable.Empty<string>());
        }

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (string path in importedAssets ?? Array.Empty<string>())
            {
                AddPendingScript(path);
            }

            foreach (string path in movedAssets ?? Array.Empty<string>())
            {
                AddPendingScript(path);
            }
        }

        private static void AddPendingScript(string path)
        {
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            pendingScriptPaths.Add(path.Replace('\\', '/'));
            scriptPathsByAssembly = null;
            QueueSynchronization(false);
        }

        private static void QueueSynchronization(bool synchronizeAllProjects)
        {
            fullSynchronizationRequested |= synchronizeAllProjects;

            if (synchronizationQueued)
            {
                return;
            }

            synchronizationQueued = true;
            EditorApplication.delayCall += RunQueuedSynchronization;
        }

        private static void RunQueuedSynchronization()
        {
            synchronizationQueued = false;

            bool synchronizeAllProjects = fullSynchronizationRequested;
            fullSynchronizationRequested = false;

            string[] changedScripts = pendingScriptPaths.ToArray();
            pendingScriptPaths.Clear();

            if (synchronizeAllProjects)
            {
                SynchronizeProjectFiles();
            }
            else
            {
                SynchronizeChangedScripts(changedScripts);
            }
        }

        private static void SynchronizeProjectFiles()
        {
            string projectRoot = Directory.GetParent(UnityEngine.Application.dataPath).FullName;
            Dictionary<string, List<string>> pathsByAssembly = GetScriptPathsByAssembly(true);

            foreach (string projectPath in Directory.GetFiles(projectRoot, "*.csproj", SearchOption.TopDirectoryOnly))
            {
                string assemblyName = Path.GetFileNameWithoutExtension(projectPath);
                pathsByAssembly.TryGetValue(assemblyName, out List<string> scriptPaths);
                SynchronizeProjectFile(projectPath, assemblyName, scriptPaths ?? Enumerable.Empty<string>());
            }
        }

        private static void SynchronizeChangedScripts(IEnumerable<string> changedScripts)
        {
            string projectRoot = Directory.GetParent(UnityEngine.Application.dataPath).FullName;
            var scriptsByAssembly = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (string scriptPath in changedScripts)
            {
                string assemblyName = CompilationPipeline.GetAssemblyNameFromScriptPath(scriptPath);
                if (string.IsNullOrEmpty(assemblyName))
                {
                    continue;
                }

                if (!scriptsByAssembly.TryGetValue(assemblyName, out List<string> scriptPaths))
                {
                    scriptPaths = new List<string>();
                    scriptsByAssembly.Add(assemblyName, scriptPaths);
                }

                scriptPaths.Add(scriptPath);
            }

            foreach (KeyValuePair<string, List<string>> assemblyScripts in scriptsByAssembly)
            {
                string projectPath = Path.Combine(projectRoot, assemblyScripts.Key + ".csproj");
                if (File.Exists(projectPath))
                {
                    SynchronizeProjectFile(projectPath, assemblyScripts.Key, assemblyScripts.Value);
                }
            }
        }

        private static void SynchronizeProjectFile(string projectPath, string assemblyName, IEnumerable<string> scriptPaths)
        {
            string content = File.ReadAllText(projectPath);
            string updatedContent = AddMissingScripts(assemblyName, content, scriptPaths);

            if (updatedContent == content)
            {
                return;
            }

            File.WriteAllText(projectPath, updatedContent, new UTF8Encoding(true));
        }

        private static string AddMissingScripts(string assemblyName, string content, IEnumerable<string> scriptPaths)
        {
            if (string.IsNullOrEmpty(assemblyName) || string.IsNullOrEmpty(content))
            {
                return content;
            }

            Match compileItem = Regex.Match(content, CompileItemPattern);
            if (!compileItem.Success)
            {
                return content;
            }

            MatchCollection existingCompileItems = Regex.Matches(content, CompileItemPattern);
            var existingPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match existingCompileItem in existingCompileItems)
            {
                existingPaths.Add(existingCompileItem.Groups[1].Value.Replace('\\', '/'));
            }

            List<string> missingScripts = scriptPaths
                .Where(path => !existingPaths.Contains(SecurityElement.Escape(path)))
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (missingScripts.Count == 0)
            {
                return content;
            }

            string newline = content.Contains("\r\n") ? "\r\n" : "\n";
            int compileItemLineStart = content.LastIndexOf('\n', compileItem.Index);
            string indentation = content.Substring(compileItemLineStart + 1, compileItem.Index - compileItemLineStart - 1);
            string compileItems = string.Join(
                newline,
                missingScripts.Select(path =>
                    $"{indentation}<Compile Include=\"{SecurityElement.Escape(path)}\" />"));

            int closingItemGroup = content.IndexOf("</ItemGroup>", compileItem.Index, StringComparison.Ordinal);
            if (closingItemGroup < 0)
            {
                return content;
            }

            return content.Insert(closingItemGroup, compileItems + newline);
        }

        private static Dictionary<string, List<string>> GetScriptPathsByAssembly(bool refresh = false)
        {
            if (!refresh && scriptPathsByAssembly != null)
            {
                return scriptPathsByAssembly;
            }

            var pathsByAssembly = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            string projectRoot = Directory.GetParent(UnityEngine.Application.dataPath).FullName
                .Replace('\\', '/')
                .TrimEnd('/') + "/";

            foreach (Assembly assembly in GetScriptAssemblies())
            {
                if (string.IsNullOrEmpty(assembly.name))
                {
                    continue;
                }

                if (!pathsByAssembly.TryGetValue(assembly.name, out List<string> scriptPaths))
                {
                    scriptPaths = new List<string>();
                    pathsByAssembly.Add(assembly.name, scriptPaths);
                }

                foreach (string sourceFile in assembly.sourceFiles ?? Array.Empty<string>())
                {
                    if (sourceFile.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    {
                        string normalizedPath = sourceFile.Replace('\\', '/');
                        if (Path.IsPathRooted(normalizedPath))
                        {
                            if (!normalizedPath.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            normalizedPath = normalizedPath.Substring(projectRoot.Length);
                        }

                        if (!scriptPaths.Any(path => string.Equals(path, normalizedPath, StringComparison.OrdinalIgnoreCase)))
                        {
                            scriptPaths.Add(normalizedPath);
                        }
                    }
                }
            }

            scriptPathsByAssembly = pathsByAssembly;
            return pathsByAssembly;
        }

        private static IEnumerable<Assembly> GetScriptAssemblies()
        {
            foreach (Assembly assembly in CompilationPipeline.GetAssemblies(AssembliesType.Player))
            {
                yield return assembly;
            }

            foreach (Assembly assembly in CompilationPipeline.GetAssemblies(AssembliesType.Editor))
            {
                yield return assembly;
            }
        }
    }
}
