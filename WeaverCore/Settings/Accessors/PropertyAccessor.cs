﻿using System;
using System.Reflection;

namespace WeaverCore.Settings
{

	public class PropertyAccessor : IAccessor
	{
		public readonly Panel Panel;
		public readonly PropertyInfo Property;
		readonly string _description;
		readonly string _displayName;

		public PropertyAccessor(Panel panel, PropertyInfo property, string displayName, string description)
		{
			Panel = panel;
			Property = property;
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

		public object FieldValue
		{
			get
			{
				return Property.GetValue(Panel, null);
			}
			set
			{
				Property.SetValue(Panel, value, null);
			}
		}

		public MemberInfo MemberInfo
		{
			get
			{
				return Property;
			}
		}

		public Type MemberType
		{
			get
			{
				return Property.PropertyType;
			}
		}
	}
}
