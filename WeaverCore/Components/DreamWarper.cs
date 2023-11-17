using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore.Dreamnail;

namespace WeaverCore.Components
{
    /// <summary>
    /// When an object is dreamnailed with this component attached, this will cause the player to be warped to a new scene
    /// </summary>
	public class DreamWarper : DreamnailableObject
	{
		[SerializeField]
		[Tooltip("The scene to be transported to")]
		string destinationScene;

		[SerializeField]
		[Tooltip("The scene to return back to")]
		string returnScene;

		[SerializeField]
		[Tooltip("The TransitionPoint the player will spawn at when they go to the destination scene")]
		string transitionGateName = "door1";

		[SerializeField]
		[Tooltip("The delay before the player gets transported to the new scene")]
		float warpDelay = 1.75f;

		[SerializeField]
		[Tooltip("Are charms usable in the destination scene?")]
		bool canUseCharms = true;
		private void Reset()
		{
			returnScene = gameObject.scene.name;
			soulAmount = 0;
			flashWhenHit = false;
		}

		private void Awake()
		{
			if (TryGetComponent<DreamNailable>(out var dn))
			{
				Destroy(dn);
			}

			soulAmount = 0;
			recoilWhenHit = false;
			flashWhenHit = false;

			gameObject.layer = LayerMask.NameToLayer("Interactive Object");
		}

        protected override int OnDreamnailHit(Player player)
        {
			if (CanBeDreamnailed)
			{
                Warp.DoDreamnailWarp(transform.position, destinationScene, returnScene, transitionGateName, warpDelay, !canUseCharms);
            }
            return base.OnDreamnailHit(player);
        }
    }
}
