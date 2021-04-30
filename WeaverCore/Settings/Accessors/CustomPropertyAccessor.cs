using System;

namespace WeaverCore.Settings
{
	public class CustomPropertyAccessor<T> : CustomPropertyAccessor_Base
	{
		public readonly Func<T> Getter;
		public readonly Action<T> Setter;

		public CustomPropertyAccessor(Panel panel, Func<T> getter, Action<T> setter, string displayName, string description) : base(panel,displayName,description)
		{
			Getter = getter;
			Setter = setter;
		}

		public override Type MemberType
		{
			get
			{
				return typeof(T);
			}
		}

		public override object FieldValue
		{
			get
			{
				return Getter();
			}
			set
			{
				Setter((T)value);
			}
		}
	}
}
