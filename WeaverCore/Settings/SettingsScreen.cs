using WeaverCore.Attributes;
using WeaverCore.Features;
using UnityEngine;
using WeaverCore.Utilities;
using System.Collections.Generic;
using WeaverCore.Settings.Elements;
using System;
using System.Linq;
using TMPro;
using System.Reflection;
using System.Collections.ObjectModel;

namespace WeaverCore.Settings
{
	/// <summary>
	/// Used for sorting <see cref="MemberInfo"/> objects by their setting order
	/// </summary>
	class MemberInfoSorter : IComparer<MemberInfo>
	{
		Dictionary<MemberInfo, int> OrderCache = new Dictionary<MemberInfo, int>();

		Comparer<int> intComparer = Comparer<int>.Default;

		public int Compare(MemberInfo x, MemberInfo y)
		{
			return intComparer.Compare(GetOrder(x), GetOrder(y));
		}

		int GetOrder(MemberInfo info)
		{
			if (OrderCache.TryGetValue(info,out var value))
			{
				return value;
			}
			else
			{
				var orderAttribute = info.GetCustomAttribute<SettingOrderAttribute>();
				if (orderAttribute != null)
				{
					OrderCache.Add(info, orderAttribute.Order);
					return orderAttribute.Order;
				}
				else
				{
					OrderCache.Add(info, info.MetadataToken);
					return info.MetadataToken;
				}
			}
		}
	}

	/// <summary>
	/// Represents the screen in-game where WeaverCore related mods can have their settings changed. See <see cref="GlobalSettings"/> for more info
	/// </summary>
	public sealed class SettingsScreen : MonoBehaviour
	{
		/// <summary>
		/// The current instance of the settings menu. Is null when the settings menu is not visible
		/// </summary>
		public static SettingsScreen Instance { get; private set; }
		static SettingsScreen prefab;

		static List<GlobalSettings> _panels = new List<GlobalSettings>();

		/// <summary>
		/// A list of all the currently registered panels
		/// </summary>
		public static IEnumerable<GlobalSettings> Panels
		{
			get
			{
				return _panels;
			}
		}

		/// <summary>
		/// The currently selected panel. Returns null if there is no panel selected
		/// </summary>
		public static GlobalSettings SelectedPanel
		{
			get
			{
				if (Instance != null && Instance.selectedTab != null)
				{
					return Instance.selectedTab.Panel;
				}
				return null;
			}
		}

		/// <summary>
		/// A list of all the UI elements currently shown
		/// </summary>
		public static ReadOnlyCollection<UIElement> UIElements
		{
			get
			{
				if (Instance != null)
				{
					return Instance.currentElements.AsReadOnly();
				}
				else
				{
					return null;
				}
			}
		}

		/// <summary>
		/// Called whenever a UI Element is added to the Settings Menu
		/// </summary>
		public static event Action<UIElement> ElementAdded;

		/// <summary>
		/// Called whenever a UI Element is removed from the Settings Menu
		/// </summary>
		public static event Action<UIElement> ElementRemoved;

		[Header("Prefabs")]
		[SerializeField]
		[Tooltip("A list of all the possible UI elements")]
		List<UIElement> SettingsElementPrefabs;
		[SerializeField]
		[Tooltip("The prefab that will represent the tabs at the top of the settings screen")]
		Tab TabPrefab;

		[Space]
		[Header("Settings Area")]
		[SerializeField]
		[Tooltip("The main area that contains all the ui elements and descriptions")]
		GameObject SettingsArea;
		[SerializeField]
		[Tooltip("The object that represents the entire settings screen")]
		GameObject ConfigScreen;
		[SerializeField]
		[Tooltip("The button for showing and hiding the weavercore settings menu")]
		GameObject ShowConfigButton;
		[SerializeField]
		[Tooltip("The parent object that will contain all the ui elements")]
		Transform elementContainer;
		[SerializeField]
		[Tooltip("The parent object that will contain all the tabs")]
		Transform tabContainer;
		[SerializeField]
		[Tooltip("The text that represents the description of each element")]
		TextMeshProUGUI DescriptionText;
		[SerializeField]
		[Tooltip("The text that represents the mod title")]
		TextMeshProUGUI SettingTitleText;

		string defaultDescriptionText;
		List<Tab> tabs = new List<Tab>();
		Tab selectedTab = null;
		List<UIElement> currentElements = new List<UIElement>();


		[AfterCameraLoad]
		static void Init()
		{
			prefab = WeaverAssets.LoadWeaverAsset<GameObject>("Weaver Settings Screen").GetComponent<SettingsScreen>();

			WeaverGameManager.OnGameStateChange += GameStateChange;
#if UNITY_EDITOR
			SpawnMenu();
#endif
		}

