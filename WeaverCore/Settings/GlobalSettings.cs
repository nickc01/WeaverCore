using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;
using WeaverCore.Settings.Elements;
using WeaverCore.Utilities;

namespace WeaverCore.Settings
{
    /// <summary>
    /// Store global information about a mod, and creates a panel in the Settings Menu where they can be changed
    /// 
    /// Inherit from this class to create a custom panel in the settings menu, then use <see cref="SettingsScreen.RegisterPanel{T}"/> to register the panel and make it show up in the settings menu
    /// </summary>
    [ShowFeature]
	public abstract class GlobalSettings : ScriptableObject
	{
		static WeaverSettingsPanel_I impl = ImplFinder.GetImplementation<WeaverSettingsPanel_I>();

		/// <summary>
		/// Called when the panel gets selected in the weaver settings
		/// </summary>
		internal protected virtual void OnPanelOpen()
		{

		}

		/// <summary>
		/// Called when the panel gets deselected in the weaver settings
		/// </summary>
		internal protected virtual void OnPanelClose()
		{

		}

		/// <summary>
		/// Called when the panel gets registered with the Settings Menu
		/// </summary>
		internal protected virtual void OnRegister()
		{

		}

		/// <summary>
		/// Called when the panel gets unregistered from the settings menu
		/// </summary>
		internal protected virtual void OnUnRegister()
		{

		}

		/// <summary>
		/// Called right before the settings are saved to a file
		/// </summary>
		protected virtual void OnSave()
		{

		}

		/// <summary>
		/// Called after the settings are loaded from a file
		/// </summary>
		protected virtual void OnLoad()
		{

		}

		/// <summary>
		/// The name of the tab. Tabs are the buttons on the top row that are used to select the panel in the weaver settings
		/// </summary>
		public virtual string TabName
		{
			get
			{
				return StringUtilities.Prettify(name);
			}
		}

		/// <summary>
		/// Can be used anywhere and anytime to get a specific settings panel
		/// </summary>
		/// <typeparam name="T">The type of panel to get</typeparam>
		/// <returns>If the panel is registered, it will return a reference to the settings panel</returns>
		public static T GetSettings<T>() where T : GlobalSettings
		{
			return (T)SettingsScreen.Panels.FirstOrDefault(p => typeof(T).IsAssignableFrom(p.GetType()));
		}

		/// <summary>
		/// Gets all the setting panels currently registered
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<GlobalSettings> GetAllSettings()
		{
			return SettingsScreen.Panels;
		}

		/// <summary>
		/// Loads the stored settings for this panel from disk
		/// </summary>
		public void LoadSettings()
		{
			impl.LoadSettings(this);
			OnLoad();
		}

		/// <summary>
		/// Saves the stored settings for this panel to disk
		/// </summary>
		public void SaveSettings()
		{
			OnSave();
			impl.SaveSettings(this);
		}

		/// <summary>
		/// Whether the panel is currently selected in the weaver settings
		/// </summary>
		public bool Selected
		{
			get
			{
				return SettingsScreen.SelectedPanel == this;
			}
		}

		/// <summary>
		/// A list of all the currently shown UI elements. Returns null if the panel is not selected
		/// </summary>
		public ReadOnlyCollection<UIElement> UIElements
		{
			get
			{
				if (Selected)
				{
					return SettingsScreen.UIElements;
				}
				else
				{
					return null;
				}
			}
		}

		/// <summary>
		/// Removes a specific UI Element from the Settings Menu
		/// </summary>
		/// <param name="element">The element to remove</param>
		/// <returns>Returns whether the element has been removed or not</returns>
		public bool RemoveUIElement(UIElement element)
		{
			if (!UIElements.Contains(element))
			{
				return false;
			}
			return SettingsScreen.Instance.RemoveUIElement(element);
		}

		/// <summary>
		/// Adds a UI element to the panel for displaying the field in the settings menu.
		/// 
		/// Returns null if the Settings Menu isn't open or the field is not compatible
		/// </summary>
		/// <param name="fieldName">The name of the field to display</param>
		/// <returns>The UIElement that has been added to the panel</returns>
		public UIElement AddFieldElement(string fieldName)
		{
			var field = GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if (field == null)
			{
				throw new Exception("The field " + fieldName + " does not exit on the type " + GetType().FullName);
			}
			return AddFieldElement(field, SettingsScreen.GetDisplayNameOfMember(field), SettingsScreen.GetDescriptionOfMember(field));
		}

