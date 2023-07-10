using System;
using WeaverCore.Utilities;

namespace WeaverCore.Settings
{
#if UNITY_EDITOR
	[System.ComponentModel.Browsable(false)]
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
#endif
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public abstract class SettingDescriptionAttribute_BASE : Attribute
	{
		/// <summary>
		/// The description of the settings property
		/// </summary>
		public string Description { get; protected set; }
	}

	/// <summary>
	/// Used to describe a method, field, or property on a <see cref="GlobalSettings"/> object
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public sealed class SettingDescriptionAttribute : SettingDescriptionAttribute_BASE
	{
		/// <summary>
		/// Applies a description to the settings property
		/// </summary>
		/// <param name="description">The text describing the settings property</param>
		public SettingDescriptionAttribute(string description)
		{
			Description = description;
		}
	}

	/// <summary>
	/// Used to describe a method, field, or property on a <see cref="GlobalSettings"/> object, but also uses <seealso cref="WeaverLanguage.GetString(string, string, string)"/> to translate it
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public sealed class LangSettingDescriptionAttribute : SettingDescriptionAttribute_BASE
	{
		/// <summary>
		/// Applies a description to the settings property
		/// </summary>
		/// <param name="description">The text describing the settings property</param>
		public LangSettingDescriptionAttribute(string sheetTitle, string key, string fallback = null)
		{
			//WeaverLog.Log($"TRANSLATING DESCRIPTION = {sheetTitle} - {key}");
			Description = WeaverLanguage.GetString(sheetTitle, key, fallback);
            if (Description == null)
            {
				Description = "INVALID_DESCRIPTION";
            }
		}

		/// <summary>
		/// Applies a description to the settings property
		/// </summary>
		/// <param name="description">The text describing the settings property</param>
		public LangSettingDescriptionAttribute(string key, string fallback = null)
		{
			//WeaverLog.Log($"TRANSLATING DESCRIPTION = {key}");
			Description = WeaverLanguage.GetString(key, fallback);
			if (Description == null)
			{
				Description = "INVALID_DESCRIPTION";
			}
		}
	}
}
