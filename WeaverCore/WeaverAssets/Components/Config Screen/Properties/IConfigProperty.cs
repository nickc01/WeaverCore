using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
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

		void BindToField(GlobalWeaverSettings settings, FieldInfo field);
	}

	public abstract class ConfigProperty : MonoBehaviour, IConfigProperty, IPointerEnterHandler
	{
		class ToolTipSorter : IComparer<TooltipAttribute>
		{
			public int Compare(TooltipAttribute x, TooltipAttribute y)
			{
				return Comparer<int>.Default.Compare(x.order, y.order);
			}

			public static ToolTipSorter Instance = new ToolTipSorter();
		}

		protected string description;
		[SerializeField]
		protected Text titleText;

		protected GlobalWeaverSettings settings;
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

		public void BindToField(GlobalWeaverSettings settings, FieldInfo field)
		{
			this.settings = settings;
			this.field = field;
			if (field != null)
			{
				description = "";
				var tooltips = ((TooltipAttribute[])field.GetCustomAttributes(typeof(TooltipAttribute), false)).ToList();
				tooltips.Sort(ToolTipSorter.Instance);
				for (int i = 0; i < tooltips.Count; i++)
				{
					description += tooltips[i].tooltip;
					if (i != tooltips.Count - 1)
					{
						description += "\n";
					}
				}
			}
			InitializeValue();
			UpdateField();
		}

		protected abstract void InitializeValue();


		protected abstract void UpdateField();

		void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
		{
			WeaverConfigScreen.Instance.SettingDescription = description;
		}
	}
}
