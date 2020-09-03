using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WeaverCore.Configuration;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{

	public class WeaverConfigScreen : MonoBehaviour
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

		void Awake()
		{
			Instance = this;
			ConfigMainMenu.SetActive(false);
			//UnboundCoroutine.Start(Waiter());
			SceneManager.sceneLoaded += SceneManager_sceneLoaded;
			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				var scene = SceneManager.GetSceneAt(i);
				if (scene.name == "Menu_Title")
				{
					onTitleScreen = true;
					EnteringTitleScreen();
				}
				else if (CoreInfo.LoadState == Enums.RunningState.Editor)
				{
					EnteringTitleScreen();
				}
			}

			foreach (var component in propertyHolder.GetComponentsInChildren<IConfigProperty>())
			{
				PropertyTemplates.Add(component.BindingFieldType, component);
			}
		}

		private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
		{
			if (scene.name == "Menu_Title")
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
			}
		}


		public void OpenMenu()
		{
			ConfigMainMenu.SetActive(true);
			SettingsArea.SetActive(false);
			ToggleButton.SetActive(false);
			RefreshTabs(ModSettings.GetAllSettings());
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
			foreach (var settings in ModSettings.GetAllSettings())
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

		void RefreshTabs(IEnumerable<ModSettings> allSettings)
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
				if (typeof(ModSettings).IsAssignableFrom(field.DeclaringType) && field.DeclaringType != typeof(ModSettings))
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
