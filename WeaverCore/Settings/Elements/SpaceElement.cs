using UnityEngine;

namespace WeaverCore.Settings.Elements
{
	public class SpaceElement : UIElement
	{
		RectTransform rTransform;

		void Awake()
		{
			rTransform = GetComponent<RectTransform>();
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

		public override bool CanWorkWithAccessor(IAccessor accessor)
		{
			return false;
		}
	}
}
