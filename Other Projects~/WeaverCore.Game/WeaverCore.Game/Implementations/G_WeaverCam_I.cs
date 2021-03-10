using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Game.Patches;

namespace WeaverCore.Game.Implementations
{
	/*public class G_WeaverCam_I : WeaverCam_I
	{
		public class G_Statics : Statics
		{
			public override WeaverCamera Create()
			{
				var cameraParent = GameObject.Find("CameraParent");
				if (cameraParent == null)
				{
					return null;
				}
				else
				{
					tk2dCamera camera = cameraParent.GetComponentInChildren<tk2dCamera>();
					if (camera == null)
					{
						return null;
					}
					else
					{
						var cam = camera.gameObject.GetComponent<WeaverCamera>();
						if (cam == null)
						{
							cam = camera.gameObject.AddComponent<WeaverCamera>();
						}
						return cam;
					}
				}
			}
		}

		[OnInit]
		static void Init()
		{
			On.tk2dCamera.Awake += Tk2dCamera_Awake;
		}

		private static void Tk2dCamera_Awake(On.tk2dCamera.orig_Awake orig, tk2dCamera self)
		{
			if (self.GetComponent<WeaverCamera>() == null)
			{
				self.gameObject.AddComponent<WeaverCamera>();
			}
			orig(self);
		}
	}*/
}
