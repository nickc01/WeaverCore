using System;
using System.Reflection;
using UnityEngine.UI;
using WeaverCore.Utilities;

namespace WeaverCore.Settings.Elements
{
	public class StringElement : UIElement
	{
		InputField inputField;

		void Awake()
		{
			inputField = GetComponentInChildren<InputField>();
			inputField.onEndEdit.AddListener(OnInputChange);
		}

		public override bool CanWorkWithAccessor(IAccessor accessor)
		{
			return accessor.MemberType == typeof(string);
			/*if (memberInfo is FieldInfo)
			{
				return ((FieldInfo)memberInfo).FieldType == typeof(string);
			}
			else if (memberInfo is PropertyInfo)
			{
				return ((PropertyInfo)memberInfo).PropertyType == typeof(string);
			}
			return false;*/
		}

		protected override void OnAccessorChanged(IAccessor accessor)
		{
			inputField.text = (string)accessor.FieldValue;
			Title = accessor.FieldName;
			base.OnAccessorChanged(accessor);
		}

		void OnInputChange(string text)
		{
			FieldAccessor.FieldValue = text;
		}
	}
}
