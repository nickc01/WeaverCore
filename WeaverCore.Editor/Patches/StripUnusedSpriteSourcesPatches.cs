using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using UnityEditor;
using WeaverCore;
using WeaverCore.Attributes;

namespace WeaverCore.Editor.Patches
{
	static class StripUnusedSpriteSourcesPatches
	{
		const string ZeroGuidString = "00000000000000000000000000000000";
		const string LogFilePath = "/tmp/build_task_log.txt";
		static readonly HashSet<string> LoggedIssues = new HashSet<string>();
		static bool loggedPatchActive;
		static bool patchApplied;
		static bool patchAttempted;

		[InitializeOnLoadMethod]
		static void EnsurePatched()
		{
			if (patchAttempted)
			{
				return;
			}

			try
			{
				patchAttempted = true;
				var patcher = HarmonyPatcher.Create("com.weavercore.sbp.stripunused");
				OnHarmonyPatch(patcher);
				LogToFile("=== SBP patch initialized ===");
			}
			catch (Exception e)
			{
				LogToFile("StripUnusedSpriteSourcesPatches failed to initialize: " + e);
			}
		}

		[OnHarmonyPatch]
		static void OnHarmonyPatch(HarmonyPatcher patcher)
		{
			try
			{
				if (patchApplied)
				{
					return;
				}
				patchApplied = true;
				var taskType = FindType("UnityEditor.Build.Pipeline.Tasks.StripUnusedSpriteSources");
				if (taskType == null)
				{
					LogToFile("StripUnusedSpriteSourcesPatches: Unable to find StripUnusedSpriteSources.");
					return;
				}

				var runMethod = taskType.GetMethod("Run", BindingFlags.Instance | BindingFlags.Public);
				var runPrefix = typeof(StripUnusedSpriteSourcesPatches).GetMethod(nameof(RunPrefix), BindingFlags.Static | BindingFlags.NonPublic);
				if (runMethod != null && runPrefix != null)
				{
					patcher.Patch(runMethod, runPrefix, null);
				}

				var setOutputMethod = taskType.GetMethod("SetOutputInformation", BindingFlags.Instance | BindingFlags.NonPublic);
				var setOutputPrefix = typeof(StripUnusedSpriteSourcesPatches).GetMethod(nameof(SetOutputInformationPrefix), BindingFlags.Static | BindingFlags.NonPublic);
				if (setOutputMethod != null && setOutputPrefix != null)
				{
					patcher.Patch(setOutputMethod, setOutputPrefix, null);
				}

				var calcType = FindType("UnityEditor.Build.Pipeline.Tasks.CalculateAssetDependencyData");
				if (calcType != null)
				{
					var calcRun = calcType.GetMethod("Run", BindingFlags.Instance | BindingFlags.Public);
					var calcPostfix = typeof(StripUnusedSpriteSourcesPatches).GetMethod(nameof(CalculateAssetDependencyDataPostfix), BindingFlags.Static | BindingFlags.NonPublic);
					if (calcRun != null && calcPostfix != null)
					{
						patcher.Patch(calcRun, null, calcPostfix);
					}
				}

				var calcSceneType = FindType("UnityEditor.Build.Pipeline.Tasks.CalculateSceneDependencyData");
				if (calcSceneType != null)
				{
					var calcRun = calcSceneType.GetMethod("Run", BindingFlags.Instance | BindingFlags.Public);
					var calcPostfix = typeof(StripUnusedSpriteSourcesPatches).GetMethod(nameof(CalculateSceneDependencyDataPostfix), BindingFlags.Static | BindingFlags.NonPublic);
					if (calcRun != null && calcPostfix != null)
					{
						patcher.Patch(calcRun, null, calcPostfix);
					}
				}

				var calcCustomType = FindType("UnityEditor.Build.Pipeline.Tasks.CalculateCustomDependencyData");
				if (calcCustomType != null)
				{
					var calcRun = calcCustomType.GetMethod("Run", BindingFlags.Instance | BindingFlags.Public);
					var calcPostfix = typeof(StripUnusedSpriteSourcesPatches).GetMethod(nameof(CalculateCustomDependencyDataPostfix), BindingFlags.Static | BindingFlags.NonPublic);
					if (calcRun != null && calcPostfix != null)
					{
						patcher.Patch(calcRun, null, calcPostfix);
					}
				}
			}
			catch (Exception e)
			{
				LogToFile("StripUnusedSpriteSourcesPatches failed to apply: " + e);
			}
		}

