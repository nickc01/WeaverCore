using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WeaverCore.Attributes;
using WeaverCore.Configuration;
using WeaverCore.Enums;
using WeaverCore.Features;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
	public class WeaverConfigScreen : CanvasExtension
	{
		[SerializeField]
		GameObject ToggleButton;

		[SerializeField]
		GameObject ConfigMainMenu;

		[Header("Tab Area")]
		[SerializeField]
		Transform TabContents;
		[SerializeField]
		Tab FirstTab;

		[Header("Settings Area")]
		[SerializeField]
		GameObject SettingsArea;
		[SerializeField]
		Transform settingContents;
		[SerializeField]
		GameObject propertyHolder;
		[SerializeField]
		Text DescriptionText;
		[SerializeField]
		Text SettingTitleText;

		int tabIndex = 0;
		bool hasTabs = false;
		bool settingsEnabled = false;

		bool onTitleScreen = false;

		List<Tab> Tabs = new List<Tab>();
		Tab selectedTab = null;
		Dictionary<Type, IConfigProperty> PropertyTemplates = new Dictionary<Type, IConfigProperty>();

		List<IConfigProperty> InstancedProperties = new List<IConfigProperty>();

		public bool Open { get; private set; }

		public static WeaverConfigScreen Instance { get; private set; }

		public string SettingDescription
		{
			get
			{
				return DescriptionText.text;
			}
			set
			{
				DescriptionText.text = value;
			}
		}

		[AfterModLoad(typeof(WeaverCore.Internal.WeaverCore))]
		static void OnGameStart()
		{
			//WeaverLog.Log("AFTER WEAVERCORE HAS LOADED");
			//WeaverLog.Log("Canvas On Game Start");
			if (CoreInfo.LoadState == RunningState.Editor)
			{
				//var prefab = Registry.GetAllFeatures<WeaverConfigScreen>().First();
				//GameObject.Instantiate(prefab, WeaverCanvas.Content);
			}
			else
			{
				//WeaverLog.Log("C_B");
				SceneManager.sceneLoaded += CreateOnSceneChange;
				for (int i = 0; i < SceneManager.sceneCount; i++)
				{
					var scene = SceneManager.GetSceneAt(i);
					if (scene.name == "Menu_Title")
					{
						//WeaverLog.Log("C_A");
						var prefab = Registry.GetAllFeatures<WeaverConfigScreen>().First();
						//WeaverLog.Log("Instantiating Prefab = " + prefab);
						Instance = GameObject.Instantiate(prefab, WeaverCanvas.Content);
						return;
					}
				}
			}
		}

		public override bool AddedOnStartup
		{
			get
			{
				if (CoreInfo.LoadState == RunningState.Editor)
				{
					return base.AddedOnStartup;
				}
				else
				{
					return false;
				}
			}
		}

		void Awake()
		{
			//WeaverLog.Log("STARTING WEAVER CANVAS");
			ConfigMainMenu.SetActive(false);
			//UnboundCoroutine.Start(Waiter());
			SceneManager.sceneUnloaded += DestroyOnSceneChange;
			if (CoreInfo.LoadState == RunningState.Editor)
			{
				EnteringTitleScreen();
			}
			else
			{
				for (int i = 0; i < SceneManager.sceneCount; i++)
				{
					var scene = SceneManager.GetSceneAt(i);
					if (scene.name == "Menu_Title")
					{
						onTitleScreen = true;
						EnteringTitleScreen();
						break;
					}
				}
			}

			foreach (var component in propertyHolder.GetComponentsInChildren<IConfigProperty>())
			{
				PropertyTemplates.Add(component.BindingFieldType, component);
			}
		}

		/*void OnDestroy()
		{
			WeaverLog.Log("STOPPING WEAVER CANVAS");
		}*/

		static void CreateOnSceneChange(Scene scene, LoadSceneMode loadSceneMode)
		{
			//WeaverLog.Log("Scene Loaded = " + scene.name);
			if (Instance == null && scene.name == "Menu_Title")
			{
				var prefab = Registry.GetAllFeatures<WeaverConfigScreen>().First();
				Instance = GameObject.Instantiate(prefab, WeaverCanvas.Content);
			}
		}

		private void DestroyOnSceneChange(Scene scene)
		{
			if (scene.name == "Menu_Title")
			{
				SceneManager.sceneUnloaded -= DestroyOnSceneChange;
				Instance = null;
				Destroy(gameObject);
			}
			/*if (scene.name == "Menu_Title")
			{
				if (!onTitleScreen)
				{
					onTitleScreen = true;
					EnteringTitleScreen();
				}
			}
			else
			{
				if (onTitleScreen)
				{
					onTitleScreen = false;
					LeavingTitleScreen();
				}
			}*/
		}


		public void OpenMenu()
		{
			ConfigMainMenu.SetActive(true);
			SettingsArea.SetActive(false);
			ToggleButton.SetActive(false);
			RefreshTabs(GlobalWeaverSettings.GetAllSettings());
		}

		public void CloseMenu()
		{
			ConfigMainMenu.SetActive(false);
			ToggleButton.SetActive(true);
			if (selectedTab != null)
			{
				selectedTab.Button.interactable = true;
			}
			DisableSettingsArea();
			foreach (var settings in GlobalWeaverSettings.GetAllSettings())
			{
				settings.SaveSettings();
			}
		}

		void EnteringTitleScreen()
		{
			//WeaverLog.Log("Entering Title Screen");
			//Open = true;
			//transform.parent.gameObject.SetActive(true);
			gameObject.SetActive(true);
			ConfigMainMenu.SetActive(false);
			ToggleButton.SetActive(true);
		}

		void LeavingTitleScreen()
		{
			//WeaverLog.Log("Leaving Title Screen");
			Open = false;
			gameObject.SetActive(false);
			CloseMenu();
		}

		public void OnTabClicked(Tab tab)
		{
			if (selectedTab != null)
			{
				selectedTab.Button.interactable = true;
				DisableSettingsArea();
			}
			selectedTab = tab;
			selectedTab.Button.interactable = false;
			EnableSettingsArea();
		}

		void RefreshTabs(IEnumerable<GlobalWeaverSettings> allSettings)
		{
			for (int i = TabContents.childCount - 1; i >= 0; i--)
			{
				var child = TabContents.GetChild(i);
				if (child != FirstTab.transform)
				{
					Destroy(child);
				}
			}
			FirstTab.gameObject.SetActive(false);

			int currentIndex = 0;
			Tabs.Clear();
			foreach (var settings in allSettings)
			{
				if (FirstTab.gameObject.activeSelf == false)
				{
					FirstTab.gameObject.SetActive(true);
					FirstTab.TabText.text = settings.TabName;
					FirstTab.TabIndex = 0;
					FirstTab.settings = settings;
					Tabs.Add(FirstTab);
				}
				else
				{
					var tab = Instantiate(FirstTab, TabContents);
					tab.TabText.text = settings.TabName;
					tab.TabIndex = currentIndex;
					tab.settings = settings;
					Tabs.Add(tab);
				}
				currentIndex++;
			}
		}

		void EnableSettingsArea()
		{
			if (settingsEnabled)
			{
				return;
			}
			var currentSettings = selectedTab.settings;

			SettingsArea.SetActive(true);
			SettingTitleText.text = currentSettings.TabName;
			SettingDescription = "Hover over an element for more information about it";
			settingsEnabled = true;

			foreach (var field in currentSettings.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			{
				if (typeof(GlobalWeaverSettings).IsAssignableFrom(field.DeclaringType) && field.DeclaringType != typeof(GlobalWeaverSettings))
				{
					var nsValue = field.GetCustomAttributes(typeof(NonSerializedAttribute), false);
					if (nsValue.GetLength(0) == 0)
					{
						var sfValue = field.GetCustomAttributes(typeof(SerializeField), false);
						if (sfValue.GetLength(0) > 0 || field.IsPublic)
						{
							if (PropertyTemplates.ContainsKey(field.FieldType))
							{
								var configPropTemplate = PropertyTemplates[field.FieldType];
								var gameObjectTemplate = (configPropTemplate as Component).gameObject;

								var configProp = GameObject.Instantiate(gameObjectTemplate, settingContents).GetComponent<IConfigProperty>();
								configProp.Title = StringUtilities.Prettify(field.Name);
								configProp.BindToField(currentSettings, field);
								InstancedProperties.Add(configProp);
							}
						}
					}
				}
			}


		}

		void DisableSettingsArea()
		{
			if (!settingsEnabled)
			{
				return;
			}
			foreach (var configProp in InstancedProperties)
			{
				var gameObject = (configProp as Component).gameObject;
				Destroy(gameObject);
			}
			settingsEnabled = false;
			InstancedProperties.Clear();
		}
	}

}
