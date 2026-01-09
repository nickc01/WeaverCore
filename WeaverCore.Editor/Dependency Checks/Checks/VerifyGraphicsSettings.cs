using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace WeaverCore.Editor
{    
    /// <summary>
    /// Makes sure the graphics settings are configured properly
    /// </summary>
    class VerifyGraphicsSettings : DependencyCheck
    {
        struct ShaderEntry
        {
            public long FileId;
            public string Guid;
            public int Type;

            public ShaderEntry(long fileId, string guid, int type)
            {
                FileId = fileId;
                Guid = guid ?? string.Empty;
                Type = type;
            }

            public override string ToString()
            {
                if (string.IsNullOrEmpty(Guid))
                {
                    return $"- {{fileID: {FileId}}}";
                }
                return $"- {{fileID: {FileId}, guid: {Guid}, type: {Type}}}";
            }
        }

        static readonly ShaderEntry[] ExpectedAlwaysIncludedShaders = new[]
        {
            new ShaderEntry(7, "0000000000000000f000000000000000", 0),
            new ShaderEntry(15104, "0000000000000000f000000000000000", 0),
            new ShaderEntry(15105, "0000000000000000f000000000000000", 0),
            new ShaderEntry(15106, "0000000000000000f000000000000000", 0),
            new ShaderEntry(0, string.Empty, -1),
            new ShaderEntry(10770, "0000000000000000f000000000000000", 0),
            new ShaderEntry(10783, "0000000000000000f000000000000000", 0),
        };

        public override void StartCheck(Action<DependencyCheckResult> finishCheck)
        {
            SerializedObject graphicsSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);

            var AIS = graphicsSettings.FindProperty("m_AlwaysIncludedShaders");
            for (int i = 0; i < AIS.arraySize; i++)
            {
                var element = AIS.GetArrayElementAtIndex(i).objectReferenceValue;
                if (element != null && element.name == "Sprites/Default")
                {
                    AIS.DeleteArrayElementAtIndex(i);
                    graphicsSettings.ApplyModifiedProperties();
                    break;
                }
            }

            graphicsSettings.FindProperty("m_TransparencySortMode").intValue = 3;
            graphicsSettings.FindProperty("m_TransparencySortAxis").vector3Value = Vector3.forward;
            graphicsSettings.ApplyModifiedProperties();

            if (!VerifyAlwaysIncludedShaders(out bool fixedList))
            {
                finishCheck(DependencyCheckResult.Error);
                return;
            }

            finishCheck(fixedList ? DependencyCheckResult.RequiresReload : DependencyCheckResult.Complete);
        }

        static bool VerifyAlwaysIncludedShaders(out bool fixedList)
        {
            fixedList = false;
            const string graphicsSettingsPath = "ProjectSettings/GraphicsSettings.asset";

            if (!File.Exists(graphicsSettingsPath))
            {
                Debug.LogError($"GraphicsSettings.asset was not found at \"{graphicsSettingsPath}\"");
                return false;
            }

            var lines = File.ReadAllLines(graphicsSettingsPath);
            var actualEntries = new List<ShaderEntry>();

            bool inList = false;
            foreach (string rawLine in lines)
            {
                string trimmed = rawLine.TrimStart();
                if (!inList)
                {
                    if (trimmed.StartsWith("m_AlwaysIncludedShaders:"))
                    {
                        inList = true;
                    }
                    continue;
                }

                if (!trimmed.StartsWith("- "))
                {
                    break;
                }

                var entry = ParseShaderEntry(trimmed);
                actualEntries.Add(entry);
            }

            bool matches = actualEntries.Count == ExpectedAlwaysIncludedShaders.Length;
            if (matches)
            {
                for (int i = 0; i < ExpectedAlwaysIncludedShaders.Length; i++)
                {
                    var expected = ExpectedAlwaysIncludedShaders[i];
                    var actual = actualEntries[i];
                    if (expected.FileId != actual.FileId ||
                        !string.Equals(expected.Guid, actual.Guid, StringComparison.OrdinalIgnoreCase) ||
                        expected.Type != actual.Type)
                    {
                        matches = false;
                        break;
                    }
                }
            }

            if (matches)
            {
                return true;
            }

            LogAlwaysIncludedShaderMismatch(actualEntries);
            if (TryRewriteAlwaysIncludedShaders(lines, graphicsSettingsPath))
            {
                fixedList = true;
                AssetDatabase.Refresh();
                return true;
            }

            return false;
        }

        static ShaderEntry ParseShaderEntry(string line)
        {
            long fileId = 0;
            string guid = string.Empty;
            int type = -1;

            var fileIdMatch = Regex.Match(line, @"fileID:\s*(-?\d+)");
            if (fileIdMatch.Success)
            {
                long.TryParse(fileIdMatch.Groups[1].Value, out fileId);
            }

            var guidMatch = Regex.Match(line, @"guid:\s*([0-9a-fA-F]+)");
            if (guidMatch.Success)
            {
                guid = guidMatch.Groups[1].Value;
            }

            var typeMatch = Regex.Match(line, @"type:\s*(\d+)");
            if (typeMatch.Success)
            {
                int.TryParse(typeMatch.Groups[1].Value, out type);
            }

            return new ShaderEntry(fileId, guid, type);
        }

        static void LogAlwaysIncludedShaderMismatch(List<ShaderEntry> actualEntries)
        {
            var builder = new StringBuilder();
            builder.AppendLine("GraphicsSettings.asset m_AlwaysIncludedShaders does not match the expected list.");
            builder.AppendLine("Expected:");
            foreach (var entry in ExpectedAlwaysIncludedShaders)
            {
                builder.AppendLine($"  {entry}");
            }
            builder.AppendLine("Actual:");
            foreach (var entry in actualEntries)
            {
                builder.AppendLine($"  {entry}");
            }
            Debug.LogError(builder.ToString());
        }

        static bool TryRewriteAlwaysIncludedShaders(string[] lines, string graphicsSettingsPath)
        {
            int listStartIndex = -1;
            string indent = string.Empty;
            for (int i = 0; i < lines.Length; i++)
            {
                string trimmed = lines[i].TrimStart();
                if (trimmed.StartsWith("m_AlwaysIncludedShaders:"))
                {
                    listStartIndex = i;
                    indent = lines[i].Substring(0, lines[i].Length - trimmed.Length);
                    break;
                }
            }

            if (listStartIndex < 0)
            {
                Debug.LogError("GraphicsSettings.asset does not contain m_AlwaysIncludedShaders.");
                return false;
            }

            var output = new List<string>(lines.Length + ExpectedAlwaysIncludedShaders.Length);
            for (int i = 0; i < listStartIndex + 1; i++)
            {
                output.Add(lines[i]);
            }

            foreach (var entry in ExpectedAlwaysIncludedShaders)
            {
                output.Add($"{indent}- {entry.ToString().Substring(2)}");
            }

            int index = listStartIndex + 1;
            while (index < lines.Length && lines[index].TrimStart().StartsWith("- "))
            {
                index++;
            }

            for (int i = index; i < lines.Length; i++)
            {
                output.Add(lines[i]);
            }

            File.WriteAllLines(graphicsSettingsPath, output);
            Debug.LogWarning("GraphicsSettings.asset m_AlwaysIncludedShaders was corrected to the expected list.");
            return true;
        }
    }
}
