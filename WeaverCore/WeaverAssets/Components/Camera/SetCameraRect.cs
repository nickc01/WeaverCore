using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeaverCore.Assets.Components
{
	/// <summary>
	/// Forces the RectTransform of a camera to be the same as it's orthographicSize
	/// </summary>
	[ExecuteAlways]
    public class SetCameraRect : MonoBehaviour
    {
        Camera cam;
		RectTransform rTransform;

		private void Awake()
		{
			cam = transform.GetComponent<Camera>();
			rTransform = GetComponent<RectTransform>();
		}

		private void Update()
		{
			var height = cam.orthographicSize * 2f;
			var width = (16f / 9f) * height;
			rTransform.sizeDelta = new Vector2(width, height);
		}
	}
}
