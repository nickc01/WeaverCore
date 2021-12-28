using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore.Assets.Components
{
	/// <summary>
	/// A particle system that is created when an uninfected enemy is hit
	/// </summary>
    public class UninfectedHitParticles : MonoBehaviour
	{
		[SerializeField]
		bool hasPool = false;

		static PoolableObject poolComponent;

		void Start()
		{
			if (poolComponent == null)
			{
				poolComponent = GetComponent<PoolableObject>();
			}
			if (hasPool && poolComponent.SourcePool != null)
			{
				poolComponent.SourcePool.ReturnToPool(poolComponent,0.1f);
			}
			else
			{
				Destroy(gameObject, 0.1f);
			}
		}
	}
}
