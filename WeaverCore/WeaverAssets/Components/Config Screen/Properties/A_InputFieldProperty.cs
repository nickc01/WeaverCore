using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using WeaverCore.Configuration;

namespace WeaverCore.Assets.Components
{
	public abstract class A_InputFieldProperty : ConfigProperty
	{
		[SerializeField]
		protected InputField input;
		protected string previousInput;

		protected virtual void Awake()
		{
			input.onValueChanged.AddListener(s => UpdateField());
		}

		protected override void InitializeValue()
		{
			if (Binded)
			{
				input.text = field.GetValue(settings).ToString();
			}
		}

		protected override void UpdateField()
		{
			if (Binded)
			{
				field.SetValue(settings, Convert.ChangeType(input.text, field.FieldType));
				previousInput = input.text;
			}
		}
	}
}