		static bool RunPrefix(object __instance)
		{
			try
			{
				LogPatchActiveOnce();
				var spriteData = GetField<object>(__instance, "m_SpriteData");
				if (spriteData == null)
				{
					return true;
				}

				var dependencyData = GetField<object>(__instance, "m_DependencyData");
				var dependencyAssetInfo = GetDependencyAssetInfo(dependencyData);
				if (dependencyAssetInfo == null)
				{
					LogToFile("SBP StripUnusedSpriteSources: dependency AssetInfo unavailable during Run.");
				}
				else
				{
					LogZeroGuidInDependencyDataOnce(dependencyAssetInfo);
				}

				var importerDataDict = GetSpriteImporterData(spriteData);
				if (importerDataDict == null)
				{
					return true;
				}

				foreach (DictionaryEntry kvp in importerDataDict)
				{
					var spriteGuid = kvp.Key;
					var importerData = kvp.Value;

					var packedSpriteObj = GetPropertyValue(importerData, "PackedSprite");
					var packedSprite = packedSpriteObj is bool value && value;
					if (!packedSprite)
					{
						continue;
					}

					var source = GetPropertyValue(importerData, "SourceTexture");
					var sourceGuid = GetPropertyValue(source, "guid");
					var zeroGuid = IsZeroGuid(sourceGuid);
					var missingFromDependencyData = dependencyAssetInfo != null && !ContainsGuid(dependencyAssetInfo, sourceGuid);

					if (zeroGuid || missingFromDependencyData)
					{
						LogIssue("Run", spriteGuid, importerData, source, missingFromDependencyData);
					}
					else if (SafeAssetPath(sourceGuid) == "<unknown>")
					{
						LogIssue("Run-UnknownPath", spriteGuid, importerData, source, missingFromDependencyData);
					}
				}
			}
			catch (Exception e)
			{
				LogToFile("StripUnusedSpriteSourcesPatches RunPrefix failed: " + e);
			}

			return true;
		}

		static void CalculateAssetDependencyDataPostfix(object __instance, object __result)
		{
			try
			{
				const string issueKey = "CalcAssetDependencyDataPostfixLogged";
				if (!LoggedIssues.Add(issueKey))
				{
					return;
				}

				LogToFile("SBP CalculateAssetDependencyData: Run completed.");
				var dependencyData = GetField<object>(__instance, "m_DependencyData");
				var dependencyAssetInfo = GetDependencyAssetInfo(dependencyData);
				if (dependencyAssetInfo != null)
				{
					LogZeroGuidAssetInfoEntries(dependencyAssetInfo);
				}

				var spriteData = GetField<object>(__instance, "m_SpriteData");
				var importerData = GetSpriteImporterData(spriteData);
				if (importerData != null)
				{
					LogZeroGuidSpriteSources(importerData);
				}

				var content = GetField<object>(__instance, "m_Content");
				var contentAssets = GetPropertyValue(content, "Assets") as IEnumerable;
				if (contentAssets != null)
				{
					int logged = 0;
					foreach (var assetGuid in contentAssets)
					{
						if (IsZeroGuid(assetGuid))
						{
							LogToFile("SBP CalculateAssetDependencyData: content Assets contains ZERO GUID.");
							break;
						}
						if (logged++ > 1000)
						{
							break;
						}
					}
				}
			}
			catch (Exception e)
			{
				LogToFile("CalculateAssetDependencyDataPostfix failed: " + e);
			}
		}

