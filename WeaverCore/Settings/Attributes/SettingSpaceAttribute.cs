using System;

namespace WeaverCore.Settings
{
	/// <summary>
	/// Used to insert a space between two UI Elements in the Settings Menu
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
	public sealed class SettingSpaceAttribute : Attribute
	{
		/// <summary>
		/// The spacing of the settings property from the one above it
		/// </summary>
		public float Spacing { get; private set; }

		/// <summary>
		/// Applies a spacing to the settings property
		/// </summary>
		/// <param name="spacing">How much spacing is applied</param>
		public SettingSpaceAttribute(float spacing = 35.5f)
		{
			Spacing = spacing;
		}
	}
}
