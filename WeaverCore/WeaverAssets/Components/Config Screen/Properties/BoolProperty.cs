/*using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using WeaverCore.Configuration;

namespace WeaverCore.Assets.Components
{
	public class BoolProperty : ConfigProperty
	{
		[SerializeField]
		Toggle toggle;


		public override Type BindingFieldType
		{
			get
			{
				return typeof(bool);
			}
		}

		private void Awake()
		{
			toggle.onValueChanged.AddListener(b => UpdateField());
		}

		protected override void InitializeValue()
		{
			if (Binded)
			{
				toggle.isOn = (bool)field.GetValue(settings);
			}
		}

		protected override void UpdateField()
		{
			if (Binded)
			{
				field.SetValue(settings, toggle.isOn);
			}
		}
	}
}
*/