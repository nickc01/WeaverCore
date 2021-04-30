using System;

namespace WeaverCore.Settings
{
	/// <summary>
	/// Limits a numerical field or property to a limited range
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public sealed class SettingRangeAttribute : Attribute
	{
		public readonly double min;
		public readonly double max;

		public SettingRangeAttribute(double min, double max)
		{
			this.min = min;
			this.max = max;
		}

		public SettingRangeAttribute(float min, float max)
		{
			this.min = min;
			this.max = max;
		}

		public SettingRangeAttribute(int min, int max)
		{
			this.min = min;
			this.max = max;
		}
	}
}
