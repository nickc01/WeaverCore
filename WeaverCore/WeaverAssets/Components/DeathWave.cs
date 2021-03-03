using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
	public class DeathWave : MonoBehaviour
	{
		static WeaverCore.Utilities.ObjectPool DeathWavePool;

		public float Speed = 1f;
		public float GrowthSpeed = 1f;

		public float SizeMultiplier = 1f;

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
			base.transform.localScale = new Vector3(num * SizeMultiplier, num * SizeMultiplier, num * SizeMultiplier);
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

		public static DeathWave Spawn(Vector3 position, float sizeMultiplier)
		{
			if (DeathWavePool == null)
			{
				DeathWavePool = new ObjectPool(WeaverAssets.LoadWeaverAsset<GameObject>("Death Wave Infected"));
			}
			var instance = DeathWavePool.Instantiate<DeathWave>(position, Quaternion.identity);
			instance.SizeMultiplier = sizeMultiplier;
			return instance;
		}
	}

}
