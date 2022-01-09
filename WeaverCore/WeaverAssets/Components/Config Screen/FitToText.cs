using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace WeaverCore.Assets.Components
{
	/// <summary>
	/// Resizes the RectTransform of an object to always fit the text it contains
	/// </summary>
	[ExecuteInEditMode]
	public class FitToText : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("Should the RectTransform of the object resize its width to fit the text?")]
		bool resizeToWidth;

		[SerializeField]
		[Tooltip("Should the RectTransform of the object resize its height to fit the text?")]
		bool resizeToHeight;

		[SerializeField]
		[Tooltip("The minimum width the RectTransform can be at")]
		float minWidth = 0f;

		[SerializeField]
		[Tooltip("The minimum height the RectTransform can be at")]
		float minHeight = 0f;

		[SerializeField]
		[FormerlySerializedAs("horizontalSpacing")]
		[Tooltip("How much extra padding should be on the left and right sides of the RectTransform")]
		float horizontalPadding = 0f;
		[SerializeField]
		[FormerlySerializedAs("verticalSpacing")]
		[Tooltip("How much extra padding should be on the top and bottom sides of the RectTransform")]
		float verticalPadding = 0f;


		[SerializeField]
		[Tooltip("The text object to resize the RectTransform to")]
		Text text;
		[SerializeField]
		[Tooltip("The text mesh pro object to resize the RectTransform to")]
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
						newSize.x = tmpro.preferredWidth + horizontalPadding;
					}
					else
					{
						newSize.x = text.preferredWidth + horizontalPadding;
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
						newSize.y = tmpro.preferredHeight + verticalPadding;
					}
					else
					{
						newSize.y = text.preferredHeight + verticalPadding;
					}
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
