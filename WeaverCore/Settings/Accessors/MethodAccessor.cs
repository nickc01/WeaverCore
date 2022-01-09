using System;
using System.Reflection;

namespace WeaverCore.Settings
{
	/// <summary>
	/// Used for accessing a method on a <see cref="GlobalSettings"/> object
	/// </summary>
	public class MethodAccessor : IAccessor
	{
		public readonly GlobalSettings Panel;
		public readonly MethodInfo Method;
		readonly string _description;
		readonly string _displayName;

		public MethodAccessor(GlobalSettings panel, MethodInfo method, string displayName, string description)
		{
			Panel = panel;
			Method = method;
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
				return Method;
			}
		}

		public object FieldValue
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public Type MemberType
		{
			get
			{
				return null;
			}
		}

		public void CallMethod(object[] parameters)
		{
			Method.Invoke(Panel, parameters);
		}
	}
}
