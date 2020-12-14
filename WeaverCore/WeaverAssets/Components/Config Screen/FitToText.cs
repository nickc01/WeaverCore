using System.Collections;
using System.Collections.Generic;
using TMPro;
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


		[SerializeField]
		Text text;
		[SerializeField]
		TextMeshProUGUI tmpro;
		RectTransform rect;



		void Start()
		{
			if (text != null)
			{
				text = GetComponentInChildren<Text>();
			}
			if (tmpro != null)
			{
				tmpro = GetComponentInChildren<TextMeshProUGUI>();
			}
			rect = GetComponent<RectTransform>();
		}






		void Update()
		{
			if (Application.isEditor && text == null)
			{
				text = GetComponentInChildren<Text>();
			}
			if (Application.isEditor && tmpro == null)
			{
				tmpro = GetComponentInChildren<TextMeshProUGUI>();
			}
			if (Application.isEditor && rect == null)
			{
				rect = GetComponent<RectTransform>();
			}
			if ((tmpro != null || text != null) && rect != null)
			{
				var newSize = rect.sizeDelta;
				if (resizeToWidth)
				{
					if (tmpro != null)
					{
						newSize.x = tmpro.preferredWidth + horizontalSpacing;
					}
					else
					{
						newSize.x = text.preferredWidth + horizontalSpacing;
					}
					if (newSize.x < minWidth)
					{
						newSize.x = minWidth;
					}
				}
				if (resizeToHeight)
				{
					if (tmpro != null)
					{
						newSize.y = tmpro.preferredHeight + verticalSpacing;
					}
					else
					{
						newSize.y = text.preferredHeight + verticalSpacing;
					}
					//newSize.y = text.preferredHeight + verticalSpacing;
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
