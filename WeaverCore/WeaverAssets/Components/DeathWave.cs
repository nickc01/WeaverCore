using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
	/// <summary>
	/// Used to play a wave effect upon death
	/// </summary>
    public class DeathWave : MonoBehaviour
	{
		static ObjectPool DeathWavePool;

		[Tooltip("Determines how long the death wave will last. The higher the number, the shorter it will live")]
		public float Speed = 1f;

		[Tooltip("How fast the death wave will expand in size")]
		public float GrowthSpeed = 1f;

		[Tooltip("A multiplier applied to the scale of the object")]
		public float SizeMultiplier = 1f;

		[SerializeField]
		[Tooltip("The behaviour when the death wave is finished")]
		OnDoneBehaviour doneBehaviour = OnDoneBehaviour.DestroyOrPool;

		PoolableObject poolable;
		SpriteRenderer spriteRenderer;
		float accel;
		float timer;

		public float TransparencyMultiplier { get; set; } = 1f;

		void Awake()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
			poolable = GetComponent<PoolableObject>();
			accel = Speed;
			timer = 0f;
			UpdateVisuals();
		}

		public void UpdateVisuals()
		{
			float num = (1f + timer * 4f) * GrowthSpeed;
			base.transform.localScale = new Vector3(num * SizeMultiplier, num * SizeMultiplier, num * SizeMultiplier);
			Color color = spriteRenderer.color;
			color.a = TransparencyMultiplier * (1f - timer);
			this.spriteRenderer.color = color;
		}

		void Update()
		{
			timer += Time.deltaTime * accel;
			UpdateVisuals();
			if (timer > 1f)
			{
				doneBehaviour.DoneWithObject(this);
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

		/// <summary>
		/// Spawns a death wave effect
		/// </summary>
		/// <param name="position">The position the death wave will spawn</param>
		/// <param name="sizeMultiplier">A multiplier applied to the scale of the object</param>
		/// <returns></returns>
		public static DeathWave Spawn(Vector3 position, float sizeMultiplier)
		{
			if (DeathWavePool == null)
			{
				DeathWavePool = ObjectPool.Create(WeaverAssets.LoadWeaverAsset<GameObject>("Death Wave Infected"));
			}
			var instance = DeathWavePool.Instantiate<DeathWave>(position, Quaternion.identity);
			instance.SizeMultiplier = sizeMultiplier;
			instance.TransparencyMultiplier = 1f;
			instance.UpdateVisuals();
			return instance;
		}
	}

}