		[OnFeatureLoad]
		static void OnFeatureLoad(GlobalSettings panel)
		{
			if (!IsPanelRegistered(panel.GetType()))
			{
				RegisterPanel(panel);
			}
		}

		[OnFeatureUnload]
		static void OnFeatureUnload(GlobalSettings panel)
		{
			if (IsPanelRegistered(panel.GetType()))
			{
				UnRegisterPanel(panel);
			}
		}

		private static void GameStateChange(GlobalEnums.GameState state)
		{
			if (state == GlobalEnums.GameState.MAIN_MENU || state == GlobalEnums.GameState.PAUSED)
			{
				SpawnMenu();
			}
			else
			{
				DestroyMenu();
			}
		}

		static void SpawnMenu()
		{
			if (Instance == null)
			{
				Instance = GameObject.Instantiate(prefab, WeaverDebugCanvas.Content);
				Instance.RebuildInterface();
			}
		}

		static void DestroyMenu()
		{
			if (Instance != null)
			{
				foreach (var panel in Panels)
				{
					panel.SaveSettings();
				}
				Instance.RebuildInterface();
				GameObject.Destroy(Instance.gameObject);
				ElementRemoved = null;
				ElementAdded = null;
				Instance = null;
			}
		}

		/// <summary>
		/// Registers a panel to the settings menu so it can be displayed in the menu
		/// </summary>
		/// <typeparam name="T">The type of panel to register</typeparam>
		/// <returns>Returns an instance to the panel</returns>
		public static T RegisterPanel<T>() where T : GlobalSettings
		{
			if (_panels.Any(p => p.GetType() == typeof(T)))
			{
				throw new Exception("A panel of type [" + typeof(T).FullName + "] has already been registered");
			}
			var newPanel = ScriptableObject.CreateInstance<T>();
			RegisterPanel(newPanel);
			return newPanel;
		}

		/// <summary>
		/// Registers a panel to the settings menu so it can be displayed in the menu
		/// </summary>
		/// <param name="panel">The panel to register</param>
		public static void RegisterPanel(GlobalSettings panel)
		{
			if (_panels.Any(p => p.GetType() == panel.GetType()))
			{
				throw new Exception("A panel of type [ " + panel.GetType().FullName + " ] has already been registered");
			}
			panel.LoadSettings();
			_panels.Add(panel);

			if (Instance != null)
			{
				Instance.RebuildInterface();
			}
			panel.OnRegister();
		}

		/// <summary>
		/// Unregisters an panel from the settings menu
		/// </summary>
		/// <param name="panel">The panel to unregister</param>
		public static void UnRegisterPanel(GlobalSettings panel)
		{
			panel.OnUnRegister();
			if (!_panels.Contains(panel))
			{
				throw new Exception("The panel [ " + panel.GetType().FullName + " ] is not registered to the settings screen");
			}
			panel.SaveSettings();
			_panels.Remove(panel);

			if (Instance != null)
			{
				Instance.RebuildInterface();
			}
		}

		/// <summary>
		/// Unregisters an panel from the settings menu
		/// </summary>
		/// <typeparam name="T">The panel type to unregister</typeparam>
		public static void UnRegisterPanel<T>() where T : GlobalSettings
		{
			var panel = _panels.FirstOrDefault(p => typeof(T).IsAssignableFrom(p.GetType()));
			if (panel == null)
			{
				throw new Exception("A panel of type [ " + typeof(T).FullName + " ] is not registered to the settings screen");
			}
			else
			{
				UnRegisterPanel(panel);
			}
		}
		
		/// <summary>
		/// Is the <see cref="GlobalSettings"/> of the specified type registered to the settings menu?
		/// </summary>
		/// <typeparam name="T">The type to check for</typeparam>
		/// <returns>Returns true if the specific type of <see cref="GlobalSettings"/> is registered with the settings menu</returns>
		public static bool IsPanelRegistered<T>() where T : GlobalSettings
		{
			return IsPanelRegistered(typeof(T));
		}

		/// <summary>
		/// Is the <see cref="GlobalSettings"/> of the specified type registered to the settings menu?
		/// </summary>
		/// <param name="panelType">The type to check for</param>
		/// <returns>Returns true if the specific type of <see cref="GlobalSettings"/> is registered with the settings menu</returns>
		public static bool IsPanelRegistered(Type panelType)
		{
			return _panels.Any(p => panelType.IsAssignableFrom(p.GetType()));
		}

