using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Settings
{

	/// <summary>
	/// When placed on a field, property or function, it will be guaranteed to show up in the options menu. If you pass in false into the constructor, it will be forced to not show
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property,AllowMultiple = false, Inherited = false)]
	public sealed class SettingFieldAttribute : Attribute
	{
		/// <summary>
		/// How the field should be shown
		/// </summary>
		public Visibility Visibility { get; private set; }

		public string DisplayName { get; private set; }

		/// <summary>
		/// Applied to a field, property, or function to determine whether it should be shown in the settings menu
		/// </summary>
		/// <param name="visibility">Determines in what scenario should the field be visible in</param>
		/// <param name="displayName">The display name of the field, property, or function</param>
		public SettingFieldAttribute(Visibility visibility = Visibility.Both, string displayName = null)
		{
			Visibility = visibility;
		}
	}
}