		static void CalculateSceneDependencyDataPostfix(object __instance, object __result)
		{
			try
			{
				const string issueKey = "CalcSceneDependencyDataPostfixLogged";
				if (!LoggedIssues.Add(issueKey))
				{
					return;
				}

				LogToFile("SBP CalculateSceneDependencyData: Run completed.");
				var dependencyData = GetField<object>(__instance, "m_DependencyData");
				var sceneInfo = GetDependencySceneInfo(dependencyData);
				if (sceneInfo != null)
				{
					LogZeroGuidSceneInfoEntries(sceneInfo);
				}
			}
			catch (Exception e)
			{
				LogToFile("CalculateSceneDependencyDataPostfix failed: " + e);
			}
		}

		static void CalculateCustomDependencyDataPostfix(object __instance, object __result)
		{
			try
			{
				const string issueKey = "CalcCustomDependencyDataPostfixLogged";
				if (!LoggedIssues.Add(issueKey))
				{
					return;
				}

				LogToFile("SBP CalculateCustomDependencyData: Run completed.");
				var dependencyData = GetField<object>(__instance, "m_DependencyData");
				var dependencyAssetInfo = GetDependencyAssetInfo(dependencyData);
				if (dependencyAssetInfo != null)
				{
					LogZeroGuidAssetInfoEntries(dependencyAssetInfo);
				}
			}
			catch (Exception e)
			{
				LogToFile("CalculateCustomDependencyDataPostfix failed: " + e);
			}
		}

		static bool SetOutputInformationPrefix(object __instance, object[] __args)
		{
			try
			{
				LogPatchActiveOnce();
				var dependencyData = GetField<object>(__instance, "m_DependencyData");
				var dependencyAssetInfo = GetDependencyAssetInfo(dependencyData);
				var unusedSources = __args != null && __args.Length > 0 ? __args[0] : null;
				if (unusedSources == null)
				{
					return true;
				}

				var unusedEnumerable = GetEnumerable(unusedSources);
				if (unusedEnumerable == null)
				{
					return true;
				}

				if (dependencyAssetInfo == null)
				{
					LogToFile("SBP StripUnusedSpriteSources: dependency AssetInfo unavailable; logging unused sources only.");
				}

				int logged = 0;
				int zeroGuidCount = 0;
				foreach (var source in unusedEnumerable)
				{
					var sourceGuid = GetPropertyValue(source, "guid");
					if (IsZeroGuid(sourceGuid))
					{
						zeroGuidCount++;
						var localId = GetPropertyValue(source, "localIdentifierInFile");
						var filePath = GetPropertyValue(source, "filePath");
						var issueKey = $"ZeroGuidSourcePrefix:{sourceGuid}:{localId}:{filePath}";
						if (LoggedIssues.Add(issueKey))
						{
							LogToFile($"SBP StripUnusedSpriteSources: unused source has ZERO GUID, localId {localId}, filePath {filePath}");
						}
					}

					if (dependencyAssetInfo != null && !ContainsGuid(dependencyAssetInfo, sourceGuid))
					{
						var localId = GetPropertyValue(source, "localIdentifierInFile");
						var filePath = GetPropertyValue(source, "filePath");
						var issueKey = $"MissingAssetInfo:{sourceGuid}:{localId}:{filePath}";
						if (LoggedIssues.Add(issueKey))
						{
							var sourcePath = SafeAssetPath(sourceGuid);
							LogToFile($"SBP StripUnusedSpriteSources: missing dependency entry for source {sourceGuid} ({sourcePath}), localId {localId}, filePath {filePath}");
						}
					}

					if (logged < 5)
					{
						var localId = GetPropertyValue(source, "localIdentifierInFile");
						var filePath = GetPropertyValue(source, "filePath");
						var assetPath = SafeAssetPath(sourceGuid);
						LogToFile($"SBP StripUnusedSpriteSources: unused source {sourceGuid} ({assetPath}), localId {localId}, filePath {filePath}");
						logged++;
					}
				}

				if (zeroGuidCount > 0)
				{
					LogToFile($"SBP StripUnusedSpriteSources: unused sources contain {zeroGuidCount} zero-GUID entries.");
				}
			}
			catch (Exception e)
			{
				LogToFile("StripUnusedSpriteSourcesPatches SetOutputInformationPrefix failed: " + e);
			}

			return true;
		}