		void Awake()
		{
			Instance = this;
			defaultDescriptionText = DescriptionText.text;
			ShowConfigButton.SetActive(true);
			SettingsArea.SetActive(false);
			ConfigScreen.SetActive(false);
		}

		/// <summary>
		/// Rebuilds the entire interface so it's back to a clean slate
		/// </summary>
		void RebuildInterface()
		{
			DeselectCurrentTab();

			foreach (var tab in tabs)
			{
				GameObject.Destroy(tab.gameObject);
			}

			foreach (var element in currentElements)
			{
				ElementRemoved?.Invoke(element);
				GameObject.Destroy(element.gameObject);
			}

			tabs.Clear();
			currentElements.Clear();

			foreach (var panel in _panels)
			{
				CreateTab(panel);
			}
		}

		/// <summary>
		/// Creates a tab for the global settings panel
		/// </summary>
		Tab CreateTab(GlobalSettings panel)
		{
			var tab = GameObject.Instantiate(TabPrefab,tabContainer);
			tab.TextComponent.text = panel.TabName;
			tab.Panel = panel;
			tabs.Add(tab);
			return tab;
		}

		/// <summary>
		/// Called when the player clicks on a tab to open up it's settings
		/// </summary>
		/// <param name="tab">The tab that was clicked on</param>
		internal void SelectTab(Tab tab)
		{
			if (selectedTab == tab)
			{
				return;
			}

			DeselectCurrentTab();

			selectedTab = tab;
			tab.Button.interactable = false;

			SettingTitleText.text = tab.Panel.TabName;
			DescriptionText.text = defaultDescriptionText;

			SettingsArea.SetActive(true);

			CreateElements(tab.Panel);

			tab.Panel.OnPanelOpen();
		}

