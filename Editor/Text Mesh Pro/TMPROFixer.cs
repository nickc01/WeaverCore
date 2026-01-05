#define SKIP_TMPRO_PROCESSING
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WeaverCore;
using System.IO;
using System.Threading.Tasks;
using System.Collections;
using WeaverCore.Utilities;

namespace TMProOld.EditorUtilities
{
	/// <summary>
	/// Redirects Unity's built-in TMP editor menus to the WeaverCore TMP (TMProOld) variants.
	/// This prevents Unity's TMP importer from pulling in incompatible scripts.
	/// </summary>
	internal static class TMPROFixer
	{
		private static readonly HashSet<string> LoggedAddComponentMenus = new HashSet<string>();
		private const int MaxAddComponentMenuLogs = 25;
		private static bool patchesApplied;
		private static bool isReplacingComponent;
		private static bool tmproProcessingStarted;
		private static Task<TmproProcessingResult> tmproProcessingTask;
		private static readonly HashSet<int> PendingReplacementIds = new HashSet<int>();
		private static MethodInfo oldCreateText3D;
		private static MethodInfo oldCreateTextUI;
		private static MethodInfo oldCreateInputField;
		private static MethodInfo oldCreateDropdown;
		private static MethodInfo oldPlaceUIElementRoot;
		private static MethodInfo oldGetStandardResources;
		private static bool methodsInitialized;

		private static Assembly newTMPROAssembly;

		[InitializeOnLoadMethod]
		private static void InitializeOnLoad()
		{
#if UNITY_EDITOR
			if (patchesApplied)
			{
				return;
			}

			patchesApplied = true;

			try
			{
				HarmonyPatcher patcher = HarmonyPatcher.Create("com.tmpro.fixer");
				CacheOldMenuMethods();

				var unityTmpMenuType = FindType("TMPro.EditorUtilities.TMPro_CreateObjectMenu");
				if (unityTmpMenuType == null)
				{
					WeaverLog.LogWarning("TMPROFixer: Unable to find Unity TMP menu type.");
					return;
				}

                newTMPROAssembly = FindType("TMPro.TMP_SpriteAsset").Assembly;

				PatchMenuMethod(patcher, unityTmpMenuType, "CreateTextMeshProObjectPerform", nameof(CreateTextMeshProObjectPerformPrefix));
				PatchMenuMethod(patcher, unityTmpMenuType, "CreateTextMeshProGuiObjectPerform", nameof(CreateTextMeshProGuiObjectPerformPrefix));
				PatchMenuMethod(patcher, unityTmpMenuType, "AddTextMeshProInputField", nameof(AddTextMeshProInputFieldPrefix));
				PatchMenuMethod(patcher, unityTmpMenuType, "AddDropdown", nameof(AddDropdownPrefix));
				PatchMenuMethod(patcher, unityTmpMenuType, "AddButton", nameof(AddButtonPrefix));

				var tmpSettingsType = FindType("TMPro.TMP_Settings");
				var delayImporter = tmpSettingsType?.GetMethod("DelayShowPackageImporterWindow", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (delayImporter != null)
				{
					var prefix = typeof(TMPROFixer).GetMethod(nameof(DelayShowPackageImporterWindowPrefix), BindingFlags.Static | BindingFlags.NonPublic);
					patcher.Patch(delayImporter, prefix, null);
				}

				var objectFactoryType = typeof(ObjectFactory);
				var addComponentMethod = objectFactoryType.GetMethod("AddComponent", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(GameObject), typeof(Type) }, null);
				if (addComponentMethod != null)
				{
					var prefix = typeof(TMPROFixer).GetMethod(nameof(ObjectFactoryAddComponentPrefix), BindingFlags.Static | BindingFlags.NonPublic);
					patcher.Patch(addComponentMethod, prefix, null);
				}

				PatchOnValidateMethods(patcher);
				PatchAddComponentMenuConstructors(patcher);
				StartTmproProcessing();
			}
			catch (Exception e)
			{
				WeaverLog.Log("TMPROFixer failed to apply patches: " + e);
			}
#endif
		}

