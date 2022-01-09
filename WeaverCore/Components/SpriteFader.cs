using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
	/// <summary>
	/// Used to fade a sprite from one color to another
	/// </summary>
	public class SpriteFader : MonoBehaviour
	{
		[Tooltip("Should this script start fading upon Awake?")]
		public bool FadeOnStart = false;
		[Tooltip("How long it should take to fade from one color to another")]
		public float Duration = 1f;
		[Tooltip("What should this script do when done fading?")]
		public OnDoneBehaviour DoneBehaviour;

		[SerializeField]
		[Tooltip("The color to fade from")]
		Color _fromColor;

		/// <summary>
		/// The color to fade from
		/// </summary>
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
		[Tooltip("The color to fade to")]
		Color _toColor;

		/// <summary>
		/// The color to fade to
		/// </summary>
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
				}
			}
		}

		/// <summary>
		/// Fades the sprite into a new color
		/// </summary>
		/// <param name="toColor">The color to fade to</param>
		/// <param name="duration">How long to should it take to fade? If set to -1, it will use the default <see cref="Duration"/> specified on the component</param>
		public void FadeToColor(Color toColor, float duration = -1f)
		{
			FadeToColor(Renderer.color, toColor, duration);
		}

		/// <summary>
		/// Fades the sprite from an old color to a new one
		/// </summary>
		/// <param name="fromColor">The color to fade from</param>
		/// <param name="toColor">The color to fade to</param>
		/// <param name="duration">How long to should it take to fade? If set to -1, it will use the default <see cref="Duration"/> specified on the component</param>
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

		/// <summary>
		/// Fades from a color to transparency
		/// </summary>
		/// <param name="fromColor">The color to fade from</param>
		/// <param name="duration">How long to should it take to fade? If set to -1, it will use the default <see cref="Duration"/> specified on the component</param>
		public void FadeToTransparency(Color fromColor, float duration = -1f)
		{
			FadeToColor(fromColor, default(Color), duration);
		}

		/// <summary>
		/// Fades the sprite into transparency
		/// </summary>
		/// <param name="duration">How long to should it take to fade? If set to -1, it will use the default <see cref="Duration"/> specified on the component</param>
		public void FadeToTransparency(float duration = -1f)
		{
			FadeToTransparency(Renderer.color,duration);
		}
	}
}