		/// <summary>
		/// Creates all default UI Elements for a Global Settings Panel
		/// </summary>
		void CreateElements(GlobalSettings panel)
		{
			var members = new List<MemberInfo>();
			var panelType = panel.GetType();

			members.AddRange(panelType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
			members.Sort(new MemberInfoSorter());

			foreach (var member in members)
			{
				SettingFieldAttribute_BASE settings = null;
				if (MemberIsUsable(member,out settings))
				{
					var name = settings != null && settings.DisplayName != null ? settings.DisplayName : StringUtilities.Prettify(member.Name);
					var description = GetDescriptionOfMember(member);
					AddMember(panel, member, name, description);
				}
			}
			foreach (var element in currentElements)
			{
				element.UpdateDisplayValue();
			}

		}

		/// <summary>
		/// Removes a UI Element from the Settings Menu
		/// </summary>
		/// <param name="element">The element to remove</param>
		/// <returns>Returns whether the element was actually removed</returns>
		internal bool RemoveUIElement(UIElement element)
		{
			if (currentElements.Contains(element))
			{
				ElementRemoved?.Invoke(element);
				currentElements.Remove(element);
				GameObject.Destroy(element.gameObject);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Adds a UI Element to the Settings Menu
		/// </summary>
		internal UIElement AddElement(GlobalSettings panel, IAccessor accessor)
		{
			foreach (var element in SettingsElementPrefabs)
			{
				if (element.CanWorkWithAccessor(accessor))
				{
					return AddElementRaw(panel, element, accessor);
				}
			}
			return null;
		}

		UIElement AddElementRaw(GlobalSettings panel, UIElement prefab, IAccessor accessor)
		{
			var newElement = GameObject.Instantiate(prefab, elementContainer);
			newElement.Panel = panel;
			newElement.FieldAccesorRaw = accessor;
			currentElements.Add(newElement);
			ElementAdded?.Invoke(newElement);
			return newElement;
		}

		internal UIElement AddMember(GlobalSettings panel, MemberInfo member, string displayName, string description)
		{
			IAccessor accessor = null;
			if (member is FieldInfo)
			{
				accessor = new FieldAccessor(panel, (FieldInfo)member, displayName, description);
			}
			else if (member is PropertyInfo)
			{
				accessor = new PropertyAccessor(panel, (PropertyInfo)member, displayName, description);
			}
			else if (member is MethodInfo)
			{
				accessor = new MethodAccessor(panel, (MethodInfo)member, displayName, description);
			}
			else
			{
				return null;
			}

			var element = AddElement(panel, accessor);

			var spacing = GetSpacingOfMember(member);
			if (spacing > 0f)
			{
				var spacingElement = AddSpacing(panel, element, spacing);
				spacingElement.Order--;
			}

			var header = GetHeaderOfMember(member);
			if (header != null)
			{
				var headerElement = AddHeading(panel, header, element);
				headerElement.Order--;
			}

			return element;
		}

		internal SpaceElement AddSpacing(GlobalSettings panel, UIElement sourceElement, float? spacing = null)
		{
			var element = (SpaceElement)AddElementRaw(panel, SettingsElementPrefabs.FirstOrDefault(ui => ui is SpaceElement), null);
			if (spacing != null)
			{
				element.Spacing = spacing.Value;
			}
			return element;
		}

		internal HeaderElement AddHeading(GlobalSettings panel, string headerText,UIElement sourceElement, float? fontSize = null)
		{
			var element = (HeaderElement)AddElementRaw(panel, SettingsElementPrefabs.FirstOrDefault(ui => ui is HeaderElement), null);
			element.Title = headerText;
			element.BoundToElement = sourceElement;
			if (fontSize != null)
			{
				element.TitleComponent.fontSize = fontSize.Value;
			}
			return element;
		}



		/// <summary>
		/// Returns whether this member can be used in the settings menu. Also returns the settings field attribute data if the member has one
		/// </summary>
		/// <param name="memberInfo">The member to check</param>
		/// <param name="attribute">The settings field information if the member has one</param>
		/// <returns></returns>
		static bool MemberIsUsable(MemberInfo memberInfo, out SettingFieldAttribute_BASE attribute)
		{
			bool hasAttribute = HasAttribute(memberInfo, out attribute);
			if (memberInfo is FieldInfo)
			{
				var field = (FieldInfo)memberInfo;
				if (hasAttribute)
				{
					return ShouldBeEnabled(attribute.IsEnabled);
					//return true;
				}
				else if ((field.IsPublic || (field.IsPrivate && field.IsDefined(typeof(SerializeField), false) && !field.IsDefined(typeof(HideInInspector),false))) && !field.IsStatic && !field.IsDefined(typeof(NonSerializedAttribute), false))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else if (memberInfo is PropertyInfo)
			{
				var property = (PropertyInfo)memberInfo;
				return property.GetGetMethod(true) != null && property.GetSetMethod(true) != null && hasAttribute && ShouldBeEnabled(attribute.IsEnabled);
			}
			else if (memberInfo is MethodInfo)
			{
				return hasAttribute && ShouldBeEnabled(attribute.IsEnabled);
			}

			return false;
		}

		/// <summary>
		/// Checks if the specified member has the specified attribute type
		/// </summary>
		/// <typeparam name="T">The type of attribute to look for</typeparam>
		/// <param name="member">The member to check under</param>
		/// <param name="attribute">The output attribute if the member has the attribute</param>
		/// <returns>Returns whether the member has the specified attribute type</returns>
		static bool HasAttribute<T>(MemberInfo member, out T attribute) where T : Attribute
		{
			var attributes = member.GetCustomAttributes(typeof(T), false);
			if (attributes != null && attributes.GetLength(0) > 0)
			{
				attribute = (T)attributes[0];
				return true;
			}
			attribute = null;
			return false;
		}

		void DeselectCurrentTab()
		{
			if (selectedTab != null)
			{
				selectedTab.Panel.OnPanelClose();
				selectedTab.Panel.SaveSettings();
				selectedTab.Button.interactable = true;

				RemoveAllUIElements();

				selectedTab = null;
				SettingsArea.SetActive(false);
			}
		}

		/// <summary>
		/// Removes all currently added UI Elements from the Settings Menu
		/// </summary>
		public void RemoveAllUIElements()
		{
			for (int i = currentElements.Count - 1; i >= 0; i--)
			{
				ElementRemoved?.Invoke(currentElements[i]);
				GameObject.Destroy(currentElements[i].gameObject);
			}

			currentElements.Clear();
		}

		/// <summary>
		/// Shows the config area
		/// </summary>
		public void Show()
		{
			ShowConfigButton.SetActive(false);
			SettingsArea.SetActive(false);
			ConfigScreen.SetActive(true);
			RebuildInterface();
		}

		/// <summary>
		/// Hides the config area
		/// </summary>
		public void Hide()
		{
			DeselectCurrentTab();
			ShowConfigButton.SetActive(true);
			SettingsArea.SetActive(false);
			ConfigScreen.SetActive(false);
		}

		/// <summary>
		/// Updates the description text in the Settings Area
		/// </summary>
		/// <param name="text">The new description to set</param>
		public static void SetDescription(string text)
		{
			if (Instance != null)
			{
				Instance.DescriptionText.text = text;
			}
		}

		/// <summary>
		/// Gets the display name of the member
		/// </summary>
		/// <param name="memberInfo">The member to check</param>
		/// <returns>Returns the display name of the member. Returns null if <paramref name="memberInfo"/> is null. </returns>
		public static string GetDisplayNameOfMember(MemberInfo memberInfo)
		{
			if (memberInfo == null)
			{
				return null;
			}
			SettingFieldAttribute_BASE settings = null;
			if (HasAttribute(memberInfo, out settings) && settings.DisplayName != null)
			{
				return settings.DisplayName;
			}
			else
			{
				return StringUtilities.Prettify(memberInfo.Name);
			}
		}

		/// <summary>
		/// Gets the description of the member
		/// </summary>
		/// <param name="member">The member to check</param>
		/// <returns>Returns the description of the member. Returns null if <paramref name="member"/>is null</returns>
		public static string GetDescriptionOfMember(MemberInfo member)
		{
			if (member == null)
			{
				return null;
			}
			SettingDescriptionAttribute_BASE descAttribute;
			TooltipAttribute tooltipAttribute;
			if (HasAttribute(member, out descAttribute))
			{
				return StringUtilities.AddSpaces(StringUtilities.AddNewLines(descAttribute.Description));
			}
			else if (HasAttribute(member, out tooltipAttribute))
			{
				return StringUtilities.AddSpaces(StringUtilities.AddNewLines(tooltipAttribute.tooltip));
			}
			else
			{
				return "";
			}
		}

		/// <summary>
		/// Gets the header of the member
		/// </summary>
		/// <param name="member">The member to check</param>
		/// <returns>Returns the header of the member, or null if the member does not have a header</returns>
		public static string GetHeaderOfMember(MemberInfo member)
		{
			if (member == null)
			{
				return null;
			}

			SettingHeaderAttribute_BASE settingHeaderAttribute;
			HeaderAttribute headerAttribute;

			if (HasAttribute(member, out settingHeaderAttribute))
			{
				return settingHeaderAttribute.HeaderText;
			}
			else if (HasAttribute(member, out headerAttribute))
			{
				return headerAttribute.header;
			}
			return null;
		}

		/// <summary>
		/// Gets how much spacing the member should be from the previous member
		/// </summary>
		/// <param name="member">The member to check</param>
		/// <returns>Returns how much spacing should be between this member and the previous</returns>
		public static float GetSpacingOfMember(MemberInfo member)
		{
			var spaceAttributes = (SpaceAttribute[])member.GetCustomAttributes(typeof(SpaceAttribute), false);
			var settingSpaceAttributes = (SettingSpaceAttribute[])member.GetCustomAttributes(typeof(SettingSpaceAttribute), false);

			float totalSpacing = 0f;

			foreach (var a in spaceAttributes)
			{
				totalSpacing += (a.height / 8.0f) * 35.5f;
			}

			foreach (var a in settingSpaceAttributes)
			{
				totalSpacing += a.Spacing;
			}

			return totalSpacing;
		}

		/// <summary>
		/// If the member is numerical, this function returns whether or not it's limited to a set range of numbers
		/// </summary>
		/// <param name="member">The member to check</param>
		/// <param name="min">The min range</param>
		/// <param name="max">The max range</param>
		/// <returns>Returns whether this member is limited to a specified range</returns>
		public static bool GetRangeOfNumberMember(MemberInfo member, out double min, out double max)
		{
			RangeAttribute rangeAttribute;
			SettingRangeAttribute settingRangeAttribute;
			
			if (HasAttribute(member, out settingRangeAttribute))
			{
				min = settingRangeAttribute.min;
				max = settingRangeAttribute.max;
				return true;
			}
			else if (HasAttribute(member, out rangeAttribute))
			{
				min = rangeAttribute.min;
				max = rangeAttribute.max;
				return true;
			}
			min = 0.0;
			max = 0.0;
			return false;
		}

		/// <summary>
		/// Whether the pause menu is open or not
		/// </summary>
		public static bool InPauseMenu
		{
			get
			{
				return GameManager.instance.gameState == GlobalEnums.GameState.PAUSED;
			}
		}

		/// <summary>
		/// Checks if a UI Element should be visible or not
		/// </summary>
		/// <param name="visibility">The visibility of the UI Element</param>
		/// <returns>Returns whether or not the UI Element should be visible</returns>
		public static bool ShouldBeEnabled(EnabledType visibility)
		{
			if (InPauseMenu && (visibility & EnabledType.PauseOnly) == EnabledType.PauseOnly)
			{
				return true;
			}
			else if (!InPauseMenu && (visibility & EnabledType.MenuOnly) == EnabledType.MenuOnly)
			{
				return true;
			}
			return false;
		}
	}
}
