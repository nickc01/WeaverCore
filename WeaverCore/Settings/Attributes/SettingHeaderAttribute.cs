using System;
using UnityEngine;

namespace WeaverCore.Settings
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public sealed class SettingHeaderAttribute : Attribute
	{
		/// <summary>
		/// The header of the settings property
		/// </summary>
		public string HeaderText { get; private set; }

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
	public sealed class TranslatedHeaderAttribute : Attribute
	{
		/// <summary>
		/// The header of the settings property
		/// </summary>
		public string HeaderText { get; private set; }

		/// <summary>
		/// Applies a header to the settings property
		/// </summary>
		/// <param name="headerText">The text on the header</param>
		public TranslatedHeaderAttribute(string headerText)
		{
			HeaderText = headerText;
		}
	}
}
