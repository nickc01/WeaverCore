/*using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WeaverCore.Assets.Components
{
	public class EnumProperty : ConfigProperty
	{
		[SerializeField]
		Dropdown dropdown;

		Dictionary<int, object> valueToDropdownIndex = new Dictionary<int, object>();

		void Awake()
		{
			dropdown.onValueChanged.AddListener(num => UpdateField());
		}

		public override Type BindingFieldType
		{
			get
			{
				return typeof(Enum);
			}
		}

		protected override void InitializeValue()
		{
			if (Binded)
			{
				dropdown.ClearOptions();
				valueToDropdownIndex.Clear();

				var type = field.FieldType;

				var names = Enum.GetNames(type);
				var values = Enum.GetValues(type);
				var currentValue = field.GetValue(settings);
				int currentValueIndex = 0;

				List<Dropdown.OptionData> Options = new List<Dropdown.OptionData>();

				for (int i = 0; i < values.Length; i++)
				{
					Options.Add(new Dropdown.OptionData(names[i]));
					var indexValue = values.GetValue(i);
					valueToDropdownIndex.Add(i, indexValue);
					if (currentValue.Equals(indexValue))
					{
						currentValueIndex = i;
					}
				}
				dropdown.AddOptions(Options);

				dropdown.value = currentValueIndex;

				//input.text = field.GetValue(settings).ToString();
				//dropdown.val = field.GetValue(settings);
			}
		}

		protected override void UpdateField()
		{
			if (Binded)
			{
				field.SetValue(settings,valueToDropdownIndex[dropdown.value]);
				//field.SetValue(settings, Convert.ChangeType(input.text, field.FieldType));
				//previousInput = input.text;
			}
		}
	}
}
*/