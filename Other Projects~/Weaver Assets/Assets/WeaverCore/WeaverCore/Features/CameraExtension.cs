using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Features
{
	[ShowFeature]
	public class CameraExtension : Feature
	{
		public WeaverCam Camera
		{
			get
			{
				return WeaverCam.Instance;
			}
		}
		public CameraShaker Shaker
		{
			get
			{
				return WeaverCam.Instance.Shaker;
			}
		}
	}
}
