using UnityEngine;
using WeaverCore.Attributes;

namespace WeaverCore.Features
{
	/// <summary>
	/// When attached to an object and added to a registry, the object will be instantiated when the UI Debug Canvas starts
	/// </summary>
	/// <remarks>
	/// The Debug Canvas is similar to the normal UI Canvas, but all UI Elements under it will be on top of everything else
	/// </remarks>
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
