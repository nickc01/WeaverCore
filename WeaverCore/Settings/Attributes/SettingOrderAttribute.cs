using System;

namespace WeaverCore.Settings
{
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
