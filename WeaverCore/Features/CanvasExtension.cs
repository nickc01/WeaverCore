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
		[SerializeField]
		bool addedOnStartup = false;
		public virtual bool AddedOnStartup
		{
			get
			{
				return addedOnStartup;
			}
		}


		public void AddToWeaverCanvas()
		{
			GameObject.Instantiate(gameObject, WeaverCanvas.Content);
		}
	}
}
