using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
	public class SpriteFader : MonoBehaviour
	{
		public bool FadeOnStart = false;
		public float Duration = 1f;
		public OnDoneBehaviour DoneBehaviour;

		[SerializeField]
		Color _fromColor;
		public Color FromColor
		{
			get
			{
				return _fromColor;
			}
			set
			{
				_fromColor = value;
			}
		}
		[SerializeField]
		Color _toColor;
		public Color ToColor
		{
			get
			{
				return _toColor;
			}
			set
			{
				_toColor = value;
			}
		}

		bool fading = false;
		float currentFadeTime = 0f;

		SpriteRenderer _renderer;

		public SpriteRenderer Renderer
		{
			get
			{
				if (_renderer == null)
				{
					_renderer = GetComponent<SpriteRenderer>();
				}
				return _renderer;
			}
			set
			{
				_renderer = value;
			}
		}

		void Start()
		{
			fading = true;
		}

		void Update()
		{
			if (fading)
			{
				currentFadeTime += Time.deltaTime;
				Renderer.color = Color.Lerp(FromColor, ToColor, currentFadeTime / Duration);
				if (currentFadeTime >= Duration)
				{
					fading = false;
					Renderer.color = ToColor;
					DoneBehaviour.DoneWithObject(this);
					/*switch (DoneBehaviour)
					{
						case OnDoneBehaviour.Disable:
							gameObject.SetActive(false);
							break;
						case OnDoneBehaviour.DestroyOrPool:
							var poolableObject = GetComponent<PoolableObject>();
							if (poolableObject != null)
							{
								poolableObject.ReturnToPool();
								break;
							}
							else
							{
								goto case OnDoneBehaviour.Destroy;
							}
						case OnDoneBehaviour.Destroy:
							Destroy(gameObject);
							break;
					}*/
				}
			}
		}

		public void FadeToColor(Color toColor, float duration = -1f)
		{
			FadeToColor(Renderer.color, toColor, duration);
		}

		public void FadeToColor(Color fromColor, Color toColor, float duration = -1f)
		{
			FromColor = fromColor;
			ToColor = toColor;
			if (duration > 0)
			{
				Duration = duration;
			}
			currentFadeTime = 0f;
			fading = true;
		}

		public void FadeToTransparency(Color fromColor, float duration = -1f)
		{
			FadeToColor(fromColor, default(Color), duration);
		}

		public void FadeToTransparency(float duration = -1f)
		{
			FadeToTransparency(Renderer.color,duration);
		}
	}
}
