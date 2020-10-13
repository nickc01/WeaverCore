using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
	public class AnimationDestroyer : MonoBehaviour
	{
		[SerializeField]
		bool partOfPool = false;

		PoolableObject poolComponent;

		void Awake()
		{
			if (partOfPool && poolComponent == null)
			{
				poolComponent = GetComponent<PoolableObject>();
			}
		}

		public void Destroy()
		{
			if (partOfPool && poolComponent.SourcePool != null)
			{
				poolComponent.SourcePool.ReturnToPool(poolComponent);
			}
			else
			{
				Destroy(gameObject);
			}
		}

		void OnParticleSystemStopped()
		{
			Destroy();
		}
	}
}
