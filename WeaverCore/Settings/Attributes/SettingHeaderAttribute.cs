using System;
using UnityEngine;

namespace WeaverCore.Settings
{
#if UNITY_EDITOR
	[System.ComponentModel.Browsable(false)]
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
#endif
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public abstract class SettingHeaderAttribute_BASE : Attribute
	{
		/// <summary>
		/// The header of the settings property
		/// </summary>
		public string HeaderText { get; protected set; }
	}

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public sealed class SettingHeaderAttribute : SettingHeaderAttribute_BASE
	{
		/// <summary>
		/// Applies a header to the settings property
		/// </summary>
		/// <param name="headerText">The text on the header</param>
		public SettingHeaderAttribute(string headerText)
		{
			HeaderText = headerText;
		}
	}

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public sealed class LangSettingHeaderAttribute : SettingHeaderAttribute_BASE
	{
		/// <summary>
		/// Applies a header to the settings property
		/// </summary>
		/// <param name="headerText">The text on the header</param>
		public LangSettingHeaderAttribute(string sheetTitle, string key, string fallback = null)
		{
			HeaderText = WeaverLanguage.GetString(key, sheetTitle, fallback);
            if (string.IsNullOrEmpty(HeaderText))
            {
				HeaderText = "UNSPECIFIED_HEADER";
            }
		}

		public LangSettingHeaderAttribute(string key, string fallback = null)
		{
			HeaderText = WeaverLanguage.GetString(key, fallback);
			if (string.IsNullOrEmpty(HeaderText))
			{
				HeaderText = "UNSPECIFIED_HEADER";
			}
		}
	}
}
