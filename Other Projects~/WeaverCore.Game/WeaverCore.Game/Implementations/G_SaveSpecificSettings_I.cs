using Modding;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Implementations;
using WeaverCore.Settings;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Implementations
{
	public class G_SaveSpecificSettings_I : SaveSpecificSettings_I
	{
		static int _saveSlot = -1;
		public override int CurrentSaveSlot => _saveSlot;

		static Dictionary<Type, string> baseInformation = new Dictionary<Type, string>();

		public static event Action<int> NewSaveFileCreated;

		static MethodInfo getSaveSlotPathMethod;
		static object primarySaveSlotUsage;

		[OnHarmonyPatch]
		static void OnHarmonyPatch(HarmonyPatcher patcher)
		{
			var desktopPlatformType = typeof(GameManager).Assembly.GetType("DesktopPlatform");

			/*{
				var writeSaveSlot = desktopPlatformType.GetMethod("WriteSaveSlot", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				var prefix = typeof(G_SaveSpecificSettings_I).GetMethod(nameof(WriteSaveSlot_Prefix), BindingFlags.NonPublic | BindingFlags.Static);
				var postfix = typeof(G_SaveSpecificSettings_I).GetMethod(nameof(WriteSaveSlot_Postfix), BindingFlags.NonPublic | BindingFlags.Static);

				patcher.Patch(writeSaveSlot, prefix, postfix);
			}*/

			/*{
				var orig = typeof(GameManager).GetMethod("StartNewGame", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				var prefix = typeof(G_SaveSpecificSettings_I).GetMethod(nameof(WriteSaveSlot_Prefix), BindingFlags.NonPublic | BindingFlags.Static);
				var postfix = typeof(G_SaveSpecificSettings_I).GetMethod(nameof(WriteSaveSlot_Postfix), BindingFlags.NonPublic | BindingFlags.Static);
			}*/
		}

		[OnFeatureLoad]
		static void SaveRegistered(SaveSpecificSettings settings)
		{
			WeaverLog.Log("SAVE REG = " + settings);
			if (!baseInformation.ContainsKey(settings.GetType()))
			{
				baseInformation.Add(settings.GetType(), JsonUtility.ToJson(settings));
			}
			/*if (GetSaveSettings(settings.GetType()) == null)
			{
				RegisterSaveSpecificSettings(settings);
			}*/
		}

		[OnFeatureUnload]
		static void SaveUnRegistered(SaveSpecificSettings settings)
		{
			WeaverLog.Log("SAVE UNREG = " + settings);
			baseInformation.Remove(settings.GetType());
		}

		/*[OnHarmonyPatch]
		static void OnHarmonyPatch(HarmonyPatcher patcher)
		{
			{
				var orig = typeof(GameM)
			}
		}*/

		Dictionary<string,JToken> GetModData()
		{
			var field = typeof(GameManager).GetField("moddedData",BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			WeaverLog.Log("MODDED DATA FIELD = " + field);
			var saveDataType = field.FieldType;
			WeaverLog.Log("MODDED DATA FIELD TYPE = " + saveDataType);
			WeaverLog.Log("GAMEMANAGER = " + GameManager.instance);
			var value = field.GetValue(GameManager.instance);

			if (value == null)
			{
				field.SetValue(GameManager.instance, Activator.CreateInstance(saveDataType));
				value = field.GetValue(GameManager.instance);
			}

			WeaverLog.Log("MODDED DATA VALUE = " + value);
			WeaverLog.Log("MODDED DATA VALUE TYPE = " + value?.GetType().FullName ?? "null");

			var modDataF = saveDataType.GetField("modData", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			WeaverLog.Log("ModData Field = " + modDataF);
			return (Dictionary<string, JToken>)modDataF.GetValue(value);
		}

		static Dictionary<Type, string> defaultState = new Dictionary<Type, string>();

		public override void LoadSettings(SaveSpecificSettings settings)
		{
			if (settings.Enabled)
			{
                var modData = GetModData();
                var settingsType = settings.GetType();
                var key = settingsType.FullName;
                if (!defaultState.ContainsKey(settingsType))
                {
                    defaultState.Add(settingsType, JsonUtility.ToJson(settings));
                }
                if (modData.ContainsKey(key))
                {
                    var token = modData[key];
                    JsonUtility.FromJsonOverwrite(token.ToString(), settings);
                }
                else
                {
                    JsonUtility.FromJsonOverwrite(defaultState[settingsType], settings);
                }
            }
		}

		public override void SaveSettings(SaveSpecificSettings settings)
		{
			WeaverLog.Log("SAVING SETTINGS!!! = " + settings);
			if (settings.Enabled)
			{
				var modData = GetModData();
				var result = JsonUtility.ToJson(settings, true);
				WeaverLog.Log("RESULT = " + result);

				var token = JToken.Parse(JsonUtility.ToJson(settings));
				var key = settings.GetType().FullName;
				if (modData.ContainsKey(key))
				{
					modData[key] = token;
				}
				else
				{
					modData.Add(key, token);
				}
			}
		}

		[OnInit]
		static void Init()
		{
            ModHooks.BeforeSavegameSaveHook += ModHooks_BeforeSavegameSaveHook;
			ModHooks.AfterSavegameLoadHook += ModHooks_AfterSavegameLoadHook;
			On.GameManager.LoadGame += GameManager_LoadGame;
			On.GameManager.StartNewGame += GameManager_StartNewGame;
		}

		private static void GameManager_StartNewGame(On.GameManager.orig_StartNewGame orig, GameManager self, bool permadeathMode, bool bossRushMode)
		{
			ResetAllSaveSpecificSettingsToBaseState();
			orig(self, permadeathMode, bossRushMode);
		}

		private static void GameManager_LoadGame(On.GameManager.orig_LoadGame orig, GameManager self, int saveSlot, Action<bool> callback)
		{
			if (Platform.IsSaveSlotIndexValid(saveSlot))
			{
				_saveSlot = saveSlot;
			}

			orig(self,saveSlot,callback);
		}

		private static void ModHooks_AfterSavegameLoadHook(SaveGameData obj)
		{
            //WeaverLog.Log("LOADING WEAVERCORE SETTINGS");
            SaveSpecificSettings.LoadSaveSlot(_saveSlot);
		}

		private static void ModHooks_BeforeSavegameSaveHook(SaveGameData obj)
		{
			//WeaverLog.Log("SAVING WEAVERCORE SETTINGS");
			SaveSpecificSettings.SaveAllSettings();
		}

		/*static void WriteSaveSlot_Prefix(object __instance, int slotIndex, ref string __state)
		{
			__state = null;
			try
			{
				var path = GetPrimarySavePath(__instance, slotIndex);
				WeaverLog.Log("PATH + " + path);
				if (!string.IsNullOrEmpty(path) && !File.Exists(path))
				{
					__state = path;
				}
			}
			catch (Exception e)
			{
				WeaverLog.LogError($"[SaveSpecificSettings] Failed to inspect save path: {e}");
			}
		}

		static void WriteSaveSlot_Postfix(string __state, int slotIndex)
		{
			try
			{
				if (!string.IsNullOrEmpty(__state) && File.Exists(__state))
				{
					WeaverLog.Log("FILE CREATED = " + __state);
					ResetAllSaveSpecificSettingsToBaseState();
					NewSaveFileCreated?.Invoke(slotIndex);
				}
			}
			catch (Exception e)
			{
				WeaverLog.LogError($"[SaveSpecificSettings] Failed to report new save creation: {e}");
			}
		}*/

		static void ResetAllSaveSpecificSettingsToBaseState()
		{
			foreach (var kvp in baseInformation)
			{
				try
				{
					var settings = SaveSpecificSettings.GetSaveSettings(kvp.Key);
					if (settings != null)
					{
						WeaverLog.Log($"RESETTING {settings.GetType()} to base");
						JsonUtility.FromJsonOverwrite(kvp.Value, settings);
					}
				}
				catch (Exception e)
				{
					WeaverLog.LogError($"[SaveSpecificSettings] Failed to reset settings for {kvp.Key?.Name}: {e}");
				}
			}
		}

		static string GetPrimarySavePath(object platformInstance, int slotIndex)
		{
			if (platformInstance == null)
			{
				return null;
			}

			if (getSaveSlotPathMethod == null)
			{
				getSaveSlotPathMethod = platformInstance.GetType().GetMethod("GetSaveSlotPath", BindingFlags.Instance | BindingFlags.NonPublic);
			}

			if (getSaveSlotPathMethod == null)
			{
				return null;
			}

			if (primarySaveSlotUsage == null)
			{
				var usageType = typeof(Platform).GetNestedType("SaveSlotFileNameUsage", BindingFlags.NonPublic);
				primarySaveSlotUsage = Enum.ToObject(usageType, 0);
			}

			return (string)getSaveSlotPathMethod.Invoke(platformInstance, new object[] { slotIndex, primarySaveSlotUsage });
		}
	}
}
