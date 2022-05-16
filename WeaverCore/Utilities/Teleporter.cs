using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Assets;
using WeaverCore.Assets.Components;

namespace WeaverCore.Utilities
{
	/// <summary>
	/// Used for teleporting an object/enemy from one place to another (with audio and particle effects too!)
	/// </summary>
	public static class Teleporter
    {
        class SingleContainer<T> : IEnumerable<T>
        {
			public T Value { get; set; }

			public SingleContainer(T value)
            {
				Value = value;
            }

            public IEnumerator<T> GetEnumerator()
            {
				yield return Value;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
				yield return Value;
            }
        }



        static ObjectPool WhiteFlashPool;
		static ObjectPool GlowPool;
		static ObjectPool TeleLinePool;


		const float WARP_TIME = 20f / 60f;

		/// <summary>
		/// The type of teleport to be employed
		/// </summary>
		public enum TeleType
		{
			/// <summary>
			/// An instantaneous teleport
			/// </summary>
			Quick,
			/// <summary>
			/// A teleport with a slight delay
			/// </summary>
			Delayed
		}

		static void LookAt(GameObject source, Vector3 destination)
		{
			source.transform.eulerAngles = new Vector3(0f, 0f, Mathf.Atan2(destination.y - source.transform.position.y, destination.x - source.transform.position.x) * Mathf.Rad2Deg);
		}


		/// <summary>
		/// Teleports an entity to a specified destination
		/// </summary>
		/// <param name="entity">The entity to teleport</param>
		/// <param name="Destination">The destination of the entity</param>
		/// <param name="teleType">How fast the teleportation should take</param>
		/// <param name="teleportColor">The color of the teleportation effects</param>
		/// <param name="flashSprite">Whether the sprite on the entity should flash or not. This only works if the entity has a <see cref="SpriteRenderer"/> and a <see cref="WeaverCore.Components.SpriteFlasher"/> If a <see cref="WeaverCore.Components.SpriteFlasher"/> is not already on the entity, one will be created</param>
		/// <param name="playEffects">Whether the teleportation effects should be played.</param>
		/// <returns>Returns the amount of time the teleportation will take. You can use this if you want to wait until the teleportation is done</returns>
		public static float TeleportEntity(GameObject entity, Vector3 Destination, TeleType teleType = TeleType.Quick, Color teleportColor = default(Color), bool flashSprite = true, bool playEffects = true, float audioPitch = 1f)
		{
			float inTime = 0.0f;
			float outTime = 0.0f;

			switch (teleType)
			{
				case TeleType.Quick:
					inTime = 0.0f;
					outTime = 0.1f;
					break;
				case TeleType.Delayed:
					inTime = 0.05f;
					outTime = 0.1f;
					break;
				default:
					break;
			}
			return TeleportEntity(entity, Destination, inTime, outTime, teleportColor, flashSprite, playEffects,audioPitch);
		}

		/// <summary>
		/// Teleports an entity to a specified destination
		/// </summary>
		/// <param name="entity">The entity to teleport</param>
		/// <param name="Destination">The destination of the entity</param>
		/// <param name="teleInTime">How long the entity will wait before it will teleport</param>
		/// <param name="teleOutTime">How long the entity will wait after it has teleported</param>
		/// <param name="teleportColor">The color of the teleportation effects. If left at the default, the teleport color will be white</param>
		/// <param name="flashSprite">Whether the sprite on the entity should flash or not. This only works if the entity has a <see cref="SpriteRenderer"/> and a <see cref="WeaverCore.Components.SpriteFlasher"/> If a <see cref="WeaverCore.Components.SpriteFlasher"/> is not already on the entity, one will be created</param>
		/// <param name="playEffects">Whether the teleportation effects should be played.</param>
		/// <returns>Returns the amount of time the teleportation will take. You can use this if you want to wait until the teleportation is done</returns>
		public static float TeleportEntity(GameObject entity, Vector3 Destination, float teleInTime, float teleOutTime, Color teleportColor = default(Color), bool flashSprite = true, bool playEffects = true, float audioPitch = 1f)
		{
			if (teleportColor == default(Color))
			{
				teleportColor = Color.white;
			}

			if (teleInTime < 0f)
			{
				teleInTime = 0f;
			}

			if (teleOutTime < 0f)
			{
				teleOutTime = 0f;
			}

			var sprites = entity.GetComponentsInChildren<SpriteRenderer>();
			var flashers = entity.GetComponentsInChildren<SpriteFlasher>();

			if (flashSprite)
			{
				if (sprites == null || sprites.GetLength(0) == 0)
				{
					throw new Exception("The entity to be teleported does not have a SpriteRenderer. Either add a sprite renderer to the entity, or set flashSprite to false");
				}
			}

			UnboundCoroutine.Start(TeleportRoutine(entity, Destination, teleInTime, teleOutTime, teleportColor, flashSprite, playEffects, audioPitch, sprites, flashers));

			return teleInTime + teleOutTime;
		}

