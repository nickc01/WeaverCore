using UnityEngine;

namespace WeaverCore.Settings.Elements
{
	/// <summary>
	/// Used to add a space between two UI Elements
	/// </summary>
	public class SpaceElement : UIElement
	{
		internal UIElement BoundToElement;

		RectTransform rTransform;

		void Awake()
		{
			rTransform = GetComponent<RectTransform>();
			SettingsScreen.ElementRemoved += SettingsScreen_ElementRemoved;
		}

		private void Start()
		{
			if (BoundToElement != null)
			{
				BoundToElement.DisplayValueUpdated += BoundToElement_DisplayValueUpdated;
			}
		}

		private void BoundToElement_DisplayValueUpdated(IAccessor obj)
		{
			Visible = BoundToElement.Visible;
		}

		private void SettingsScreen_ElementRemoved(UIElement obj)
		{
			if (obj == this || obj == BoundToElement)
			{
				SettingsScreen.ElementRemoved -= SettingsScreen_ElementRemoved;
				if (obj == BoundToElement)
				{
					Panel.RemoveUIElement(this);
				}
			}
		}

		public float Spacing
		{
			get
			{
				return rTransform.sizeDelta.y;
			}
			set
			{
				rTransform.sizeDelta = new Vector2(rTransform.sizeDelta.x,Spacing);
			}
		}

		/// <inheritdoc/>
		public override bool CanWorkWithAccessor(IAccessor accessor)
		{
			return false;
		}
	}
}
