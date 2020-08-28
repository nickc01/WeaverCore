using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using WeaverCore.Configuration;

namespace WeaverCore.Assets.Components
{
	public class BoolProperty : ConfigProperty
	{
		[SerializeField]
		Dropdown dropdown;


		public override Type BindingFieldType
		{
			get
			{
				return typeof(bool);
			}
		}

		private void Awake()
		{
			dropdown.onValueChanged.AddListener(i => UpdateField());
		}

		protected override void InitializeValue()
		{
			if (Binded)
			{
				bool value = (bool)field.GetValue(settings);

				dropdown.value = value ? 1 : 0;
			}
		}

		protected override void UpdateField()
		{
			if (Binded)
			{
				int dropdownValue = dropdown.value;
				bool value = false;
				if (dropdownValue == 1)
				{
					value = true;
				}

				field.SetValue(settings, value);
			}
		}
	}
}