		private static IEnumerator TeleportRoutine(GameObject entity, Vector3 Destination, float teleInTime, float teleOutTime, Color teleportColor, bool flashSprite, bool playEffects, float audioPitch, IEnumerable<SpriteRenderer> sprites, IEnumerable<SpriteFlasher> flashers)
		{
			List<float> prevSpriteAlphas = new List<float>();
            foreach (var sprite in sprites)
            {
				prevSpriteAlphas.Add(sprite.color.a);
            }

			if (teleInTime == 0f && teleOutTime == 0f)
			{
				var originalPosition = entity.transform.position;
				entity.transform.position = Destination;

				if (playEffects)
				{
					SpawnTeleportGlow(Destination, teleportColor);

					SpawnTeleportLine(originalPosition, Destination, teleportColor);

					SpawnWhiteFlash(teleportColor, originalPosition);
				}

				PlayTeleportSound(Destination, audioPitch);
			}
			else
			{
				if (flashSprite)
				{
                    foreach (var flasher in flashers)
                    {
						flasher.DoFlash(teleInTime, 0.0f, 0.8f, teleportColor, 10f);
					}
				}
				WhiteFlash whiteFlash = null;

				if (playEffects)
				{
					whiteFlash = SpawnWhiteFlash(teleportColor, entity.transform.position);
					whiteFlash.transform.parent = entity.transform;
				}
				if (teleInTime > 0f)
				{
					yield return new WaitForSeconds(teleInTime);
				}
				if (playEffects)
				{
					whiteFlash.transform.parent = null;
					whiteFlash.transform.position = entity.transform.position;
				}
				if (teleOutTime == 0f)
				{
					entity.transform.position = Destination;
					if (flashSprite)
					{
                        foreach (var flasher in flashers)
                        {
							flasher.StopFlashing();
							flasher.FlashIntensity = 0f;
						}
					}

					var originalPosition = entity.transform.position;

					if (playEffects)
					{
						SpawnTeleportGlow(Destination, teleportColor);

						SpawnTeleportLine(originalPosition, Destination, teleportColor);
					}

					PlayTeleportSound(Destination, audioPitch);
				}
				else
				{
					if (flashSprite)
					{
                        foreach (var flasher in flashers)
                        {
							flasher.StopFlashing();
							flasher.FlashIntensity = 0.8f;
							flasher.FlashColor = teleportColor;
						}
					}

					float currentWarpTime = WARP_TIME;

					if (teleOutTime < WARP_TIME)
					{
						currentWarpTime = teleOutTime;
					}

					float fadeOutTime = currentWarpTime / 2f;

					float fadeOutTimer = 0f;

					var originalPosition = entity.transform.position;

					if (playEffects)
					{
						SpawnTeleportGlow(Destination, teleportColor);
						SpawnTeleportLine(originalPosition, Destination, teleportColor);
					}
					PlayTeleportSound(Destination, audioPitch);

					while (fadeOutTimer < fadeOutTime)
					{
						yield return null;
						fadeOutTimer += Time.deltaTime;
						if (fadeOutTimer > fadeOutTime)
						{
							fadeOutTimer = fadeOutTime;
						}
						if (playEffects)
						{
							int counter = 0;
                            foreach (var sprite in sprites)
                            {
								sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, Mathf.Lerp(prevSpriteAlphas[counter], 0f, fadeOutTimer / fadeOutTime));
								counter++;
							}
						}
					}

					entity.transform.position = Destination;

					float fadeInTime = currentWarpTime - fadeOutTime;

					float fadeInTimer = 0f;

					if (playEffects)
					{
                        foreach (var flasher in flashers)
                        {
							flasher.DoFlash(0.0f, teleOutTime, 0.8f, teleportColor, 0f);
						}
					}

					while (fadeInTimer < fadeInTime)
					{
						yield return null;
						fadeInTimer += Time.deltaTime;
						if (fadeInTimer > fadeInTime)
						{
							fadeInTimer = fadeInTime;
						}
						if (playEffects)
						{
							int counter = 0;
                            foreach (var sprite in sprites)
                            {
								sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, Mathf.Lerp(0f, prevSpriteAlphas[counter], fadeInTimer / fadeInTime));
								counter++;
							}
						}
					}
				}
			}
		}

		private static void PlayTeleportSound(Vector3 position, float audioPitch)
		{
			var teleportSound = WeaverAudio.PlayAtPoint(AudioAssets.Teleport, position, 1f, AudioChannel.Sound);

			teleportSound.AudioSource.pitch = audioPitch;
		}

		static WhiteFlash SpawnWhiteFlash(Color color, Vector3 originalPosition)
		{
			if (WhiteFlashPool == null)
			{
				WhiteFlashPool = ObjectPool.Create(EffectAssets.WhiteFlashPrefab);
			}
			var whiteFlash = WhiteFlashPool.Instantiate<WhiteFlash>(originalPosition, Quaternion.identity);
			whiteFlash.FadeInTime = 0f;
			whiteFlash.FlashColor = Color.Lerp(color, Color.white, 0.5f);
			whiteFlash.transform.localScale = Vector3.one * 2f;

			return whiteFlash;
		}

		static SpriteRenderer SpawnTeleportGlow(Vector3 spawnPoint, Color color)
		{
			if (GlowPool == null)
			{
				GlowPool = ObjectPool.Create(EffectAssets.TeleportGlowPrefab);
			}
			var sprite = GlowPool.Instantiate<SpriteRenderer>(new Vector3(spawnPoint.x, spawnPoint.y, spawnPoint.z - 0.1f), Quaternion.identity);
			sprite.color = Color.Lerp(color, Color.white, 0.5f);
			return sprite;
		}

		static ParticleSystem SpawnTeleportLine(Vector3 originalPosition, Vector3 destination, Color color)
		{
			if (TeleLinePool == null)
			{
				TeleLinePool = ObjectPool.Create(EffectAssets.TeleLinePrefab);
			}
			var teleLine = TeleLinePool.Instantiate(Vector3.Lerp(originalPosition, destination, 0.5f), Quaternion.identity);
			LookAt(teleLine, destination);
			teleLine.transform.localScale = new Vector3(Vector3.Distance(originalPosition, destination), teleLine.transform.localScale.y, teleLine.transform.localScale.z);

			var particleSystem = teleLine.GetComponent<ParticleSystem>();

			var mainModule = particleSystem.main;
			mainModule.startColor = Color.Lerp(color, Color.white, 0.5f);

			return particleSystem;
		}
	}
}
