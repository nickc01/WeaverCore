using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Interfaces;

namespace WeaverCore.Game.Patches
{
	class HitTakerPatch : IPatch
	{
		public void Apply()
		{
			On.HitTaker.Hit += HitTaker_Hit;
		}

		private static void HitTaker_Hit(On.HitTaker.orig_Hit orig, UnityEngine.GameObject targetGameObject, HitInstance damageInstance, int recursionDepth)
		{
			HitInfo info = Misc.ConvertHitInstance(damageInstance);
			if (targetGameObject != null)
			{
				Transform transform = targetGameObject.transform;
				for (int i = 0; i < recursionDepth; i++)
				{
					IHittable hittable = transform.GetComponent<IHittable>();
					if (hittable != null)
					{
						hittable.Hit(info);
					}
					transform = transform.parent;
					if (transform == null)
					{
						break;
					}
				}
			}
			orig(targetGameObject, damageInstance, recursionDepth);
		}
	}
}
