using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using WeaverCore.Features;
using WeaverCore.Interfaces;

namespace WeaverCore.Editor.Inits
{
	class CanvasInit : IRuntimeInit
	{
		public void RuntimeInit()
		{
			/*var canvas = GameObject.FindObjectOfType<Canvas>();
			if (canvas == null)
			{
				var canvasObject = new GameObject("Canvas");
				canvas = canvasObject.AddComponent<Canvas>();
				canvas.renderMode = RenderMode.ScreenSpaceOverlay;
				var scaler = canvasObject.AddComponent<CanvasScaler>();
				scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
			}

			foreach (var canvasExtension in Registry.GetAllFeatures<CanvasExtension>())
			{
				GameObject.Instantiate(canvasExtension.gameObject, canvas.transform);
			}*/
		}
	}
}
