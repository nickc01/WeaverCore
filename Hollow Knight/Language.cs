/*using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Modding;
using UnityEngine;

namespace Language
{
	// Token: 0x020006AA RID: 1706
	public static class Language
	{
		// Token: 0x1700057F RID: 1407
		// (get) Token: 0x0600286F RID: 10351 RVA: 0x000E3744 File Offset: 0x000E1944
		public static LocalizationSettings settings
		{
			get
			{
				if (Language._settings == null)
				{
					Language._settings = (LocalizationSettings)Resources.Load("Languages/" + Path.GetFileNameWithoutExtension(Language.settingsAssetPath), typeof(LocalizationSettings));
				}
				return Language._settings;
			}
		}

		// Token: 0x06002870 RID: 10352 RVA: 0x000E3790 File Offset: 0x000E1990
		static Language()
		{
			Language.settingsAssetPath = "Assets/Localization/Resources/Languages/LocalizationSettings.asset";
			Language._settings = null;
			Language.currentLanguage = LanguageCode.N;
			Language.LoadAvailableLanguages();
			Language.LoadLanguage();
		}

		// Token: 0x06002871 RID: 10353 RVA: 0x000E37B4 File Offset: 0x000E19B4
		public static void LoadLanguage()
		{
			string text = Language.RestoreLanguageSelection();
			Debug.LogFormat("Restored language code '{0}'", new object[]
			{
				text
			});
			Language.SwitchLanguage(text);
		}

		// Token: 0x06002872 RID: 10354 RVA: 0x000E37E4 File Offset: 0x000E19E4
		private static string RestoreLanguageSelection()
		{
			if (Platform.Current && Platform.Current.SharedData.HasKey("M2H_lastLanguage"))
			{
				string @string = Platform.Current.SharedData.GetString("M2H_lastLanguage", "");
				Debug.LogFormat("Loaded saved language code '{0}'", new object[]
				{
					@string
				});
				if (Language.availableLanguages.Contains(@string))
				{
					return @string;
				}
				Debug.LogErrorFormat("Loaded saved language code '{0}' is not an available language", new object[]
				{
					@string
				});
			}
			if (Language.settings.useSystemLanguagePerDefault)
			{
				SystemLanguage systemLanguage = Platform.Current.GetSystemLanguage();
				Debug.LogFormat("Loaded system language '{0}'", new object[]
				{
					systemLanguage
				});
				string text = Language.LanguageNameToCode(systemLanguage).ToString();
				Debug.LogFormat("Loaded system language code '{0}'", new object[]
				{
					text
				});
				if (Language.availableLanguages.Contains(text))
				{
					return text;
				}
				Debug.LogErrorFormat("System language code '{0}' is not an available language", new object[]
				{
					text
				});
			}
			Debug.LogFormat("Falling back to default language code '{0}'", new object[]
			{
				Language.settings.defaultLangCode
			});
			return LocalizationSettings.GetLanguageEnum(Language.settings.defaultLangCode).ToString();
		}

		// Token: 0x06002873 RID: 10355 RVA: 0x000E391C File Offset: 0x000E1B1C
		public static void LoadAvailableLanguages()
		{
			Language.availableLanguages = new List<string>();
			if (Language.settings.sheetTitles == null || Language.settings.sheetTitles.Length == 0)
			{
				Debug.Log("None available");
				return;
			}
			foreach (object obj in Enum.GetValues(typeof(LanguageCode)))
			{
				LanguageCode languageCode = (LanguageCode)obj;
				if (Language.HasLanguageFile(languageCode.ToString() ?? "", Language.settings.sheetTitles[0]))
				{
					Language.availableLanguages.Add(languageCode.ToString() ?? "");
				}
			}
			StringBuilder stringBuilder = new StringBuilder("Discovered supported languages: ");
			for (int i = 0; i < Language.availableLanguages.Count; i++)
			{
				stringBuilder.Append(Language.availableLanguages[i]);
				if (i < Language.availableLanguages.Count - 1)
				{
					stringBuilder.Append(", ");
				}
			}
			Debug.Log(stringBuilder.ToString());
			Resources.UnloadUnusedAssets();
		}

		// Token: 0x06002874 RID: 10356 RVA: 0x000E3A54 File Offset: 0x000E1C54
		public static string[] GetLanguages()
		{
			return Language.availableLanguages.ToArray();
		}

		// Token: 0x06002875 RID: 10357 RVA: 0x000E3A60 File Offset: 0x000E1C60
		public static bool SwitchLanguage(string langCode)
		{
			return Language.SwitchLanguage(LocalizationSettings.GetLanguageEnum(langCode));
		}

		// Token: 0x06002876 RID: 10358 RVA: 0x000E3A70 File Offset: 0x000E1C70
		public static bool SwitchLanguage(LanguageCode code)
		{
			if (Language.availableLanguages.Contains(code.ToString() ?? ""))
			{
				Language.DoSwitch(code);
				return true;
			}
			Debug.LogError("Could not switch from language " + Language.currentLanguage.ToString() + " to " + code.ToString());
			if (Language.currentLanguage == LanguageCode.N)
			{
				if (Language.availableLanguages.Count > 0)
				{
					Language.DoSwitch(LocalizationSettings.GetLanguageEnum(Language.availableLanguages[0]));
					Debug.LogError("Switched to " + Language.currentLanguage.ToString() + " instead");
				}
				else
				{
					Debug.LogError("Please verify that you have the file: Resources/Languages/" + code.ToString());
					Debug.Break();
				}
			}
			return false;
		}

		// Token: 0x06002877 RID: 10359 RVA: 0x000E3B4C File Offset: 0x000E1D4C
		private static void DoSwitch(LanguageCode newLang)
		{
			if (Platform.Current)
			{
				Platform.Current.SharedData.SetString("M2H_lastLanguage", newLang.ToString() ?? "");
				Platform.Current.SharedData.Save();
			}
			Language.currentLanguage = newLang;
			Language.currentEntrySheets = new Dictionary<string, Dictionary<string, string>>();
			foreach (string text in Language.settings.sheetTitles)
			{
				Language.currentEntrySheets[text] = new Dictionary<string, string>();
				string languageFileContents = Language.GetLanguageFileContents(text);
				if (languageFileContents != "")
				{
					using (XmlReader xmlReader = XmlReader.Create(new StringReader(languageFileContents)))
					{
						while (xmlReader.ReadToFollowing("entry"))
						{
							xmlReader.MoveToFirstAttribute();
							string value = xmlReader.Value;
							xmlReader.MoveToElement();
							string text2 = xmlReader.ReadElementContentAsString().Trim();
							text2 = text2.UnescapeXML();
							Language.currentEntrySheets[text][value] = text2;
						}
					}
				}
			}
			LocalizedAsset[] array = (LocalizedAsset[])UnityEngine.Object.FindObjectsOfType(typeof(LocalizedAsset));
			for (int i = 0; i < array.Length; i++)
			{
				array[i].LocalizeAsset();
			}
			Language.SendMonoMessage("ChangedLanguage", new object[]
			{
				Language.currentLanguage
			});
			if (!ConfigManager.IsSavingConfig)
			{
				ConfigManager.SaveConfig();
			}
		}

		// Token: 0x06002878 RID: 10360 RVA: 0x000E3CCC File Offset: 0x000E1ECC
		public static UnityEngine.Object GetAsset(string name)
		{
			return Resources.Load("Languages/Assets/" + Language.CurrentLanguage().ToString() + "/" + name);
		}

		// Token: 0x06002879 RID: 10361 RVA: 0x000E3D01 File Offset: 0x000E1F01
		private static bool HasLanguageFile(string lang, string sheetTitle)
		{
			return (TextAsset)Resources.Load("Languages/" + lang + "_" + sheetTitle, typeof(TextAsset)) != null;
		}

		// Token: 0x0600287A RID: 10362 RVA: 0x000E3D30 File Offset: 0x000E1F30
		private static string GetLanguageFileContents(string sheetTitle)
		{
			TextAsset textAsset = (TextAsset)Resources.Load("Languages/" + Language.currentLanguage.ToString() + "_" + sheetTitle, typeof(TextAsset));
			if (!(textAsset != null))
			{
				return "";
			}
			return textAsset.text;
		}

		// Token: 0x0600287B RID: 10363 RVA: 0x000E3D87 File Offset: 0x000E1F87
		public static LanguageCode CurrentLanguage()
		{
			return Language.currentLanguage;
		}

		// Token: 0x0600287C RID: 10364 RVA: 0x000E3D8E File Offset: 0x000E1F8E
		public static string Get(string key)
		{
			return Language.Get(key, Language.settings.sheetTitles[0]);
		}

		// Token: 0x0600287D RID: 10365 RVA: 0x000E3DA2 File Offset: 0x000E1FA2
		public static string Get(string key, string sheetTitle)
		{
			return ModHooks.LanguageGet(key, sheetTitle);
		}

		// Token: 0x0600287E RID: 10366 RVA: 0x000E3DAB File Offset: 0x000E1FAB
		public static bool Has(string key)
		{
			return Language.Has(key, Language.settings.sheetTitles[0]);
		}

		// Token: 0x0600287F RID: 10367 RVA: 0x000E3DBF File Offset: 0x000E1FBF
		public static bool Has(string key, string sheetTitle)
		{
			return Language.currentEntrySheets != null && Language.currentEntrySheets.ContainsKey(sheetTitle) && Language.currentEntrySheets[sheetTitle].ContainsKey(key);
		}

		// Token: 0x06002880 RID: 10368 RVA: 0x000E3DE8 File Offset: 0x000E1FE8
		private static void SendMonoMessage(string methodString, params object[] parameters)
		{
			if (parameters != null && parameters.Length > 1)
			{
				Debug.LogError("We cannot pass more than one argument currently!");
			}
			foreach (GameObject gameObject in (GameObject[])UnityEngine.Object.FindObjectsOfType(typeof(GameObject)))
			{
				if (gameObject && gameObject.transform.parent == null)
				{
					if (parameters != null && parameters.Length == 1)
					{
						gameObject.gameObject.BroadcastMessage(methodString, parameters[0], SendMessageOptions.DontRequireReceiver);
					}
					else
					{
						gameObject.gameObject.BroadcastMessage(methodString, SendMessageOptions.DontRequireReceiver);
					}
				}
			}
		}

		// Token: 0x06002881 RID: 10369 RVA: 0x000E3E74 File Offset: 0x000E2074
		public static LanguageCode LanguageNameToCode(SystemLanguage name)
		{
			if (name == SystemLanguage.Afrikaans)
			{
				return LanguageCode.AF;
			}
			if (name == SystemLanguage.Arabic)
			{
				return LanguageCode.AR;
			}
			if (name == SystemLanguage.Basque)
			{
				return LanguageCode.BA;
			}
			if (name == SystemLanguage.Belarusian)
			{
				return LanguageCode.BE;
			}
			if (name == SystemLanguage.Bulgarian)
			{
				return LanguageCode.BG;
			}
			if (name == SystemLanguage.Catalan)
			{
				return LanguageCode.CA;
			}
			if (name == SystemLanguage.Chinese)
			{
				return LanguageCode.ZH;
			}
			if (name == SystemLanguage.Czech)
			{
				return LanguageCode.CS;
			}
			if (name == SystemLanguage.Danish)
			{
				return LanguageCode.DA;
			}
			if (name == SystemLanguage.Dutch)
			{
				return LanguageCode.NL;
			}
			if (name == SystemLanguage.English)
			{
				return LanguageCode.EN;
			}
			if (name == SystemLanguage.Estonian)
			{
				return LanguageCode.ET;
			}
			if (name == SystemLanguage.Faroese)
			{
				return LanguageCode.FA;
			}
			if (name == SystemLanguage.Finnish)
			{
				return LanguageCode.FI;
			}
			if (name == SystemLanguage.French)
			{
				return LanguageCode.FR;
			}
			if (name == SystemLanguage.German)
			{
				return LanguageCode.DE;
			}
			if (name == SystemLanguage.Greek)
			{
				return LanguageCode.EL;
			}
			if (name == SystemLanguage.Hebrew)
			{
				return LanguageCode.HE;
			}
			if (name == SystemLanguage.Hungarian)
			{
				return LanguageCode.HU;
			}
			if (name == SystemLanguage.Icelandic)
			{
				return LanguageCode.IS;
			}
			if (name == SystemLanguage.Indonesian)
			{
				return LanguageCode.ID;
			}
			if (name == SystemLanguage.Italian)
			{
				return LanguageCode.IT;
			}
			if (name == SystemLanguage.Japanese)
			{
				return LanguageCode.JA;
			}
			if (name == SystemLanguage.Korean)
			{
				return LanguageCode.KO;
			}
			if (name == SystemLanguage.Latvian)
			{
				return LanguageCode.LA;
			}
			if (name == SystemLanguage.Lithuanian)
			{
				return LanguageCode.LT;
			}
			if (name == SystemLanguage.Norwegian)
			{
				return LanguageCode.NO;
			}
			if (name == SystemLanguage.Polish)
			{
				return LanguageCode.PL;
			}
			if (name == SystemLanguage.Portuguese)
			{
				return LanguageCode.PT;
			}
			if (name == SystemLanguage.Romanian)
			{
				return LanguageCode.RO;
			}
			if (name == SystemLanguage.Russian)
			{
				return LanguageCode.RU;
			}
			if (name == SystemLanguage.SerboCroatian)
			{
				return LanguageCode.SH;
			}
			if (name == SystemLanguage.Slovak)
			{
				return LanguageCode.SK;
			}
			if (name == SystemLanguage.Slovenian)
			{
				return LanguageCode.SL;
			}
			if (name == SystemLanguage.Spanish)
			{
				return LanguageCode.ES;
			}
			if (name == SystemLanguage.Swedish)
			{
				return LanguageCode.SW;
			}
			if (name == SystemLanguage.Thai)
			{
				return LanguageCode.TH;
			}
			if (name == SystemLanguage.Turkish)
			{
				return LanguageCode.TR;
			}
			if (name == SystemLanguage.Ukrainian)
			{
				return LanguageCode.UK;
			}
			if (name == SystemLanguage.Vietnamese)
			{
				return LanguageCode.VI;
			}
			if (name == SystemLanguage.Hungarian)
			{
				return LanguageCode.HU;
			}
			if (name == SystemLanguage.ChineseSimplified)
			{
				return LanguageCode.ZH;
			}
			if (name == SystemLanguage.ChineseTraditional)
			{
				return LanguageCode.ZH;
			}
			return LanguageCode.N;
		}

		// Token: 0x06002882 RID: 10370 RVA: 0x000E4008 File Offset: 0x000E2208
		public static string GetInternal(string key, string sheetTitle)
		{
			if (Language.currentEntrySheets == null || !Language.currentEntrySheets.ContainsKey(sheetTitle))
			{
				Debug.LogError("The sheet with title \"" + sheetTitle + "\" does not exist!");
				return string.Empty;
			}
			if (Language.currentEntrySheets[sheetTitle].ContainsKey(key))
			{
				return Language.currentEntrySheets[sheetTitle][key];
			}
			return "#!#" + key + "#!#";
		}

		// Token: 0x06002883 RID: 10371 RVA: 0x000E407C File Offset: 0x000E227C
		public static string orig_Get(string key, string sheetTitle)
		{
			if (Language.currentEntrySheets == null || !Language.currentEntrySheets.ContainsKey(sheetTitle))
			{
				Debug.LogError("The sheet with title \"" + sheetTitle + "\" does not exist!");
				return "";
			}
			if (Language.currentEntrySheets[sheetTitle].ContainsKey(key))
			{
				return Language.currentEntrySheets[sheetTitle][key];
			}
			return "#!#" + key + "#!#";
		}

		// Token: 0x04002DA4 RID: 11684
		public static string settingsAssetPath;

		// Token: 0x04002DA5 RID: 11685
		private static LocalizationSettings _settings;

		// Token: 0x04002DA6 RID: 11686
		private static List<string> availableLanguages;

		// Token: 0x04002DA7 RID: 11687
		private static LanguageCode currentLanguage;

		// Token: 0x04002DA8 RID: 11688
		private static Dictionary<string, Dictionary<string, string>> currentEntrySheets;

		// Token: 0x04002DA9 RID: 11689
		private const string LastLanguageKey = "M2H_lastLanguage";
	}
}
*/