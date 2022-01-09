namespace WeaverCore.Settings.Elements
{
	/// <summary>
	/// A UI Element for inserting a big header between two UI Elements
	/// </summary>
	public class HeaderElement : UIElement
	{
		internal UIElement BoundToElement;

		private void Awake()
		{
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

		/// <inheritdoc/>
		public override bool CanWorkWithAccessor(IAccessor accessor)
		{
			return false;
		}
	}
}
