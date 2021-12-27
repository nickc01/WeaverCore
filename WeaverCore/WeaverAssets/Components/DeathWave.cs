using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{


    public class DeathWave : MonoBehaviour
	{
		static ObjectPool DeathWavePool;

		public float Speed = 1f;
		public float GrowthSpeed = 1f;

		public float SizeMultiplier = 1f;

		[SerializeField]
		OnDoneBehaviour doneBehaviour = OnDoneBehaviour.DestroyOrPool;

		PoolableObject poolable;
		SpriteRenderer spriteRenderer;
		float accel;
		float timer;

		void Awake()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
			poolable = GetComponent<PoolableObject>();
			accel = Speed;
			timer = 0f;
			UpdateVisuals();
		}

		void UpdateVisuals()
		{
			float num = (1f + timer * 4f) * GrowthSpeed;
			base.transform.localScale = new Vector3(num * SizeMultiplier, num * SizeMultiplier, num * SizeMultiplier);
			Color color = spriteRenderer.color;
			color.a = 1f - timer;
			this.spriteRenderer.color = color;
		}

		void Update()
		{
			timer += Time.deltaTime * accel;
			UpdateVisuals();
			if (timer > 1f)
			{
				doneBehaviour.DoneWithObject(this);
				/*if (poolable != null)
				{
					poolable.ReturnToPool();
				}
				else
				{
					Destroy(gameObject);
				}*/
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
				DeathWavePool = ObjectPool.Create(WeaverAssets.LoadWeaverAsset<GameObject>("Death Wave Infected"));
			}
			var instance = DeathWavePool.Instantiate<DeathWave>(position, Quaternion.identity);
			instance.SizeMultiplier = sizeMultiplier;
			instance.UpdateVisuals();
			return instance;
		}
	}

}
