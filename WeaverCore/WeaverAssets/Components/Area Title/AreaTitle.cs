using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WeaverCore.Assets
{
	public class AreaTitle : MonoBehaviour
	{
		public enum TitlePosition
		{
			BottomLeft,
			BottomCenter,
			BottomRight,
			MiddleLeft,
			MiddleCenter,
			MiddleRight,
			TopLeft,
			TopCenter,
			TopRight
		}


		[SerializeField]
		RectTransform TextArea;

		[SerializeField]
		Text BottomText;

		[SerializeField]
		Text TopText;

		TitlePosition _position = TitlePosition.BottomLeft;
		public TitlePosition Position
		{
			get
			{
				return _position;
			}
			set
			{

			}
		}

		void Awake()
		{
			BottomText.gameObject.SetActive(false);
			TopText.gameObject.SetActive(false);
		}
	}
}
