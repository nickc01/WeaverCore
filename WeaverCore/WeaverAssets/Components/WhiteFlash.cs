using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore.Assets.Components
{
	/// <summary>
	/// A white flash effect that is spawned when an entity is teleported via the <see cref="WeaverCore.Utilities.Teleporter"/> functions
	/// </summary>
	public class WhiteFlash : MonoBehaviour
	{
		public float FlashIntensity = 0.5f;
		public float FadeInTime = 0f;
		public float StayTime = 0f;
		public float FadeOutTime = 0f;
		public Color FlashColor;


		SpriteRenderer sprite;

		void Awake()
		{
			sprite = GetComponent<SpriteRenderer>();
			sprite.color = default(Color);
		}

		void Start()
		{
			StartCoroutine(Fader());
		}

		IEnumerator Fader()
		{
			var flashColorAlpha = new Color(FlashColor.r,FlashColor.g,FlashColor.b,0.0f);

			float timer = 0f;
			while (timer < FadeInTime)
			{
				yield return null;
				timer += Time.deltaTime;
				if (timer > FadeInTime)
				{
					timer = FadeInTime;
				}
				sprite.color = Color.Lerp(flashColorAlpha, FlashColor, (timer / FadeInTime) * FlashIntensity);
			}

			sprite.color = Color.Lerp(flashColorAlpha, FlashColor, (timer / FadeInTime) * FlashIntensity);

			if (StayTime > 0f)
			{
				yield return new WaitForSeconds(StayTime);
			}

			timer = 0f;

			while (timer < FadeOutTime)
			{
				yield return null;
				timer += Time.deltaTime;
				if (timer > FadeOutTime)
				{
					timer = FadeOutTime;
				}
				sprite.color = Color.Lerp(flashColorAlpha, FlashColor, (1f - (timer / FadeOutTime)) * FlashIntensity);
			}

			sprite.color = flashColorAlpha;

			Destroy(gameObject);
		}
	}
}
