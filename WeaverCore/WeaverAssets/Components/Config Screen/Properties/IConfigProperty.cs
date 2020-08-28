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
	public interface IConfigProperty
	{
		Type BindingFieldType
		{
			get;
		}

		string Title
		{
			get;
			set;
		}

		void BindToField(ModSettings settings, FieldInfo field);
	}


	public abstract class ConfigProperty : MonoBehaviour, IConfigProperty
	{
		[SerializeField]
		protected Text titleText;

		protected ModSettings settings;
		protected FieldInfo field;

		public abstract Type BindingFieldType { get; }

		protected bool Binded
		{
			get
			{
				return settings != null && field != null;
			}
		}

		public string Title
		{
			get
			{
				return titleText.text;
			}
			set
			{
				titleText.text = value;
			}
		}

		public void BindToField(ModSettings settings, FieldInfo field)
		{
			this.settings = settings;
			this.field = field;
			InitializeValue();
			UpdateField();
		}

		protected abstract void InitializeValue();


		protected abstract void UpdateField();
	}
}
