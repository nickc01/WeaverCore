using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WeaverCore.Assets.Components
{
	[ExecuteInEditMode]
	public class FitToText : MonoBehaviour
	{
		[SerializeField]
		bool resizeToWidth;

		[SerializeField]
		bool resizeToHeight;

		[SerializeField]
		float minWidth = 0f;

		[SerializeField]
		float minHeight = 0f;

		[SerializeField]
		float horizontalSpacing = 0f;
		[SerializeField]
		float verticalSpacing = 0f;


		Text text;
		RectTransform rect;



		void Start()
		{
			text = GetComponentInChildren<Text>();
			rect = GetComponent<RectTransform>();
		}





		void Update()
		{
			if (text != null && rect != null)
			{
				var newSize = rect.sizeDelta;
				if (resizeToWidth)
				{
					newSize.x = text.preferredWidth + horizontalSpacing;
					if (newSize.x < minWidth)
					{
						newSize.x = minWidth;
					}
				}
				if (resizeToHeight)
				{
					newSize.y = text.preferredHeight + verticalSpacing;
					if (newSize.y < minHeight)
					{
						newSize.y = minHeight;
					}
				}
				rect.sizeDelta = newSize;
			}
		}
	}

}