		static void SetOutputInformationFinalizer(object __instance, object unusedSources, Exception __exception)
		{
			if (__exception == null)
			{
				return;
			}

			try
			{
				LogPatchActiveOnce();
				LogToFile("SBP StripUnusedSpriteSources exception captured: " + __exception.Message);

				var unusedEnumerable = GetEnumerable(unusedSources);
				if (unusedEnumerable == null)
				{
					return;
				}

				foreach (var source in unusedEnumerable)
				{
					var sourceGuid = GetPropertyValue(source, "guid");
					if (IsZeroGuid(sourceGuid))
					{
						var localId = GetPropertyValue(source, "localIdentifierInFile");
						var filePath = GetPropertyValue(source, "filePath");
						var issueKey = $"ZeroGuidSource:{sourceGuid}:{localId}:{filePath}";
						if (LoggedIssues.Add(issueKey))
						{
							LogToFile($"SBP StripUnusedSpriteSources: unused source has ZERO GUID, localId {localId}, filePath {filePath}");
						}
					}
				}
			}
			catch (Exception e)
			{
				LogToFile("StripUnusedSpriteSourcesPatches finalizer failed: " + e);
			}
		}

		static void LogIssue(string context, object spriteGuid, object importerData, object source, bool missingFromDependencyData)
		{
			var sourceGuid = GetPropertyValue(source, "guid");
			var localId = GetPropertyValue(source, "localIdentifierInFile");
			var filePath = GetPropertyValue(source, "filePath");
			var issueKey = $"{context}:{spriteGuid}:{sourceGuid}:{localId}:{filePath}:{missingFromDependencyData}";
			if (!LoggedIssues.Add(issueKey))
			{
				return;
			}

			var spritePath = SafeAssetPath(spriteGuid);
			var sourcePath = SafeAssetPath(sourceGuid);
			var packedSprite = GetPropertyValue(importerData, "PackedSprite");
			LogToFile(
				$"SBP StripUnusedSpriteSources [{context}] sprite {spriteGuid} ({spritePath}) " +
				$"source guid {sourceGuid} ({sourcePath}), localId {localId}, filePath {filePath}, " +
				$"packed={packedSprite}, missingAssetInfo={missingFromDependencyData}");
		}

		static string SafeAssetPath(object guid)
		{
			if (IsZeroGuid(guid))
			{
				return "<zero-guid>";
			}

			var guidString = guid?.ToString() ?? string.Empty;
			if (string.IsNullOrWhiteSpace(guidString))
			{
				return "<unknown>";
			}

			var path = AssetDatabase.GUIDToAssetPath(guidString);
			return string.IsNullOrEmpty(path) ? "<unknown>" : path;
		}

		static T GetField<T>(object instance, string fieldName) where T : class
		{
			if (instance == null)
			{
				return null;
			}

			var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
			if (field == null)
			{
				return null;
			}

			return field.GetValue(instance) as T;
		}

		static IDictionary GetSpriteImporterData(object spriteData)
		{
			return GetPropertyValue(spriteData, "ImporterData") as IDictionary
				?? GetInterfacePropertyValue(spriteData, "UnityEditor.Build.Pipeline.Interfaces.IBuildSpriteData", "ImporterData") as IDictionary;
		}

		static IDictionary GetDependencyAssetInfo(object dependencyData)
		{
			return GetPropertyValue(dependencyData, "AssetInfo") as IDictionary
				?? GetInterfacePropertyValue(dependencyData, "UnityEditor.Build.Pipeline.Interfaces.IDependencyData", "AssetInfo") as IDictionary;
		}

