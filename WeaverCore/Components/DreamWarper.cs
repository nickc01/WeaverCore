using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WeaverCore.Components
{

    /// <summary>
    /// When an object is dreamnailed with this component attached, this will cause the player to be warped to a new scene
    /// </summary>
    [RequireComponent(typeof(DreamNailable))]
	public class DreamWarper : MonoBehaviour
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

		DreamNailable dn;

		private void Reset()
		{
			returnScene = gameObject.scene.name;
		}

		private void Awake()
		{
			dn = GetComponent<DreamNailable>();
			dn.OnDreamnailEvent += Dn_OnDreamnailEvent;
		}

		private void Dn_OnDreamnailEvent()
		{
			Warp.DoDreamnailWarp(transform.position, destinationScene, returnScene, transitionGateName, warpDelay, !canUseCharms);
		}
	}
}
