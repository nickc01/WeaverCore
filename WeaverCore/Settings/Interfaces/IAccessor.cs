using System;
using System.Reflection;

namespace WeaverCore.Settings
{
	/// <summary>
	/// Base class for "Accessors", which are types that act as links between <see cref="WeaverCore.Settings.Elements.UIElement"/> and properties in a <see cref="GlobalSettings"/> class
	/// </summary>
	public interface IAccessor
	{
		/// <summary>
		/// The name of the field this accessor is accessing
		/// </summary>
		string FieldName { get; }

		/// <summary>
		/// A description of the field (if any)
		/// </summary>
		string Description { get; }

		/// <summary>
		/// The member info of the field (if any)
		/// </summary>
		MemberInfo MemberInfo { get; }

		/// <summary>
		/// The type of member this accessor is using
		/// </summary>
		Type MemberType { get; }

		/// <summary>
		/// The value of the member field
		/// </summary>
		object FieldValue { get; set; }
	}
}