		/// <summary>
		/// Adds a UI element to the panel for displaying the field in the settings menu.
		/// 
		/// Returns null if the Settings Menu isn't open or the field is not compatible
		/// </summary>
		/// <param name="fieldName">The name of the field to display</param>
		/// <param name="displayName">The name of the field displayed on the panel</param>
		/// <param name="description">The description of the field when hovering over the element</param>
		/// <returns>The UIElement that has been added to the panel</returns>
		public UIElement AddFieldElement(string fieldName, string displayName, string description)
		{
			var fieldInfo = GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if (fieldInfo == null)
			{
				throw new Exception("The field " + fieldName + " does not exit on the type " + GetType().FullName);
			}
			return AddFieldElement(fieldInfo,displayName,description);
		}

		/// <summary>
		/// Adds a UI element to the panel for displaying the field in the settings menu.
		/// 
		/// Returns null if the Settings Menu isn't open or the field is not compatible
		/// </summary>
		/// <param name="field">The field to display</param>
		/// <returns>The UIElement that has been added to the panel</returns>
		public UIElement AddFieldElement(FieldInfo field)
		{
			return AddFieldElement(field, SettingsScreen.GetDisplayNameOfMember(field), SettingsScreen.GetDescriptionOfMember(field));
		}

		/// <summary>
		/// Adds a UI element to the panel for displaying the field in the settings menu.
		/// 
		/// Returns null if the Settings Menu isn't open or the field is not compatible
		/// </summary>
		/// <param name="field">The field to display</param>
		/// <param name="displayName">The name of the field displayed on the panel</param>
		/// <param name="description">The description of the field when hovering over the element</param>
		/// <returns>The UIElement that has been added to the panel</returns>
		public UIElement AddFieldElement(FieldInfo field, string displayName, string description)
		{
			if (!field.DeclaringType.IsAssignableFrom(GetType()))
			{
				throw new ArgumentException("The field " + field.DeclaringType + ":" + field.Name + " is not declared in the type or a base of the type [ " + GetType().FullName + " ]");
			}
			if (SettingsScreen.Instance != null)
			{
				var element = SettingsScreen.Instance.AddMember(this, field, displayName, description);
				element.UpdateDisplayValue();
				return element;
			}
			else
			{
				return null;
			}
		}


		/// <summary>
		/// Adds a UI element to the panel for displaying the property in the settings menu.
		/// 
		/// Returns null if the Settings Menu isn't open or the property is not compatible
		/// </summary>
		/// <param name="propertyName">The name of the property to display</param>
		/// <returns>The UIElement that has been added to the panel</returns>
		public UIElement AddPropertyElement(string propertyName)
		{
			var property = GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if (property == null)
			{
				throw new Exception("The property " + propertyName + " does not exit on the type " + GetType().FullName);
			}
			return AddPropertyElement(property, SettingsScreen.GetDisplayNameOfMember(property), SettingsScreen.GetDescriptionOfMember(property));
		}

		/// <summary>
		/// Adds a UI element to the panel for displaying the property in the settings menu.
		/// 
		/// Returns null if the Settings Menu isn't open or the property is not compatible
		/// </summary>
		/// <param name="propertyName">The name of the property to display</param>
		/// <param name="displayName">The name of the property displayed on the panel</param>
		/// <param name="description">The description of the property when hovering over the element</param>
		/// <returns>The UIElement that has been added to the panel</returns>
		public UIElement AddPropertyElement(string propertyName, string displayName, string description)
		{
			var property = GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if (property == null)
			{
				throw new Exception("The property " + propertyName + " does not exit on the type " + GetType().FullName);
			}
			return AddPropertyElement(property, displayName, description);
		}

		/// <summary>
		/// Adds a UI element to the panel for displaying the property in the settings menu.
		/// 
		/// Returns null if the Settings Menu isn't open or the property is not compatible
		/// </summary>
		/// <param name="property">The property to display</param>
		/// <returns>The UIElement that has been added to the panel</returns>
		public UIElement AddPropertyElement(PropertyInfo property)
		{
			return AddPropertyElement(property, SettingsScreen.GetDisplayNameOfMember(property), SettingsScreen.GetDescriptionOfMember(property));
		}

