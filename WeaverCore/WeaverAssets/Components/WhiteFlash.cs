using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
	/// <summary>
	/// A white flash effect that is spawned when an entity is teleported via the <see cref="WeaverCore.Utilities.Teleporter"/> functions
	/// </summary>
	public class WhiteFlash : MonoBehaviour, IOnPool
	{
		static WhiteFlash _defaultPrefab;

		public static WhiteFlash DefaultPrefab
		{
			get
			{
				if (_defaultPrefab == null)
				{
					_defaultPrefab = WeaverAssets.LoadWeaverAsset<GameObject>("White Flash Default").GetComponent<WhiteFlash>();
				}
				return _defaultPrefab;
			}
		}

		public float FlashIntensity = 0.5f;
		public float FadeInTime = 0f;
		public float StayTime = 0f;
		public float FadeOutTime = 0f;
		public Color FlashColor;


        //SpriteRenderer sprite;
        [NonSerialized]
        SpriteRenderer _mainRenderer;
        public SpriteRenderer MainRenderer => _mainRenderer ??= GetComponent<SpriteRenderer>();

        public Color SpriteColor
		{
			get => MainRenderer.color;
			set => MainRenderer.color = value;
		}

		void Awake()
		{
            MainRenderer.color = default;
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
                MainRenderer.color = Color.Lerp(flashColorAlpha, FlashColor, (timer / FadeInTime) * FlashIntensity);
			}

            MainRenderer.color = Color.Lerp(flashColorAlpha, FlashColor, (timer / FadeInTime) * FlashIntensity);

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
                MainRenderer.color = Color.Lerp(flashColorAlpha, FlashColor, (1f - (timer / FadeOutTime)) * FlashIntensity);
			}

            MainRenderer.color = flashColorAlpha;

			Destroy(gameObject);
		}

		public static WhiteFlash Spawn(Vector3 position, WhiteFlash prefab = null)
		{
			if (prefab == null)
			{
				prefab = DefaultPrefab;
            }

			return Pooling.Instantiate(prefab, position, Quaternion.identity);
		}

        public virtual void OnPool()
        {
			transform.localScale = Vector3.one;
        }
    }
}
