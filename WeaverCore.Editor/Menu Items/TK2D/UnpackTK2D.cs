using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.Utilities;

namespace WeaverCore.Editor
{
	static class UnpackTK2DSpriteMap
	{
		[MenuItem("WeaverCore/Tools/Unpack Sprite Map")]
		static void UnpackSpriteMap()
		{
			UnpackTK2DWindow.OpenSprite();
		}

		[MenuItem("WeaverCore/Tools/Unpack Animation Map")]
		static void UnpackAnimationMap()
		{
			UnpackTK2DWindow.OpenAnim();
		}
	}
}