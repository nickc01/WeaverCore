using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Components;

namespace WeaverCore.Assets.Components
{
	/// <summary>
	/// When a new scene is loaded, these are the black objects that are used to surround the scene at it's borders
	/// </summary>
	public class WeaverSceneBorder : MonoBehaviour
	{
		[OnInit]
		static void Init()
		{
			ModHooks.DrawBlackBordersHook += ModHooks_DrawBlackBordersHook;
		}

		private static void ModHooks_DrawBlackBordersHook(List<GameObject> obj)
		{
			if (GameManager.instance.sm is WeaverSceneManager wsm)
			{
				var posOffset = (Vector3)wsm.SceneDimensions.min;

				foreach (var gm in obj)
				{
					var border = gm.GetComponent<WeaverSceneBorder>();
					if (border != null)
					{
						border.transform.position += posOffset;
					}
				}
			}
		}
	}
}
