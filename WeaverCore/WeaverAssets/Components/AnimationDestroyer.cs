using System;
using System.Collections;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
	// Token: 0x020000D3 RID: 211
	public class AnimationDestroyer : MonoBehaviour
	{
		// Token: 0x06000475 RID: 1141 RVA: 0x0000F26E File Offset: 0x0000D46E
		private void OnEnable()
		{
			if (this.destroyAfterTime)
			{
				base.StartCoroutine(this.Waiter());
			}
		}

		// Token: 0x06000476 RID: 1142 RVA: 0x0000F288 File Offset: 0x0000D488
		private void OnDisable()
		{
			base.StopAllCoroutines();
		}

		// Token: 0x06000477 RID: 1143 RVA: 0x0000F290 File Offset: 0x0000D490
		private IEnumerator Waiter()
		{
			yield return new WaitForSeconds(this.lifeTime);
			this.Destroy();
			yield break;
		}

		// Token: 0x06000478 RID: 1144 RVA: 0x0000F2AC File Offset: 0x0000D4AC
		public void Destroy()
		{
			DestroyBehaviour.DoneWithObject(this);
			/*OnDoneBehaviour destroyBehaviour = this.DestroyBehaviour;
			if (destroyBehaviour != OnDoneBehaviour.Disable)
			{
				if (destroyBehaviour != OnDoneBehaviour.DestroyOrPool)
				{
					if (destroyBehaviour != OnDoneBehaviour.Destroy)
					{
						return;
					}
				}
				else
				{
					PoolableObject component = base.GetComponent<PoolableObject>();
					if (component != null)
					{
						component.ReturnToPool();
						return;
					}
				}
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else
			{
				base.gameObject.SetActive(false);
			}*/
		}

		// Token: 0x06000479 RID: 1145 RVA: 0x0000F31E File Offset: 0x0000D51E
		private void OnParticleSystemStopped()
		{
			this.Destroy();
		}

		// Token: 0x040002C6 RID: 710
		[SerializeField]
		private OnDoneBehaviour DestroyBehaviour = OnDoneBehaviour.DestroyOrPool;

		// Token: 0x040002C7 RID: 711
		[SerializeField]
		private bool destroyAfterTime;

		// Token: 0x040002C8 RID: 712
		[Tooltip("Used only if Destroy After Time is set to true")]
		[SerializeField]
		private float lifeTime;
	}
}
