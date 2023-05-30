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
			var saveDataType = field.FieldType;
			var value = field.GetValue(GameManager.instance);

			var modDataF = saveDataType.GetField("modData", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			return (Dictionary<string, JToken>)modDataF.GetValue(value);
		}

		static Dictionary<Type, string> defaultState = new Dictionary<Type, string>();

		public override void LoadSettings(SaveSpecificSettings settings)
		{
            //WeaverLog.Log("LOADING SETTINGS");
            var modData = GetModData();
			var settingsType = settings.GetType();
			var key = settingsType.FullName;
			if (!defaultState.ContainsKey(settingsType))
			{
				defaultState.Add(settingsType,JsonUtility.ToJson(settings));
			}
			if (modData.ContainsKey(key))
			{
				var token = modData[key];
				JsonUtility.FromJsonOverwrite(token.ToString(), settings);
			}
			else
			{
				JsonUtility.FromJsonOverwrite(defaultState[settingsType],settings);
			}
		}

		public override void SaveSettings(SaveSpecificSettings settings)
		{
			var modData = GetModData();
			var result = JsonUtility.ToJson(settings,true);
			//WeaverLog.Log("RESULT = " + result);

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

		[OnInit]
		static void Init()
		{
            //WeaverLog.Log("SAVE INIT");
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
