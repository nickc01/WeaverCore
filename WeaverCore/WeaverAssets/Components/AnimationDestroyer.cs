using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
	public class AnimationDestroyer : MonoBehaviour
	{
		[SerializeField]
		OnDoneBehaviour DestroyBehaviour = OnDoneBehaviour.DestroyOrPool;

		public void Destroy()
		{
			switch (DestroyBehaviour)
			{
				case OnDoneBehaviour.Disable:
					gameObject.SetActive(false);
					break;
				case OnDoneBehaviour.DestroyOrPool:
					var poolableObject = GetComponent<PoolableObject>();
					if (poolableObject != null)
					{
						poolableObject.ReturnToPool();
						break;
					}
					else
					{
						goto case OnDoneBehaviour.Destroy;
					}
				case OnDoneBehaviour.Destroy:
					Destroy(gameObject);
					break;
			}
		}

		void OnParticleSystemStopped()
		{
			Destroy();
		}
	}
}
