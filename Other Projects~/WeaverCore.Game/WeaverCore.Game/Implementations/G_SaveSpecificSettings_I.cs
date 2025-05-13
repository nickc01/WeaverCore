using Modding;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

		Dictionary<string,JToken> GetModData()
		{
			var field = typeof(GameManager).GetField("moddedData",BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			WeaverLog.Log("MODDED DATA FIELD = " + field);
			var saveDataType = field.FieldType;
			WeaverLog.Log("MODDED DATA FIELD TYPE = " + saveDataType);
			WeaverLog.Log("GAMEMANAGER = " + GameManager.instance);
			var value = field.GetValue(GameManager.instance);

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
			if (settings.Enabled)
			{
				var modData = GetModData();
				//var result = JsonUtility.ToJson(settings, true);

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
	}
}