		private static void PatchMenuMethod(HarmonyPatcher patcher, Type menuType, string methodName, string prefixName)
		{
			var original = menuType.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (original == null)
			{
				WeaverLog.LogWarning($"TMPROFixer: Unable to find Unity TMP menu method {menuType.FullName}.{methodName}");
				return;
			}

			var prefix = typeof(TMPROFixer).GetMethod(prefixName, BindingFlags.Static | BindingFlags.NonPublic);
			if (prefix == null)
			{
				WeaverLog.LogWarning($"TMPROFixer: Missing prefix {prefixName}");
				return;
			}

			patcher.Patch(original, prefix, null);
		}

		private static void CacheOldMenuMethods()
		{
			if (methodsInitialized)
			{
				return;
			}

			var oldMenuType = typeof(TMProOld.EditorUtilities.TMPro_CreateObjectMenu);
			oldCreateText3D = oldMenuType.GetMethod("CreateTextMeshProObjectPerform", BindingFlags.Static | BindingFlags.NonPublic);
			oldCreateTextUI = oldMenuType.GetMethod("CreateTextMeshProGuiObjectPerform", BindingFlags.Static | BindingFlags.NonPublic);
			oldCreateInputField = oldMenuType.GetMethod("AddTextMeshProInputField", BindingFlags.Static | BindingFlags.NonPublic);
			oldCreateDropdown = oldMenuType.GetMethod("AddDropdown", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			oldPlaceUIElementRoot = oldMenuType.GetMethod("PlaceUIElementRoot", BindingFlags.Static | BindingFlags.NonPublic);
			oldGetStandardResources = oldMenuType.GetMethod("GetStandardResources", BindingFlags.Static | BindingFlags.NonPublic);

			methodsInitialized = true;
		}

		private static bool CreateTextMeshProObjectPerformPrefix(MenuCommand command)
		{
			return !InvokeOldMenuMethod(oldCreateText3D, command);
		}

		private static bool CreateTextMeshProGuiObjectPerformPrefix(MenuCommand menuCommand)
		{
			return !InvokeOldMenuMethod(oldCreateTextUI, menuCommand);
		}

		private static bool AddTextMeshProInputFieldPrefix(MenuCommand menuCommand)
		{
			return !InvokeOldMenuMethod(oldCreateInputField, menuCommand);
		}

		private static bool AddDropdownPrefix(MenuCommand menuCommand)
		{
			return !InvokeOldMenuMethod(oldCreateDropdown, menuCommand);
		}

		private static bool AddButtonPrefix(MenuCommand menuCommand)
		{
			try
			{
				var button = CreateWeaverTmpButton();
				if (button == null)
				{
					return true;
				}

				if (oldPlaceUIElementRoot != null)
				{
					oldPlaceUIElementRoot.Invoke(null, new object[] { button, menuCommand });
				}
				else
				{
					PlaceUIElementFallback(button, menuCommand);
				}

				EnsureEventSystemExists();
				return false;
			}
			catch (Exception e)
			{
				WeaverLog.LogException(e);
				return true;
			}
		}

		private static bool DelayShowPackageImporterWindowPrefix()
		{
			return false;
		}

		private static bool ObjectFactoryAddComponentPrefix(GameObject gameObject, Type type, ref Component __result)
		{
			if (isReplacingComponent)
			{
				return true;
			}

			if (IsInPlayMode())
			{
				return true;
			}

			var replacement = GetWeaverCoreType(type);
			if (replacement == null)
			{
				return true;
			}

			try
			{
				isReplacingComponent = true;
				__result = Undo.AddComponent(gameObject, replacement);
				return false;
			}
			catch (Exception e)
			{
				WeaverLog.LogException(e);
				return true;
			}
			finally
			{
				isReplacingComponent = false;
			}
		}

		private static Type GetOldType(Type componentType)
		{
			if (componentType == null || componentType.Namespace != "TMPro")
			{
				return null;
			}

			if (componentType.FullName == "TMPro.TextMeshProUGUI")
			{
				return typeof(WeaverCore.Assets.TMProOld.TextMeshProUGUI);
			}

			if (componentType.FullName == "TMPro.TextMeshPro")
			{
				return typeof(WeaverCore.Assets.TMProOld.TextMeshPro);
			}

			return FindType($"TMProOld.{componentType.Name}");
		}


		private static Type GetWeaverCoreType(Type componentType)
		{
			var oldType = GetOldType(componentType);
			if (oldType != null)
			{
				return oldType;
			}

			if (componentType == typeof(TMProOld.TextMeshProUGUI))
			{
				return typeof(WeaverCore.Assets.TMProOld.TextMeshProUGUI);
			}

			if (componentType == typeof(TMProOld.TextMeshPro))
			{
				return typeof(WeaverCore.Assets.TMProOld.TextMeshPro);
			}

			return componentType;
		}

		private static void PatchOnValidateMethods(HarmonyPatcher patcher)
		{
			if (newTMPROAssembly == null)
			{
				return;
			}

			Type[] types;
			try
			{
				types = newTMPROAssembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
				types = e.Types;
			}

			if (types == null)
			{
				return;
			}

			var prefix = typeof(TMPROFixer).GetMethod(nameof(OnValidateReplacePrefix), BindingFlags.Static | BindingFlags.NonPublic);
			if (prefix == null)
			{
				return;
			}

			foreach (var type in types)
			{
                //WeaverLog.Log("FOUND TYPE = " + type);
				if (type == null || type.Namespace != "TMPro")
				{
					continue;
				}

				if (!typeof(Component).IsAssignableFrom(type))
				{
					continue;
				}

				var onValidate = type.GetMethod("OnValidate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (onValidate == null || onValidate.IsAbstract || onValidate.DeclaringType != type)
				{
					continue;
				}

				patcher.Patch(onValidate, prefix, null);
			}
		}

		private static bool OnValidateReplacePrefix(Component __instance)
		{
			return !TryReplaceNewTmpComponent(__instance);
		}

		private static bool TryReplaceNewTmpComponent(Component component)
		{
			if (component == null || isReplacingComponent || IsInPlayMode())
			{
				return false;
			}

			var replacementType = GetOldType(component.GetType());
			if (replacementType == null)
			{
				return false;
			}

			var id = component.GetInstanceID();
			if (!PendingReplacementIds.Add(id))
			{
				return true;
			}

			UnboundCoroutine.Start(ReplaceNewTmpComponentRoutine(component, replacementType, id));
			return true;
		}

		private static IEnumerator ReplaceNewTmpComponentRoutine(Component component, Type replacementType, int instanceId)
		{
			yield return null;

			var continueAfterDestroy = false;
			GameObject gameObject = null;

			try
			{
				if (component == null || replacementType == null || IsInPlayMode())
				{
					yield break;
				}

				if (GetOldType(component.GetType()) == null)
				{
					yield break;
				}

				gameObject = component.gameObject;
				if (gameObject == null)
				{
					yield break;
				}

				isReplacingComponent = true;
				UnityEngine.Object.DestroyImmediate(component, true);
				continueAfterDestroy = true;
			}
			catch (Exception e)
			{
				WeaverLog.LogException(e);
			}
			finally
			{
				isReplacingComponent = false;
				if (!continueAfterDestroy)
				{
					PendingReplacementIds.Remove(instanceId);
				}
			}

			if (!continueAfterDestroy)
			{
				yield break;
			}

			yield return null;

			try
			{
				if (gameObject == null || IsInPlayMode())
				{
					yield break;
				}

				isReplacingComponent = true;
				if (gameObject.GetComponent(replacementType) == null)
				{
					Undo.AddComponent(gameObject, replacementType);
				}
			}
			catch (Exception e)
			{
				WeaverLog.LogException(e);
			}
			finally
			{
				isReplacingComponent = false;
				PendingReplacementIds.Remove(instanceId);
			}
		}

		private static void PatchAddComponentMenuConstructors(HarmonyPatcher patcher)
		{
			var attributeType = typeof(AddComponentMenu);
			var stringCtor = attributeType.GetConstructor(new[] { typeof(string) });
			if (stringCtor != null)
			{
				var prefix = typeof(TMPROFixer).GetMethod(nameof(AddComponentMenuCtorPrefix), BindingFlags.Static | BindingFlags.NonPublic);
				patcher.Patch(stringCtor, prefix, null);
			}

			var stringIntCtor = attributeType.GetConstructor(new[] { typeof(string), typeof(int) });
			if (stringIntCtor != null)
			{
				var prefix = typeof(TMPROFixer).GetMethod(nameof(AddComponentMenuCtorWithOrderPrefix), BindingFlags.Static | BindingFlags.NonPublic);
				patcher.Patch(stringIntCtor, prefix, null);
			}
		}

		private static void AddComponentMenuCtorPrefix(string menuName)
		{
			LogAddComponentMenu(menuName, null);
		}

		private static void AddComponentMenuCtorWithOrderPrefix(string menuName, int order)
		{
			LogAddComponentMenu(menuName, order);
		}

		private static void LogAddComponentMenu(string menuName, int? order)
		{
			if (LoggedAddComponentMenus.Count >= MaxAddComponentMenuLogs)
			{
				return;
			}

			var key = order.HasValue ? $"{menuName}:{order.Value}" : menuName;
			if (!LoggedAddComponentMenus.Add(key))
			{
				return;
			}

			var suffix = order.HasValue ? $" (order {order.Value})" : string.Empty;
			WeaverLog.Log($"AddComponentMenuAttribute ctor: {menuName}{suffix}");
		}

		private static bool InvokeOldMenuMethod(MethodInfo method, MenuCommand menuCommand)
		{
			if (method == null)
			{
				WeaverLog.LogWarning("TMPROFixer: Old TMP menu method missing; using Unity TMP behavior.");
				return false;
			}

			try
			{
				method.Invoke(null, new object[] { menuCommand });
				return true;
			}
			catch (Exception e)
			{
				WeaverLog.LogException(e);
				return false;
			}
		}

		private static GameObject CreateWeaverTmpButton()
		{
			var root = new GameObject("Button");
			var rect = root.AddComponent<RectTransform>();
			rect.sizeDelta = new Vector2(160f, 30f);

			var image = root.AddComponent<Image>();
			image.sprite = GetBuiltinSprite("UI/Skin/UISprite.psd");
			image.type = Image.Type.Sliced;
			image.color = Color.white;

			var button = root.AddComponent<Button>();
			var colors = button.colors;
			colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
			colors.pressedColor = new Color(0.698f, 0.698f, 0.698f);
			colors.disabledColor = new Color(0.521f, 0.521f, 0.521f);
			button.colors = colors;

			var textGo = new GameObject("Text (TMP)");
			var textRect = textGo.AddComponent<RectTransform>();
			textRect.anchorMin = Vector2.zero;
			textRect.anchorMax = Vector2.one;
			textRect.sizeDelta = Vector2.zero;
			textRect.SetParent(root.transform, false);

			var text = textGo.AddComponent<WeaverCore.Assets.TMProOld.TextMeshProUGUI>();
			text.text = "Button";
			text.fontSize = 24f;
			text.alignment = TMProOld.TextAlignmentOptions.Center;
			text.color = new Color(10f / 51f, 10f / 51f, 10f / 51f, 1f);

			return root;
		}

		private static Sprite GetBuiltinSprite(string path)
		{
			if (oldGetStandardResources != null)
			{
				try
				{
					var resources = oldGetStandardResources.Invoke(null, null);
					var field = resources?.GetType().GetField("standard");
					if (field != null)
					{
						var sprite = field.GetValue(resources) as Sprite;
						if (sprite != null)
						{
							return sprite;
						}
					}
				}
				catch (Exception e)
				{
					WeaverLog.LogException(e);
				}
			}

			return AssetDatabase.GetBuiltinExtraResource<Sprite>(path);
		}

		private static void EnsureEventSystemExists()
		{
			if (UnityEngine.Object.FindObjectOfType<EventSystem>() != null)
			{
				return;
			}

			var eventSystem = new GameObject("EventSystem", typeof(EventSystem));
			eventSystem.AddComponent<StandaloneInputModule>();
			Undo.RegisterCreatedObjectUndo(eventSystem, "Create " + eventSystem.name);
		}

		private static void PlaceUIElementFallback(GameObject element, MenuCommand menuCommand)
		{
			GameObject parent = menuCommand.context as GameObject;
			if (parent == null || parent.GetComponentInParent<Canvas>() == null)
			{
				parent = TMProOld.EditorUtilities.TMPro_CreateObjectMenu.GetOrCreateCanvasGameObject();
			}

			GameObjectUtility.SetParentAndAlign(element, parent);
			Selection.activeGameObject = element;
		}

		private static bool IsInPlayMode()
		{
			return EditorApplication.isPlayingOrWillChangePlaymode || Application.isPlaying;
		}

		private static void StartTmproProcessing()
		{
			if (tmproProcessingStarted || IsInPlayMode())
			{
				return;
			}

			tmproProcessingStarted = true;
			EditorApplication.delayCall += QueueTmproProcessing;
		}

		private static void QueueTmproProcessing()
		{
			if (IsInPlayMode())
			{
				tmproProcessingStarted = false;
				return;
			}

			try
			{
				tmproProcessingTask = Task.Run(ProcessTmproScripts);
				EditorApplication.update += PollTmproProcessing;
			}
			catch (Exception e)
			{
				WeaverLog.LogException(e);
				tmproProcessingStarted = false;
			}
		}

		private static void PollTmproProcessing()
		{
			if (tmproProcessingTask == null || !tmproProcessingTask.IsCompleted)
			{
				return;
			}

			EditorApplication.update -= PollTmproProcessing;
			var task = tmproProcessingTask;
			tmproProcessingTask = null;
			tmproProcessingStarted = false;

			if (task.IsFaulted)
			{
				WeaverLog.LogException(task.Exception);
				return;
			}

			var result = task.Result;
			if (result.Errors.Count > 0)
			{
				WeaverLog.LogWarning($"TMPROFixer: encountered {result.Errors.Count} errors while processing scripts. First error: {result.Errors[0]}");
			}

			if (result.ChangedCount > 0)
			{
				EditorApplication.delayCall += () => AssetDatabase.Refresh();
			}
		}

		private static TmproProcessingResult ProcessTmproScripts()
		{
			var result = new TmproProcessingResult();
			var rootPath = Application.dataPath;
			if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
			{
				return result;
			}

			foreach (var path in Directory.EnumerateFiles(rootPath, "*.cs", SearchOption.AllDirectories))
			{
				string text;
				try
				{
					text = File.ReadAllText(path);
				}
				catch (Exception e)
				{
					result.Errors.Add(e);
					continue;
				}

				if (text.IndexOf("#define SKIP_TMPRO_PROCESSING", StringComparison.Ordinal) >= 0)
				{
					continue;
				}

				bool hasUsing = text.IndexOf("using TMPro;", StringComparison.Ordinal) >= 0;
				bool hasNamespace = text.IndexOf("TMPro.", StringComparison.Ordinal) >= 0;
				if (!hasUsing && !hasNamespace)
				{
					continue;
				}

				var updated = text;
				if (hasUsing)
				{
					updated = updated.Replace("using TMPro;", "using TMProOld;");
				}

				if (hasNamespace)
				{
					updated = updated.Replace("TMPro.", "TMProOld.");
				}

				if (updated == text)
				{
					continue;
				}

				try
				{
					File.WriteAllText(path, updated);
					result.ChangedCount++;
				}
				catch (Exception e)
				{
					result.Errors.Add(e);
				}
			}

			return result;
		}

		private sealed class TmproProcessingResult
		{
			public int ChangedCount;
			public List<Exception> Errors = new List<Exception>();
		}

		private static Type FindType(string fullName)
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				var type = assembly.GetType(fullName);
				if (type != null)
				{
					return type;
				}
			}

			return null;
		}
	}
}
