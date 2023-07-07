using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore.Interfaces;
using WeaverCore.Enums;

namespace WeaverCore.Components.HitEffects
{

    /// <summary>
    /// The hit effects for non-infected enemies
    /// </summary>
    public class HitEffectsNormal : MonoBehaviour, IHitEffects
	{
		static ObjectPool UninfectedHitPool;

		[SerializeField]
		[Tooltip("Should the sprite flash upon hit?")]
		bool doFlashEffects = true;
		bool firedOnCurrentFrame = false;
		//SpriteFlasher flasher;
		SpriteFlasher[] flashers;

		[SerializeField]
		[Tooltip("The sound that is played when hit")]
		AudioClip damageSound;

		public AudioClip DamageSound
        {
			get => damageSound;
			protected set => damageSound = value;
        }


		FlingInfo[] NormalFlings;

		void Start()
		{
			NormalFlings = Flings.CreateNormalFlings();
			if (UninfectedHitPool == null)
			{
				UninfectedHitPool = ObjectPool.Create(Assets.EffectAssets.UninfectedHitPrefab);
			}

			flashers = GetComponentsInChildren<SpriteFlasher>();
		}

        private void Reset()
        {
			damageSound = Assets.AudioAssets.DamageEnemy;

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

				WeaverAudio.PlayAtPoint(DamageSound, transform.position, channel: AudioChannel.Sound);

				if (doFlashEffects)
				{
                    foreach (var flasher in flashers)
                    {
						flasher.FlashNormalHit();
					}
				}

				GameObject hitParticles = Instantiate(Assets.EffectAssets.UninfectedHitPrefab, transform.position + effectsOffset, Quaternion.identity);

				var direction = DirectionUtilities.DegreesToDirection(hit.Direction);

				switch (direction)
				{
					case CardinalDirection.Up:
						hitParticles.transform.SetRotation2D(45f);
						break;
					case CardinalDirection.Down:
						hitParticles.transform.SetRotation2D(225f);
						break;
					case CardinalDirection.Left:
						hitParticles.transform.SetRotation2D(-225f);
						break;
					case CardinalDirection.Right:
						hitParticles.transform.SetRotation2D(-45f);
						break;
				}

				Flings.SpawnFlings(NormalFlings, transform.position + effectsOffset, direction);
			}
		}
	}
}
