using System;
using System.Reflection;

namespace WeaverCore.Settings
{
	public interface IAccessor
	{
		string FieldName { get; }
		string Description { get; }
		MemberInfo MemberInfo { get; }
		Type MemberType { get; }
		object FieldValue { get; set; }
	}
}
