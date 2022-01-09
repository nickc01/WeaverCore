using System;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WeaverCore.Settings.Elements
{
	/// <summary>
	/// Used for displaying a field or property in the settings menu
	/// </summary>
	public abstract class UIElement : MonoBehaviour, IPointerEnterHandler
	{
		public event Action<IAccessor> DisplayValueUpdated;

		TextMeshProUGUI _titleText;

		/// <summary>
		/// The title component of the UI Element
		/// </summary>
		public TextMeshProUGUI TitleComponent
		{
			get
			{
				if (_titleText == null)
				{
					_titleText = GetComponentsInChildren<TextMeshProUGUI>().FirstOrDefault(tm => tm.gameObject.name.ToLower().Contains("title"));
				}
				return _titleText;
			}
			set
			{
				_titleText = value;
			}
		}

		/// <summary>
		/// The title of the UI Element
		/// </summary>
		public string Title
		{
			get
			{
				return TitleComponent.text;
			}

			set
			{
				TitleComponent.text = value;
			}
		}

		IAccessor _accessor;

		/// <summary>
		/// The accessor used for retrieving the value of the field or property
		/// </summary>
		public IAccessor FieldAccessor
		{
			get
			{
				return _accessor;
			}

			internal set
			{
				if (_accessor != value)
				{
					_accessor = value;
					OnAccessorChanged(value);
				}
			}
		}

		internal IAccessor FieldAccesorRaw
		{
			get => _accessor;
			set => _accessor = value;
		}

		internal void UpdateDisplayValue()
		{
			OnAccessorChanged(_accessor);
		}

		public GlobalSettings Panel { get; internal set; }

		/// <summary>
		/// Called whenever the accessor gets changed
		/// </summary>
		/// <param name="accessor">The new accessor</param>
		protected virtual void OnAccessorChanged(IAccessor accessor)
		{
			DisplayValueUpdated?.Invoke(accessor);
		}

		/// <summary>
		/// Whether or not this UI Element can work with the specified <paramref name="accessor"/>
		/// </summary>
		/// <param name="accessor">The accessor to check against</param>
		/// <returns></returns>
		public abstract bool CanWorkWithAccessor(IAccessor accessor);

		/// <summary>
		/// Called when the mouse hovers over the ui element. Default behaviour is to call <see cref="SettingsScreen.SetDescription(string)"/> to update the description
		/// </summary>
		/// <param name="eventData"></param>
		public virtual void OnPointerEnter(PointerEventData eventData)
		{
			if (FieldAccessor != null)
			{
				SettingsScreen.SetDescription(FieldAccessor.Description);
			}
		}

		/// <summary>
		/// Moves the UI Element up by one
		/// </summary>
		public void MoveUp()
		{
			var siblingIndex = transform.GetSiblingIndex();
			if (siblingIndex - 1 <= 0)
			{
				transform.SetAsFirstSibling();
			}
			else
			{
				transform.SetSiblingIndex(siblingIndex - 1);
			}
		}

		/// <summary>
		/// Moves the UI Element down by one
		/// </summary>
		public void MoveDown()
		{
			var siblingIndex = transform.GetSiblingIndex();
			if (siblingIndex + 1 >= transform.parent.childCount - 1)
			{
				transform.SetAsLastSibling();
			}
			else
			{
				transform.SetSiblingIndex(siblingIndex + 1);
			}
		}

		/// <summary>
		/// Moves the UI Element to the top of the settings menu
		/// </summary>
		public void MoveToTop()
		{
			transform.SetAsFirstSibling();
		}

		/// <summary>
		/// Moves the UI Element to the bottom of the settings menu
		/// </summary>
		public void MoveToBottom()
		{
			transform.SetAsLastSibling();
		}

		/// <summary>
		/// The order in which the UI element is arranged on the settings menu
		/// </summary>
		public int Order
		{
			get
			{
				return transform.GetSiblingIndex();
			}
			set
			{
				transform.SetSiblingIndex(value);
			}
		}

		/// <summary>
		/// Whether the UIElement is visible
		/// </summary>
		public bool Visible
		{
			get
			{
				return gameObject.activeSelf;
			}
			set
			{
				if (value != gameObject.activeSelf)
				{
					gameObject.SetActive(value);
					DisplayValueUpdated?.Invoke(FieldAccessor);
				}
			}
		}

		/// <summary>
		/// Refreshes the UI Element so it's displaying the most recent version of the field or property
		/// </summary>
		public void Refresh()
		{
			OnAccessorChanged(_accessor);
		}
	}
}
