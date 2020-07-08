using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Patches
{
	/*class CameraControllerPatch : IInit
	{
		bool Initialized = false;

		private void CameraController_LateUpdate(On.CameraController.orig_LateUpdate orig, CameraController self)
		{
			if (!Initialized)
			{
				if (_instance == null)
				{
					_instance = staticImpl.Create();
					_instance.Initialize();
				}
				Initialized = true;
			}

			orig(self);
		}

		void IInit.OnInit()
		{
			On.CameraController.LateUpdate += CameraController_LateUpdate;
		}
	}*/
}
