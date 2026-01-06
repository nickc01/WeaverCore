using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WeaverCore.Editor
{
	public class ZeroGuidFinderWindow : EditorWindow
	{
		const string ZeroGuid = "guid: 00000000000000000000000000000000";

		static readonly HashSet<string> AllowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			".prefab",
			".asset",
			".unity",
			".spriteatlas",
			".mat",
			".controller",
			".overrideController",
			".anim",
			".playable",
			".mask",
			".renderTexture"
		};

		class Result
		{
			public string Path;
			public int Line;
			public string Text;
		}

		string rootFolder = "Assets";
		Vector2 scroll;
		List<Result> results = new List<Result>();
		string status = "";

		[MenuItem("WeaverCore/Tools/Asset Audit/Find Zero GUID References")]
		static void Open()
		{
			GetWindow<ZeroGuidFinderWindow>().Show();
		}

		void OnEnable()
		{
			titleContent = new GUIContent("Zero GUID Finder");
		}

		void OnGUI()
		{
			EditorGUILayout.LabelField("Root Folder");
			EditorGUILayout.BeginHorizontal();
			rootFolder = EditorGUILayout.TextField(rootFolder);
			if (GUILayout.Button("...", GUILayout.MaxWidth(30)))
			{
				var chosen = EditorUtility.OpenFolderPanel("Select Root Folder", Application.dataPath, "");
				if (!string.IsNullOrEmpty(chosen))
				{
					rootFolder = ToProjectRelativePath(chosen);
				}
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Scan"))
			{
				Scan();
			}
			if (GUILayout.Button("Clear"))
			{
				results.Clear();
				status = "";
			}
			EditorGUILayout.EndHorizontal();

			if (!string.IsNullOrEmpty(status))
			{
				EditorGUILayout.HelpBox(status, MessageType.Info);
			}

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Results", EditorStyles.boldLabel);

			scroll = EditorGUILayout.BeginScrollView(scroll);
			foreach (var result in results)
			{
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("Ping", GUILayout.MaxWidth(50)))
				{
					var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(result.Path);
					if (obj != null)
					{
						Selection.activeObject = obj;
						EditorGUIUtility.PingObject(obj);
					}
				}

				EditorGUILayout.LabelField($"{result.Path}:{result.Line}", GUILayout.MaxWidth(position.width * 0.5f));
				EditorGUILayout.LabelField(result.Text);
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndScrollView();
		}

		void Scan()
		{
			results.Clear();

			var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
			var root = string.IsNullOrWhiteSpace(rootFolder) ? "Assets" : rootFolder.Trim();

			if (!root.StartsWith("Assets", StringComparison.OrdinalIgnoreCase))
			{
				status = "Root folder must be within Assets/.";
				return;
			}

			var allPaths = AssetDatabase.GetAllAssetPaths()
				.Where(p => p.StartsWith(root, StringComparison.OrdinalIgnoreCase))
				.Where(p => AllowedExtensions.Contains(Path.GetExtension(p)))
				.ToList();

			status = $"Scanning {allPaths.Count} files...";

			try
			{
				for (int i = 0; i < allPaths.Count; i++)
				{
					var assetPath = allPaths[i];
					EditorUtility.DisplayProgressBar("Zero GUID Scan", assetPath, (float)i / allPaths.Count);

					var fullPath = Path.Combine(projectRoot, assetPath);
					if (!File.Exists(fullPath))
					{
						continue;
					}

					try
					{
						using (var reader = new StreamReader(fullPath))
						{
							int lineNumber = 0;
							while (!reader.EndOfStream)
							{
								lineNumber++;
								var line = reader.ReadLine();
								if (line != null && line.Contains(ZeroGuid))
								{
									results.Add(new Result
									{
										Path = assetPath,
										Line = lineNumber,
										Text = line.Trim()
									});
								}
							}
						}
					}
					catch (Exception)
					{
						// Skip unreadable or binary files that happen to have a matching extension.
					}
				}
			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}

			status = results.Count == 0
				? "Scan complete. No zero GUID references found."
				: $"Scan complete. Found {results.Count} zero GUID reference(s).";
		}

		static string ToProjectRelativePath(string absolutePath)
		{
			var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
			var full = Path.GetFullPath(absolutePath);
			if (full.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
			{
				var relative = full.Substring(projectRoot.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				return relative.Replace('\\', '/');
			}
			return "Assets";
		}
	}
}
