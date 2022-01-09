using System.Reflection;
using UnityEngine.UI;
using WeaverCore.Utilities;

namespace WeaverCore.Settings.Elements
{
	/// <summary>
	/// A UI Element with a toggle between true and false
	/// </summary>
	public class BoolElement : UIElement
	{
		Toggle toggle;

		void Awake()
		{
			toggle = GetComponentInChildren<Toggle>();
			toggle.onValueChanged.AddListener(OnValueChanged);
		}

		/// <inheritdoc/>
		public override bool CanWorkWithAccessor(IAccessor accessor)
		{
			return accessor.MemberType == typeof(bool);
		}

		/// <inheritdoc/>
		protected override void OnAccessorChanged(IAccessor accessor)
		{
			toggle.isOn = (bool)accessor.FieldValue;
			Title = accessor.FieldName;
			base.OnAccessorChanged(accessor);
		}

		void OnValueChanged(bool newValue)
		{
			FieldAccessor.FieldValue = newValue;
		}
	}
}
