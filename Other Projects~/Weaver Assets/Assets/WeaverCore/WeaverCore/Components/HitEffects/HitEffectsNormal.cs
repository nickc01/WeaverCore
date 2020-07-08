using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore.Interfaces;
using WeaverCore.Enums;
using WeaverCore.DataTypes;

namespace WeaverCore.Components.HitEffects
{
	[RequireComponent(typeof(SpriteFlasher))]
	public class HitEffectsNormal : MonoBehaviour, IHitEffects
	{
		[SerializeField]
		bool doFlashEffects = true;
		bool firedOnCurrentFrame = false;
		SpriteFlasher flasher;

		static AudioClip DamageSound;

		void Start()
		{
			flasher = GetComponent<SpriteFlasher>();
			if (DamageSound == null)
			{
				DamageSound = WeaverAssets.AudioAssets.DamageEnemy;
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

				WeaverAudio.Play(DamageSound, transform.position, channel: AudioChannel.Sound);

				if (doFlashEffects)
				{
					flasher.FlashNormalHit();
				}

				GameObject hitParticles = Instantiate(WeaverAssets.EffectAssets.UninfectedHitPrefab, transform.position + effectsOffset, Quaternion.identity);

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

				Flings.SpawnFlingsNormal(transform.position + effectsOffset, direction);
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
