using UnityEngine;
using WeaverCore.Attributes;

namespace WeaverCore.Features
{
	[ShowFeature]
	public class DebugCanvasExtension : MonoBehaviour
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


		public void AddToDebugCanvas()
		{
			GameObject.Instantiate(gameObject, WeaverDebugCanvas.Content);
		}
	}
}
