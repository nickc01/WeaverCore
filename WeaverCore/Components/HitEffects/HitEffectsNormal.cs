using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore.Interfaces;
using WeaverCore.Enums;

namespace WeaverCore.Components.HitEffects
{


	[RequireComponent(typeof(SpriteFlasher))]
	public class HitEffectsNormal : MonoBehaviour, IHitEffects
	{
		static ObjectPool UninfectedHitPool;

		[SerializeField]
		bool doFlashEffects = true;
		bool firedOnCurrentFrame = false;
		SpriteFlasher flasher;

		static AudioClip DamageSound;

		FlingInfo[] NormalFlings;

		void Start()
		{
			NormalFlings = Flings.CreateNormalFlings();
			if (UninfectedHitPool == null)
			{
				UninfectedHitPool = ObjectPool.Create(Assets.EffectAssets.UninfectedHitPrefab);
			}


			flasher = GetComponent<SpriteFlasher>();
			if (DamageSound == null)
			{
				DamageSound = Assets.AudioAssets.DamageEnemy;
			}
		}

		void Update()
		{
			firedOnCurrentFrame = false;
		}

		public void PlayHitEffect(HitInfo hit, Vector3 effectsOffset = default(Vector3))
		{
			if (!firedOnCurrentFrame)
			{
				firedOnCurrentFrame = true;

				Audio.PlayAtPoint(DamageSound, transform.position, channel: AudioChannel.Sound);

				if (doFlashEffects)
				{
					flasher.FlashNormalHit();
				}

				GameObject hitParticles = Instantiate(Assets.EffectAssets.UninfectedHitPrefab, transform.position + effectsOffset, Quaternion.identity);
				//GameObject hitParticles = Pooling.Instantiate(Assets.EffectAssets.UninfectedHitPrefab, transform.position + effectsOffset, Quaternion.identity);

				var direction = DirectionUtilities.DegreesToDirection(hit.Direction);

				switch (direction)
				{
					case CardinalDirection.Up:
						SetRotation2D(hitParticles.transform, 45f);
						break;
					case CardinalDirection.Down:
						SetRotation2D(hitParticles.transform, 225f);
						break;
					case CardinalDirection.Left:
						SetRotation2D(hitParticles.transform, -225f);
						break;
					case CardinalDirection.Right:
						SetRotation2D(hitParticles.transform, -45f);
						break;
				}

				Flings.SpawnFlings(NormalFlings, transform.position + effectsOffset, direction);
			}
		}

		static void SetRotation2D(Transform t, float rotation)
		{
			Vector3 eulerAngles = t.eulerAngles;
			eulerAngles.z = rotation;
			t.eulerAngles = eulerAngles;
		}
	}
}
