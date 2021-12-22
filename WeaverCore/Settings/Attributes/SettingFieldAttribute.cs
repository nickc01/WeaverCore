using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Settings
{
#if UNITY_EDITOR
	[System.ComponentModel.Browsable(false)]
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
#endif
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property,AllowMultiple = false, Inherited = false)]
	public abstract class SettingFieldAttribute_BASE : Attribute
	{
		/// <summary>
		/// Whether the field should be enabled and in what circumstances
		/// </summary>
		public EnabledType IsEnabled { get; protected set; }

		public string DisplayName { get; protected set; }
	}


	/// <summary>
	/// This attribute will cause a field, property or function to show up in the Weaver Settings Screen
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public sealed class SettingFieldAttribute : SettingFieldAttribute_BASE
	{
		/// <summary>
		/// Applied to a field, property, or function to determine whether it should be shown in the settings menu
		/// </summary>
		/// <param name="enabled">Determines in what scenario should the field be visible in</param>
		/// <param name="displayName">The display name of the field, property, or function</param>
		public SettingFieldAttribute(EnabledType enabled = EnabledType.Both, string displayName = null)
		{
			IsEnabled = enabled;
			DisplayName = displayName;
		}
	}

	/// <summary>
	/// Similar to <see cref="SettingFieldAttribute"/>, but can accept a language key and/or sheetTitle for translating the field into multiple languages
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public sealed class LangSettingFieldAttribute : SettingFieldAttribute_BASE
	{
		/// <summary>
		/// Applied to a field, property, or function to determine whether it should be shown in the settings menu
		/// </summary>
		/// <param name="enabled">Determines in what scenario should the field be visible in</param>
		/// <param name="displayName">The display name of the field, property, or function</param>
		public LangSettingFieldAttribute(string sheetTitle, string key, EnabledType enabled = EnabledType.Both)
		{
			IsEnabled = enabled;
			DisplayName = WeaverLanguage.GetString(sheetTitle, key);
		}

		/// <summary>
		/// Applied to a field, property, or function to determine whether it should be shown in the settings menu
		/// </summary>
		/// <param name="enabled">Determines in what scenario should the field be visible in</param>
		/// <param name="displayName">The display name of the field, property, or function</param>
		public LangSettingFieldAttribute(string key, EnabledType enabled = EnabledType.Both)
		{
			IsEnabled = enabled;
			DisplayName = WeaverLanguage.GetString(key);
		}
	}

}
