using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Utilities;

namespace WeaverCore.Features
{
	public class CameraExtension : Feature
	{
		public WeaverCam Camera => WeaverCam.Instance;
		public CameraShaker Shaker => WeaverCam.Instance.Shaker;
	}
}
