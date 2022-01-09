using System;

namespace WeaverCore.Settings
{
	/// <summary>
	/// Determines the order a UI Element will be displayed in. The lower the number, the higher up it will be displayed in the Settings Menu
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public sealed class SettingOrderAttribute : Attribute
	{
		/// <summary>
		/// The order of the settings property, which determines where the setting should be positioned
		/// </summary>
		public int Order { get; private set; }

		public SettingOrderAttribute(int order)
		{
			Order = order;
		}
	}
}
