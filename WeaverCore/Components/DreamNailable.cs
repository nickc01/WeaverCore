using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
	public class DreamNailable : EnemyDreamnailReaction
	{
		class DreamNailExecuter : MonoBehaviour
		{
			[SerializeField]
			[HideInInspector]
			internal DreamNailable source;

			void Awake()
			{
				if (source != null)
				{
					source.DoDreamNailTrigger();
					Destroy(gameObject);
				}
			}
		}

		[SerializeField]
		UnityEvent DreamnailEvent;

		static Action<EnemyDreamnailReaction, GameObject> prefabSetter;

		GameObject prefab;

		private void Awake()
		{
			if (prefabSetter == null)
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
			prefabSetter(this, prefab);
		}

		void DoDreamNailTrigger()
		{
			OnDreamnail();
			DreamnailEvent.Invoke();
		}

		public virtual void OnDreamnail()
		{

		}
	}
}
