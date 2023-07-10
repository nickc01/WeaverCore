using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Implementations;

namespace WeaverCore.Components
{
	/// <summary>
	/// This component causes the sprite to flash. This is also used by <see cref="EntityHealth"/> to flash the enemy upon hit
	/// </summary>
	[RequireComponent(typeof(SpriteRenderer))]
	public class SpriteFlasher : MonoBehaviour
	{
		static SpriteFlasher_I impl;

		static Material flasherMaterial;

		Material previousMaterial;
		MaterialPropertyBlock propertyBlock;

		[SerializeField]
		Color flashColor = Color.white;
		[SerializeField]
		[Range(0f,1f)]
		float flashIntensity;

		Coroutine currentFlashRoutine;
		bool ranOnce = false;

		public Material CustomFlasherMaterial;
		
		/// <summary>
		/// The color of the flash
		/// </summary>
		public Color FlashColor
		{
			get
			{
				return flashColor;
			}
			set
			{
				if (flashColor != value)
				{
					Start();
					flashColor = value;
					UpdateBlock();
				}
			}
		}

		/// <summary>
		/// How intense the flash is. 0 means no flash, while 1 means the sprite is fully flashing
		/// </summary>
		public float FlashIntensity
		{
			get { return flashIntensity; }
			set
			{
				value = Mathf.Clamp01(value);
				if (flashIntensity != value)
				{
					Start();
					flashIntensity = value;
					UpdateBlock();
				}
			}
		}

		new SpriteRenderer renderer;

		void Start()
		{
			if (impl == null)
			{
				impl = ImplFinder.GetImplementation<SpriteFlasher_I>();
            }

			if (!ranOnce)
			{
				ranOnce = true;

				UpdateRenderer();

				propertyBlock = new MaterialPropertyBlock();

				if (flasherMaterial == null)
				{
					flasherMaterial = Assets.MaterialAssets.SpriteFlash;
				}
				if (previousMaterial == null)
				{
					previousMaterial = renderer.material;
				}
				renderer.material = CustomFlasherMaterial == null ? flasherMaterial : CustomFlasherMaterial;

				impl.OnFlasherInit(this);
			}
		}

		void OnEnable()
		{
			UpdateRenderer();
			if (previousMaterial == null)
			{
				previousMaterial = renderer.material;
			}
			renderer.material = flasherMaterial;
		}

		void OnDisable()
		{
			if (currentFlashRoutine != null)
			{
				StopCoroutine(currentFlashRoutine);
				currentFlashRoutine = null;
			}
			FlashIntensity = 0.0f;
			if (renderer != null)
			{
				renderer.material = previousMaterial;
			}
		}


		void UpdateRenderer()
		{
			renderer = GetComponentInChildren<SpriteRenderer>();
			if (renderer == null)
			{
				throw new Exception("The GameObject " + gameObject.name + " does not have a SpriteRenderer Component");
			}
		}

		void UpdateBlock()
		{
			if (renderer != null)
			{
				renderer.GetPropertyBlock(propertyBlock);

				propertyBlock.SetColor("_FlashColor", flashColor);
				propertyBlock.SetFloat("_FlashAmount", flashIntensity);

				renderer.SetPropertyBlock(propertyBlock);
			}
		}

		public void StopFlashing()
		{
			if (currentFlashRoutine != null)
			{
				StopCoroutine(currentFlashRoutine);
				currentFlashRoutine = null;
			}
		}

		/// <summary>
		/// Causes the sprite to flash
		/// </summary>
		/// <param name="BeginTime">How long it takes to go from no flash to full flash</param>
		/// <param name="EndTime">How long it takes to go from full flash to no flash</param>
		/// <param name="Intensity">The maximum intensity of the flash</param>
		/// <param name="StayTime">How long the sprite should stay at full flash before fading out</param>
		public void DoFlash(float BeginTime, float EndTime, float Intensity = 0.8f, float StayTime = 0.05f)
		{
			DoFlash(BeginTime, EndTime, Intensity, FlashColor, StayTime);
		}


		/// <summary>
		/// Causes the sprite to flash
		/// </summary>
		/// <param name="BeginTime">How long it takes to go from no flash to full flash</param>
		/// <param name="EndTime">How long it takes to go from full flash to no flash</param>
		/// <param name="Intensity">The maximum intensity of the flash</param>
		/// <param name="FlashColor">The color of the flash. If left null, will use the default color (white)</param>
		/// <param name="StayTime">How long the sprite should stay at full flash before fading out</param>
		public void DoFlash(float BeginTime, float EndTime, float Intensity = 0.8f, Color? FlashColor = null, float StayTime = 0.05f)
		{
			Start();
			if (currentFlashRoutine != null)
			{
				StopCoroutine(currentFlashRoutine);
				currentFlashRoutine = null;
			}
			if (FlashColor == null)
			{
				this.FlashColor = Color.white;
			}
			else
			{
				this.FlashColor = FlashColor.Value;
			}

			Coroutine routine = null;

			FlashIntensity = Mathf.Lerp(0.0f, Intensity, Time.deltaTime / BeginTime);

			routine = StartCoroutine(FlashRoutine(BeginTime, EndTime, Intensity, StayTime, routine));

			currentFlashRoutine = routine;
		}

