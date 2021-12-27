using System;
using System.Reflection;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace WeaverCore.Settings.Elements
{
	/// <summary>
	/// A UI Element that runs a function when it is clicked ohn
	/// </summary>
	public class FunctionElement : UIElement
	{
		public enum InvocationType
		{
			None,
			Method,
			Delegate,
			UnityEvent
		}

		Button button;
		TextMeshProUGUI text;

		MethodInfo method;
		Delegate del;
		UnityEvent uEvent;
		public InvocationType InvokeType { get; private set; }

		void Awake()
		{
			button = GetComponentInChildren<Button>();
			text = button.GetComponentInChildren<TextMeshProUGUI>();
			button.onClick.AddListener(OnClick);
		}

		/// <inheritdoc/>
		protected override void OnAccessorChanged(IAccessor accessor)
		{
			text.text = accessor.FieldName;
			InvokeType = InvocationType.None;
			if (accessor is MethodAccessor)
			{
				del = null;
				uEvent = null;
				method = ((MethodAccessor)accessor).Method;
				InvokeType = InvocationType.Method;
			}
			else if (accessor is FieldAccessor || accessor is PropertyAccessor || accessor is CustomPropertyAccessor_Base || accessor is CustomFunctionAccessor)
			{
				method = null;
				uEvent = null;
				del = null;
				var value = accessor.FieldValue;
				if (value is Delegate)
				{
					del = (Delegate)value;
					InvokeType = InvocationType.Delegate;
				}
				else if (value is UnityEvent)
				{
					uEvent = (UnityEvent)value;
					InvokeType = InvocationType.UnityEvent;
				}
			}
			base.OnAccessorChanged(accessor);
		}

		/// <inheritdoc/>
		public override bool CanWorkWithAccessor(IAccessor accessor)
		{
			if (accessor is MethodAccessor)
			{
				var method = ((MethodAccessor)accessor).Method;

				var parameters = method.GetParameters();

				if (parameters.GetLength(0) == 0)
				{
					return true;
				}
			}
			else if (accessor is FieldAccessor || accessor is PropertyAccessor || accessor is CustomPropertyAccessor_Base || accessor is CustomFunctionAccessor)
			{
				var value = accessor.FieldValue;
				if (value != null)
				{
					if (value is Delegate)
					{
						var del = (Delegate)value;
						if (del.Method.GetParameters().GetLength(0) == 0)
						{
							return true;
						}
					}
					else if (value is UnityEvent)
					{
						return true;
					}
				}
			}
			return false;
		}

		public void OnClick()
		{
			switch (InvokeType)
			{
				case InvocationType.Method:
					if (method.IsStatic)
					{
						method.Invoke(null, null);
					}
					else
					{
						method.Invoke(Panel, null);
					}
					break;
				case InvocationType.Delegate:
					del.DynamicInvoke(null);
					break;
				case InvocationType.UnityEvent:
					uEvent.Invoke();
					break;
			}
		}
	}
}