		/// <summary>
		/// Adds a UI element to the panel for displaying the property in the settings menu.
		/// 
		/// Returns null if the Settings Menu isn't open or the property is not compatible
		/// </summary>
		/// <param name="property">The property to display</param>
		/// <param name="displayName">The name of the property displayed on the panel</param>
		/// <param name="description">The description of the property when hovering over the element</param>
		/// <returns>The UIElement that has been added to the panel</returns>
		public UIElement AddPropertyElement(PropertyInfo property, string displayName, string description)
		{
			if (!property.DeclaringType.IsAssignableFrom(GetType()))
			{
				throw new ArgumentException("The property " + property.DeclaringType + ":" + property.Name + " is not declared in the type or a base of the type [ " + GetType().FullName + " ]");
			}
			if (SettingsScreen.Instance != null)
			{
				var element = SettingsScreen.Instance.AddMember(this, property, displayName, description);
				element.UpdateDisplayValue();
				return element;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Adds a custom property to the panel. Both retrieval and setting of the value can be customized
		/// </summary>
		/// <typeparam name="T">The type of the property element</typeparam>
		/// <param name="getter">The getter function for getting the value</param>
		/// <param name="setter">The setter function for setting the value</param>
		/// <param name="displayName">The name of the property displayed on the panel</param>
		/// <param name="description">The description of the property when hovering over the element</param>
		/// <returns>The UIElement that has been added to the panel</returns>
		public UIElement AddPropertyElement<T>(Func<T> getter, Action<T> setter, string displayName, string description)
		{
			if (SettingsScreen.Instance != null)
			{
				var element = SettingsScreen.Instance.AddElement(this, new CustomPropertyAccessor<T>(this, getter, setter, displayName, description));
				element.UpdateDisplayValue();
				return element;
			}
			return null;
		}

		/// <summary>
		/// Adds a button to the settings menu
		/// </summary>
		/// <param name="methodName">The name of the method to call when the button is pressed</param>
		/// <returns>The UIElement that has been added to the panel</returns>
		public UIElement AddButtonElement(string methodName)
		{
			var method = GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
			if (method == null)
			{
				throw new Exception("The method " + methodName + " does not exist on the type " + GetType().FullName);
			}
			return AddButtonElement(method, SettingsScreen.GetDisplayNameOfMember(method), SettingsScreen.GetDescriptionOfMember(method));
		}

		/// <summary>
		/// Adds a button to the settings menu
		/// </summary>
		/// <param name="methodName">The name of the method to call when the button is pressed</param>
		/// <param name="displayName">The name of the property displayed on the panel</param>
		/// <param name="description">The description of the property when hovering over the element</param>
		/// <returns>The UIElement that has been added to the panel</returns>
		public UIElement AddButtonElement(string methodName, string displayName, string description)
		{
			var method = GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
			if (method == null)
			{
				throw new Exception("The method " + methodName + " does not exist on the type " + GetType().FullName);
			}
			return AddButtonElement(method, displayName, description);
		}

		/// <summary>
		/// Adds a button to the settings menu
		/// </summary>
		/// <param name="methodInfo">The method to call when the button is pressed</param>
		/// <returns>The UIElement that has been added to the panel</returns>
		public UIElement AddButtonElement(MethodInfo methodInfo)
		{
			return AddButtonElement(methodInfo, SettingsScreen.GetDisplayNameOfMember(methodInfo), SettingsScreen.GetDescriptionOfMember(methodInfo));
		}

		/// <summary>
		/// Adds a button to the settings menu
		/// </summary>
		/// <param name="methodInfo">The method to call when the button is pressed</param>
		/// <param name="displayName">The name of the property displayed on the panel</param>
		/// <param name="description">The description of the property when hovering over the element</param>
		/// <returns>The UIElement that has been added to the panel</returns>
		public UIElement AddButtonElement(MethodInfo methodInfo, string displayName, string description)
		{
			if (!methodInfo.DeclaringType.IsAssignableFrom(GetType()))
			{
				throw new ArgumentException("The method " + methodInfo.DeclaringType + ":" + methodInfo.Name + " is not declared in the type or a base of the type [ " + GetType().FullName + " ]");
			}

			if (SettingsScreen.Instance != null)
			{
				var element = SettingsScreen.Instance.AddElement(this, new MethodAccessor(this, methodInfo, displayName, description));
				element.UpdateDisplayValue();
				return element;
			}
			return null;
		}

		/// <summary>
		/// Adds a button to the settings menu
		/// </summary>
		/// <param name="onClick">The action to call when the button is pressed</param>
		/// <param name="displayName">The name of the property displayed on the panel</param>
		/// <param name="description">The description of the property when hovering over the element</param>
		/// <returns>The UIElement that has been added to the panel</returns>
		public UIElement AddButtonElement(Action onClick, string displayName, string description)
		{
			if (SettingsScreen.Instance != null)
			{
				var element = SettingsScreen.Instance.AddElement(this, new CustomFunctionAccessor(this, onClick, displayName, description));
				element.UpdateDisplayValue();
				return element;
 			}
			return null;
		}

		/// <summary>
		/// Removes all elements from the settings menu
		/// </summary>
		public void RemoveAllElements()
		{
			if (SettingsScreen.Instance != null)
			{
				SettingsScreen.Instance.RemoveAllUIElements();
			}
		}

		/// <summary>
		/// Adds a spacing element
		/// </summary>
		/// <returns>Returns the spacing element that was just created</returns>
		public SpaceElement AddSpacing()
		{
			if (SettingsScreen.Instance != null)
			{
				var element = SettingsScreen.Instance.AddSpacing(this,null);
				element.UpdateDisplayValue();
				return element;
			}
			return null;
		}

		/// <summary>
		/// Adds a spacing element
		/// </summary>
		/// <param name="spacing">The amount of spacing to add. 35.5 is the default</param>
		/// <returns>Returns the spacing element that was just created</returns>
		public SpaceElement AddSpacing(float spacing)
		{
			if (SettingsScreen.Instance != null)
			{
				var element = SettingsScreen.Instance.AddSpacing(this, null, spacing);
				element.UpdateDisplayValue();
				return element;
 			}
			return null;
		}

		/// <summary>
		/// Adds a header
		/// </summary>
		/// <param name="headerText">The text the header will have</param>
		/// <returns>Returns the header element that was just created</returns>
		public HeaderElement AddHeading(string headerText)
		{
			if (SettingsScreen.Instance != null)
			{
				var element = SettingsScreen.Instance.AddHeading(this, headerText, null);
				element.UpdateDisplayValue();
				return element;
 			}
			return null;
		}

		/// <summary>
		/// Adds a header
		/// </summary>
		/// <param name="headerText">The text the header will have</param>
		/// <param name="fontSize">The font size the header will have. 58 is the default</param>
		/// <returns>Returns the header element that was just created</returns>
		public HeaderElement AddHeading(string headerText, float fontSize)
		{
			if (SettingsScreen.Instance != null)
			{
				var element = SettingsScreen.Instance.AddHeading(this, headerText,null, fontSize);
				element.UpdateDisplayValue();
				return element;			}
			return null;
		}

		/// <summary>
		/// Refreshes all UI Elements so that they are displaying the most recent version of the fields and properties of this panel
		/// </summary>
		public void RefreshAllElements()
		{
			foreach (var element in UIElements)
			{
				element.Refresh();
			}
		}

		/// <summary>
		/// Gets an element with the corresponding member name
		/// </summary>
		/// <param name="memberName">The member name to look for</param>
		/// <returns>Returns the corresponding element with the same member name, or null if no matches were found</returns>
		public UIElement GetElement(string memberName)
		{
			return UIElements.FirstOrDefault(ui => ui.FieldAccessor != null && ui.FieldAccessor.MemberInfo != null && ui.FieldAccessor.MemberInfo.Name == memberName);
		}

		/// <summary>
		/// Gets a header element with the same title name
		/// </summary>
		/// <param name="headerTitle">The header title to look for</param>
		/// <returns>Returns the corresponding header element with the same title, or null if no matches were found</returns>
		public UIElement GetHeaderElement(string headerTitle)
		{
			return UIElements.FirstOrDefault(ui => ui is HeaderElement header && header.Title == headerTitle);
		}

		/// <summary>
		/// If true, then the Weaver Settings Menu is being opened up in the pause menu.
		/// 
		/// If false, then the Weaver Settings Menu is being opened up in the main menu.
		/// </summary>
		public static bool InPauseMenu
		{
			get
			{
				return GameManager.instance.gameState == GlobalEnums.GameState.PAUSED;
			}
		}
	}
}