		private IEnumerator FlashRoutine(float BeginTime, float EndTime, float Intensity, float StayTime, Coroutine routine)
		{
			float clock = 0.0f;
			while (clock <= BeginTime)
			{
				yield return null;
				clock += Time.deltaTime;
				FlashIntensity = Mathf.Lerp(0.0f, Intensity, clock / BeginTime);
			}
			clock = 0.0f;
			while (clock <= StayTime)
			{
				yield return null;
				clock += Time.deltaTime;
			}
			clock = EndTime;
			while (clock >= 0f)
			{
				yield return null;
				clock -= Time.deltaTime;
				FlashIntensity = Mathf.Lerp(0.0f, Intensity, clock / EndTime);
			}
			FlashIntensity = 0.0f;
			if (currentFlashRoutine == routine)
			{
				currentFlashRoutine = null;
			}
		}


		public void FlashNormalHit() => flashFocusHeal();
		public void FlashingSuperDash() => DoFlash(0.1f, 0.1f, 0.7f, new Color(1f, 1f, 1f), 0.01f);
		public void FlashingGhostWounded() => DoFlash(0.5f, 0.5f, 0.7f, new Color(1f, 1f, 1f), 0.01f);
		public void FlashingWhiteStay() => DoFlash(0.01f, 0.01f, 0.6f, new Color(1f, 1f, 1f), 999f);
		public void FlashingWhiteStayMoth() => DoFlash(2f, 2f, 0.6f, new Color(1f, 1f, 1f), 9999f);
		public void FlashingFury() => DoFlash(0.25f, 0.25f, 0.75f, new Color(0.71f, 0.18f, 0.18f), 0.01f);
		public void FlashingOrange() => DoFlash(0.1f, 0.1f, 0.7f, new Color(1f, 0.31f, 0f), 0.01f);
		public void flashInfected() => DoFlash(0.01f, 0.25f, 0.9f, new Color(1f, 0.31f, 0f), 0.01f);
		public void flashDung() => DoFlash(0.01f, 0.25f, 0.9f, new Color(0.45f, 0.27f, 0f), 0.01f);
		public void flashDungQuick() => DoFlash(0.001f, 0.1f, 0.75f, new Color(0.45f, 0.27f, 0f), 0.05f);
		public void flashSporeQuick() => DoFlash(0.001f, 0.1f, 0.75f, new Color(0.95f, 0.9f, 0.15f), 0.05f);
		public void flashWhiteQuick() => DoFlash(0.001f, 0.001f, 1f, new Color(1f, 1f, 1f), 0.05f);
		public void flashInfectedLong() => DoFlash(0.01f, 0.35f, 0.9f, new Color(1f, 0.31f, 0f), 0.25f);
		public void flashArmoured() => DoFlash(0.01f, 0.25f, 0.9f, new Color(1f, 1f, 1f), 0.01f);
		public void flashBenchRest() => DoFlash(0.01f, 0.75f, 0.7f, new Color(1f, 1f, 1f), 0.1f);
		public void flashDreamImpact() => DoFlash(0.01f, 0.75f, 0.9f, new Color(1f, 1f, 1f), 0.25f);
		public void flashMothDepart() => DoFlash(1.9f, 0.25f, 0.75f, new Color(1f, 1f, 1f), 1f);
		public void flashSoulGet() => DoFlash(0.01f, 0.25f, 0.5f, new Color(1f, 1f, 1f), 0.01f);
		public void flashShadeGet() => DoFlash(0.01f, 0.25f, 0.5f, new Color(0f, 0f, 0f), 0.01f);
		public void flashWhiteLong() => DoFlash(0.01f, 2f, 1f, new Color(1f, 1f, 1f), 0.01f);
		public void flashOvercharmed() => DoFlash(0.2f, 0.5f, 0.75f, new Color(0.72f, 0.376f, 0.72f), 0.01f);
		public void flashFocusHeal() => DoFlash(0.01f, 0.35f, 0.85f, new Color(1f, 1f, 1f), 0.01f);
		public void flashFocusGet() => DoFlash(0.01f, 0.35f, 0.5f, new Color(1f, 1f, 1f), 0.01f);
		public void flashWhitePulse() => DoFlash(0.5f, 0.75f, 0.35f, new Color(1f, 1f, 1f), 0.01f);
		public void flashHealBlue() => DoFlash(0.01f, 0.5f, 0.75f, new Color(0f, 0.584f, 1f), 0.01f);
		public void FlashShadowRecharge() => DoFlash(0.01f, 0.35f, 0.75f, new Color(0f, 0f, 0f), 0.01f);
		public void flashInfectedLoop() => DoFlash(0.2f, 0.2f, 0.9f, new Color(1f, 0.31f, 0f), 0.01f);
		public void FlashGrimmflame() => DoFlash(0.01f, 1f, 0.75f, new Color(1f, 0.25f, 0.25f), 0.01f);
		public void FlashGrimmHit() => DoFlash(0.01f, 0.25f, 0.75f, new Color(1f, 0.25f, 0.25f), 0.01f);
	}
}
