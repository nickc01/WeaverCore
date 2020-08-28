using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;

namespace WeaverCore.Features
{
	[ShowFeature]
	public class CanvasExtension : Feature
	{
		public bool AddedByDefault = true;


		public void AddToWeaverCanvas()
		{
			GameObject.Instantiate(gameObject, WeaverCanvas.Content);
		}
	}
}
