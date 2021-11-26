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
using WeaverCore.Utilities;

namespace WeaverCore.Components
{

	public class DreamNailable : EnemyDreamnailReaction
	{
		/*class DreamNailExecuter : MonoBehaviour
		{
			[SerializeField]
			[HideInInspector]
			internal DreamNailable source;

			void Awake()
			{
				WeaverLog.Log("DREAM NAILED EXECUTER");
				if (source != null)
				{
					source.DoDreamNailTrigger();
					Destroy(gameObject);
				}
			}
		}*/

		[SerializeField]
		UnityEvent DreamnailEvent;

		static Action<EnemyDreamnailReaction, GameObject> prefabSetter;

		GameObject prefab;

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
			WeaverLog.Log("OBJECT DREAMNAILED");
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
			WeaverLog.Log("DREAM NAILABLE AWOKEN");
			StartCoroutine(SetLayer());
			/*if (prefabSetter == null)
			{
				prefabSetter = ReflectionUtilities.CreateFieldSetter<EnemyDreamnailReaction, GameObject>("dreamImpactPrefab");
			}

			prefab = new GameObject("EXECUTER");
			prefab.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInHierarchy;
			prefab.transform.SetParent(transform);
			prefab.transform.localPosition = default;
			prefab.transform.localRotation = Quaternion.identity;
			var executer = prefab.AddComponent<DreamNailExecuter>();
			executer.source = this;
			prefabSetter(this, prefab);*/
		}

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

		public virtual void OnDreamnail()
		{
			WeaverLog.Log("DREAMNAILED OBJECT = " + gameObject.name);
		}
	}
}
