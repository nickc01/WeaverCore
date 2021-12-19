using System;
using System.Reflection;

namespace WeaverCore.Settings
{
	public abstract class CustomPropertyAccessor_Base : IAccessor
	{
		public readonly GlobalSettings Panel;
		protected readonly string _description;
		protected readonly string _displayName;

		public CustomPropertyAccessor_Base(GlobalSettings panel, string displayName, string description)
		{
			Panel = panel;
			_description = description;
			_displayName = displayName;
		}

		public string FieldName
		{
			get
			{
				return _displayName;
			}
		}

		public string Description
		{
			get
			{
				return _description;
			}
		}

		public MemberInfo MemberInfo
		{
			get
			{
				return null;
			}
		}

		public abstract Type MemberType { get; }

		public abstract object FieldValue { get; set; }
	}
}
