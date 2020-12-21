using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
	public class DeathWave : MonoBehaviour
	{
		public float Speed = 1f;
		public float GrowthSpeed = 1f;

		PoolableObject poolable;
		SpriteRenderer spriteRenderer;
		float accel;
		float timer;

		void Start()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
			poolable = GetComponent<PoolableObject>();
			accel = Speed;
			timer = 0f;
		}

		void Update()
		{
			timer += Time.deltaTime * accel;
			float num = (1f + timer * 4f) * GrowthSpeed;
			base.transform.localScale = new Vector3(num, num, num);
			Color color = spriteRenderer.color;
			color.a = 1f - timer;
			this.spriteRenderer.color = color;
			if (timer > 1f)
			{
				if (poolable != null)
				{
					poolable.ReturnToPool();
				}
				else
				{
					Destroy(gameObject);
				}
			}
		}

		void FixedUpdate()
		{
			accel *= 0.95f;
			if (accel < 0.5f)
			{
				accel = 0.5f;
			}
		}
	}

}