		static IDictionary GetDependencySceneInfo(object dependencyData)
		{
			return GetPropertyValue(dependencyData, "SceneInfo") as IDictionary
				?? GetInterfacePropertyValue(dependencyData, "UnityEditor.Build.Pipeline.Interfaces.IDependencyData", "SceneInfo") as IDictionary;
		}

		static object GetInterfacePropertyValue(object instance, string interfaceFullName, string propertyName)
		{
			if (instance == null)
			{
				return null;
			}

			var type = instance.GetType();
			var iface = type.GetInterface(interfaceFullName);
			if (iface == null)
			{
				return null;
			}

			var prop = iface.GetProperty(propertyName);
			if (prop == null)
			{
				return null;
			}

			var getter = prop.GetGetMethod();
			if (getter == null)
			{
				return null;
			}

			return getter.Invoke(instance, null);
		}

		static object GetPropertyValue(object instance, string propertyName)
		{
			if (instance == null)
			{
				return null;
			}

			var type = instance.GetType();
			var property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null)
			{
				return property.GetValue(instance, null);
			}

			var field = type.GetField(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null)
			{
				return field.GetValue(instance);
			}

			return null;
		}

		static IEnumerable GetEnumerable(object value)
		{
			return value as IEnumerable;
		}

		static bool ContainsGuid(IDictionary dictionary, object guid)
		{
			if (dictionary == null)
			{
				return false;
			}

			if (guid == null)
			{
				return false;
			}

			if (dictionary.Contains(guid))
			{
				return true;
			}

			var guidString = guid.ToString();
			foreach (DictionaryEntry entry in dictionary)
			{
				if (entry.Key != null && entry.Key.ToString() == guidString)
				{
					return true;
				}
			}

			return false;
		}

		static bool IsZeroGuid(object guid)
		{
			if (guid == null)
			{
				return true;
			}

			var guidString = guid.ToString();
			if (guidString == ZeroGuidString)
			{
				return true;
			}

			var normalized = NormalizeGuidString(guidString);
			if (normalized.Length > 0 && normalized.All(c => c == '0'))
			{
				return true;
			}

			var guidType = guid.GetType();
			try
			{
				var emptyField = guidType.GetField("Empty", BindingFlags.Public | BindingFlags.Static);
				if (emptyField != null && Equals(guid, emptyField.GetValue(null)))
				{
					return true;
				}

				var emptyProp = guidType.GetProperty("Empty", BindingFlags.Public | BindingFlags.Static);
				if (emptyProp != null && Equals(guid, emptyProp.GetValue(null, null)))
				{
					return true;
				}

				var defaultGuid = Activator.CreateInstance(guidType);
				if (Equals(guid, defaultGuid))
				{
					return true;
				}
			}
			catch
			{
				// Best-effort detection; ignore reflection issues.
			}

			return false;
		}

		static string NormalizeGuidString(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return string.Empty;
			}

			char[] buffer = new char[value.Length];
			int length = 0;
			for (int i = 0; i < value.Length; i++)
			{
				var c = value[i];
				if (char.IsLetterOrDigit(c))
				{
					buffer[length++] = char.ToLowerInvariant(c);
				}
			}

