using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;

namespace WeaverCore.Features
{
	/// <summary>
	/// When attached to an object and added to a registry, the object will be instantiated when the UI Canvas starts
	/// </summary>
	[ShowFeature]
	public class CanvasExtension : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("Should this object be instantiated as soon as the WeaverCanvas initializes?")]
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
