using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UI;
using WeaverCore.Utilities;

namespace WeaverCore.Settings.Elements
{

	/// <summary>
	/// A UI element which can have it's value selected from a list of predefined values
	/// </summary>
	public class EnumElement : UIElement
	{
		Dropdown dropdown;

		List<KeyValuePair<string, object>> EnumData = new List<KeyValuePair<string, object>>();

		void Awake()
		{
			dropdown = GetComponentInChildren<Dropdown>();
		}

		/// <inheritdoc/>
		public override bool CanWorkWithAccessor(IAccessor accessor)
		{
			if (accessor.MemberType != null)
			{
				return accessor.MemberType.IsEnum;
			}
			return false;
		}

		/// <inheritdoc/>
		protected override void OnAccessorChanged(IAccessor accessor)
		{
			base.OnAccessorChanged(accessor);
			dropdown.onValueChanged.RemoveAllListeners();
			var type = accessor.MemberType;

			Title = accessor.FieldName;

			EnumData.Clear();

			List<string> OptionNames = new List<string>();

			foreach (var val in Enum.GetValues(type))
			{
				var valName = Enum.GetName(type, val);
				EnumData.Add(new KeyValuePair<string, object>(valName, val));
				OptionNames.Add(valName);
			}

			dropdown.ClearOptions();
			dropdown.AddOptions(OptionNames);
			dropdown.value = EnumData.FindIndex(kv => kv.Value.Equals(accessor.FieldValue));

			dropdown.onValueChanged.AddListener(OnValueChanged);
			base.OnAccessorChanged(accessor);
		}

		void OnValueChanged(int index)
		{
			FieldAccessor.FieldValue = EnumData[index].Value;
		}
	}
}