			return new string(buffer, 0, length);
		}

		static void LogZeroGuidInDependencyDataOnce(IDictionary dependencyAssetInfo)
		{
			const string issueKey = "DependencyAssetInfoZeroGuid";
			if (!LoggedIssues.Add(issueKey))
			{
				return;
			}

			foreach (DictionaryEntry entry in dependencyAssetInfo)
			{
				if (entry.Key != null && entry.Key.ToString() == ZeroGuidString)
				{
					LogToFile("SBP StripUnusedSpriteSources: dependency AssetInfo contains ZERO GUID key.");
					return;
				}
			}
		}

		static void LogZeroGuidAssetInfoEntries(IDictionary dependencyAssetInfo)
		{
			int logged = 0;
			foreach (DictionaryEntry entry in dependencyAssetInfo)
			{
				if (!IsZeroGuid(entry.Key))
				{
					continue;
				}

				var assetInfo = entry.Value;
				var address = GetPropertyValue(assetInfo, "address") ?? GetPropertyValue(assetInfo, "Address");
				LogToFile($"SBP CalculateAssetDependencyData: AssetInfo has ZERO GUID key. Address={address ?? "<null>"}");

				var includedObjects = GetPropertyValue(assetInfo, "includedObjects") as IEnumerable;
				var referencedObjects = GetPropertyValue(assetInfo, "referencedObjects") as IEnumerable;
				LogObjectIdentifiers("includedObjects", includedObjects);
				LogObjectIdentifiers("referencedObjects", referencedObjects);

				if (++logged >= 3)
				{
					break;
				}
			}
		}

		static void LogZeroGuidSpriteSources(IDictionary importerData)
		{
			int logged = 0;
			foreach (DictionaryEntry entry in importerData)
			{
				var spriteGuid = entry.Key;
				var data = entry.Value;
				var source = GetPropertyValue(data, "SourceTexture");
				var sourceGuid = GetPropertyValue(source, "guid");
				if (!IsZeroGuid(sourceGuid))
				{
					continue;
				}

				var spritePath = SafeAssetPath(spriteGuid);
				LogToFile($"SBP CalculateAssetDependencyData: sprite {spriteGuid} ({spritePath}) has ZERO source texture GUID.");
				LogObjectIdentifier("sourceTexture", source);

				if (++logged >= 5)
				{
					break;
				}
			}
		}

		static void LogObjectIdentifiers(string label, IEnumerable identifiers)
		{
			if (identifiers == null)
			{
				return;
			}

			int logged = 0;
			foreach (var obj in identifiers)
			{
				if (logged >= 5)
				{
					break;
				}

				LogObjectIdentifier(label, obj);
				logged++;
			}
		}

		static void LogObjectIdentifier(string label, object obj)
		{
			if (obj == null)
			{
				return;
			}

			var guid = GetPropertyValue(obj, "guid");
			var localId = GetPropertyValue(obj, "localIdentifierInFile");
			var filePath = GetPropertyValue(obj, "filePath");
			var guidString = guid?.ToString() ?? "<null>";
			LogToFile($"SBP CalculateAssetDependencyData: {label} guid={guidString}, localId={localId}, filePath={filePath}");
		}

		static void LogPatchActiveOnce()
		{
			if (loggedPatchActive)
			{
				return;
			}

			loggedPatchActive = true;
			LogToFile("StripUnusedSpriteSources patches active.");
		}

		static void LogZeroGuidSceneInfoEntries(IDictionary sceneInfo)
		{
			int logged = 0;
			foreach (DictionaryEntry entry in sceneInfo)
			{
				if (!IsZeroGuid(entry.Key))
				{
					continue;
				}

				var sceneDep = entry.Value;
				var scenePath = GetPropertyValue(sceneDep, "scene") ?? GetPropertyValue(sceneDep, "scenePath");
				LogToFile($"SBP CalculateSceneDependencyData: SceneInfo has ZERO GUID key. Scene={scenePath ?? "<null>"}");

				var referencedObjects = GetPropertyValue(sceneDep, "referencedObjects") as IEnumerable;
				LogObjectIdentifiers("scene.referencedObjects", referencedObjects);

				if (++logged >= 3)
				{
					break;
				}
			}
		}

		static void LogToFile(string message)
		{
			try
			{
				var line = $"[{DateTime.UtcNow:O}] {message}{Environment.NewLine}";
				File.AppendAllText(LogFilePath, line);
			}
			catch
			{
				// Ignore logging failures.
			}
		}

		static Type FindType(string fullName)
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				var type = assembly.GetType(fullName, false);
				if (type != null)
				{
					return type;
				}
			}
			return null;
		}
	}
}
