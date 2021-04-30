using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UI;
using WeaverCore.Utilities;

namespace WeaverCore.Settings.Elements
{

	public class EnumElement : UIElement
	{
		Dropdown dropdown;

		List<KeyValuePair<string, object>> EnumData = new List<KeyValuePair<string, object>>();

		void Awake()
		{
			dropdown = GetComponentInChildren<Dropdown>();
		}

		public override bool CanWorkWithAccessor(IAccessor accessor)
		{
			if (accessor.MemberType != null)
			{
				return accessor.MemberType.IsEnum;
			}
			return false;
		}

		protected override void OnAccessorChanged(IAccessor accessor)
		{
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
		}

		void OnValueChanged(int index)
		{
			FieldAccessor.FieldValue = EnumData[index].Value;
		}
	}
}
