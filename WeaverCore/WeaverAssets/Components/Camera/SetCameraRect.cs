using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeaverCore.Assets.Components
{


	[ExecuteAlways]
    public class SetCameraRect : MonoBehaviour
    {
        Camera cam;
		RectTransform rTransform;

		private void Awake()
		{
			Debug.Log("R_A");
			cam = transform.GetComponent<Camera>();
			Debug.Log("R_B");
			rTransform = GetComponent<RectTransform>();
			Debug.Log("R_C");
		}

		private void Update()
		{
			//transform.localPosition = new Vector3(0f,0f,transform.localPosition.z);
			var height = cam.orthographicSize * 2f;
			var width = (16f / 9f) * height;

			//transform.SetScaleY(height);
			//transform.SetScaleX(width);
			rTransform.sizeDelta = new Vector2(width, height);
			//rTransform.
		}
	}
}
