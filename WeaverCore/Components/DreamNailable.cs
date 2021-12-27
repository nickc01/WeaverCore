using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using WeaverCore.Attributes;

namespace WeaverCore.Components
{
    /// <summary>
    /// This component allows an action to be done when the player dreamnails an object
    /// </summary>
    public class DreamNailable : EnemyDreamnailReaction
	{
		[SerializeField]
		[Tooltip("An action to be executed when the player dreamnails this object")]
		UnityEvent DreamnailEvent;

		static Action<EnemyDreamnailReaction, GameObject> prefabSetter;

		GameObject prefab;

		/// <summary>
		/// An action to be executed when the player dreamnails this object
		/// </summary>
		public event UnityAction OnDreamnailEvent
		{
			add
			{
				DreamnailEvent.AddListener(value);
			}
			remove
			{
				DreamnailEvent.RemoveListener(value);
			}
		}

		[OnHarmonyPatch]
		static void Patch(HarmonyPatcher patcher)
		{
			var orig = typeof(EnemyDreamnailReaction).GetMethod("RecieveDreamImpact");
			var pre = typeof(DreamNailable).GetMethod("RecieveDreamImpact_Prefix", BindingFlags.Static | BindingFlags.NonPublic);
			patcher.Patch(orig, pre, null);
		}

		static bool RecieveDreamImpact_Prefix(EnemyDreamnailReaction __instance)
		{
			if (__instance is DreamNailable dn)
			{
				dn.DoDreamNailTrigger();
				return false;
			}
			else
			{
				return true;
			}
		}

		private void Awake()
		{
			StartCoroutine(SetLayer());
		}

		/// <summary>
		/// Makes sure this object is set to the correct layer for dreamnailing to work
		/// </summary>
		/// <returns></returns>
		IEnumerator SetLayer()
		{
			yield return null;
			gameObject.layer = LayerMask.NameToLayer("Interactive Object");
		}

		void DoDreamNailTrigger()
		{
			OnDreamnail();
			DreamnailEvent.Invoke();
		}

		/// <summary>
		/// Called when this object is dreamnailed
		/// </summary>
		public virtual void OnDreamnail()
		{
			
		}
	}
}
