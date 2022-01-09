using System;
using System.Reflection;
using TMPro;
using UnityEngine.UI;
using WeaverCore.Utilities;

namespace WeaverCore.Settings.Elements
{
	/// <summary>
	/// A UI Element where the user can input any string value into it
	/// </summary>
	public class StringElement : UIElement
	{
		TMP_InputField inputField;

		void Awake()
		{
			inputField = GetComponentInChildren<TMP_InputField>();
			inputField.onEndEdit.AddListener(OnInputChange);
		}

		/// <inheritdoc/>
		public override bool CanWorkWithAccessor(IAccessor accessor)
		{
			return accessor.MemberType == typeof(string);
		}

		/// <inheritdoc/>
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
