using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace WeaverCore.Assets.Components
{
	/// <summary>
	/// Forces the camera to be a certain aspect ratio
	/// </summary>
    [RequireComponent(typeof(Camera))]
    [ExecuteAlways]
    public class ForceAspectRatio : MonoBehaviour
    {
		private Camera cam;
		private Camera hudcam;
		private bool viewportChanged;
		private int lastX;
		private int lastY;
		private float scaleAdjust;

		private void Awake()
		{
			cam = GetComponent<Camera>();
		}

		private void Start()
		{
			hudcam = GameCameras.instance.hudCamera;
			AutoScaleViewport();
		}

		private void Update()
		{
			viewportChanged = false;
			if (lastX != Screen.width)
			{
				viewportChanged = true;
			}
			if (lastY != Screen.height)
			{
				viewportChanged = true;
			}
			if (viewportChanged)
			{
				AutoScaleViewport();
			}
			lastX = Screen.width;
			lastY = Screen.height;
		}

		public void SetOverscanViewport(float adjustment)
		{
			scaleAdjust = adjustment;
			AutoScaleViewport();
		}

		private void AutoScaleViewport()
		{
			float num = (float)Screen.width / Screen.height / 1.77777779f;
			float num2 = 1f + scaleAdjust;
			Rect rect = cam.rect;
			if (num < 1f)
			{
				rect.width = 1f * num2;
				rect.height = num * num2;
				float num4 = (rect.x = (1f - rect.width) / 2f);
				float num6 = (rect.y = (1f - rect.height) / 2f);
			}
			else
			{
				float num7 = 1f / num;
				rect.width = num7 * num2;
				rect.height = 1f * num2;
				float num9 = (rect.x = (1f - rect.width) / 2f);
				float num11 = (rect.y = (1f - rect.height) / 2f);
			}
			cam.rect = rect;

			if (hudcam != null)
			{
				hudcam.rect = rect;
			}
		}
	}

}