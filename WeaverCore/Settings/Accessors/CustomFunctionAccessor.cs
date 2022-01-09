using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WeaverCore.Settings
{
	/// <summary>
	/// Used for accessing a function on a <see cref="GlobalSettings"/> object
	/// </summary>
	public class CustomFunctionAccessor : IAccessor
	{
		public readonly GlobalSettings Panel;
		public readonly PropertyInfo Property;
		readonly string _description;
		readonly string _displayName;
		readonly Action _action;

		public CustomFunctionAccessor(GlobalSettings panel, Action action, string displayName, string description)
		{
			Panel = panel;
			_description = description;
			_displayName = displayName;
			_action = action;
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

		public Type MemberType
		{
			get
			{
				return _action.GetType();
			}
		}

		public object FieldValue
		{
			get
			{
				return _action;
			}
			set
			{
				
			}
		}
	}
}
