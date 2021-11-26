using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeaverCore.Assets.Components
{
    [ExecuteAlways]
    public class FixedAspectRatio : MonoBehaviour
    {
		public float wantedAspectRatio = 1.7777777777777777777777777777778f;

		RectTransform parent;
		RectTransform rt;

		private void Awake()
		{
			rt = GetComponent<RectTransform>();
			parent = transform.parent.GetComponent<RectTransform>();
		}

		private void LateUpdate()
		{
			//var oldAspect = 
			/*var size = parent.sizeDelta;

			var currentAspectRatio = size.x / size.y;

			float inset = 1.0f - currentAspectRatio / wantedAspectRatio;

			rt.sizeDelta
			cam.rect = new Rect(0.0f, inset / 2, 1.0f, 1.0f - inset);*/
		}
	}
}
