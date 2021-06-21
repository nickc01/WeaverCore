using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore.Components
{
	public class SpriteFlasher : MonoBehaviour
	{
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



		[Serializable]
		public class SpriteFlasherException : Exception
		{
			public SpriteFlasherException() { }
			public SpriteFlasherException(string message) : base(message) { }
			public SpriteFlasherException(string message, Exception inner) : base(message, inner) { }
			protected SpriteFlasherException(
			  System.Runtime.Serialization.SerializationInfo info,
			  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
		}

		new SpriteRenderer renderer;

		void Start()
		{
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
				throw new SpriteFlasherException("The GameObject " + gameObject.name + " does not have a SpriteRenderer Component");
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

		public void DoFlash(float BeginTime, float EndTime, float Intensity = 0.8f, float StayTime = 0.05f)
		{
			DoFlash(BeginTime, EndTime, Intensity, FlashColor, StayTime);
		}


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
			//FlashIntensity = 0.0f;
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
		/*{
			DoFlash(0.01f, 0.35f, 0.85f, Color.white, 0.01f);
		}*/

		/*public void FlashInfected()
		{
			DoFlash(0.01f, 0.25f, 0.9f, new Color(1f, 0.31f, 0f), 0.01f);
		}*/

		/*public void FlashDung()
		{
			this.DoFlash(0.001f, 0.1f, 0.75f, new Color?(new Color(0.45f, 0.27f, 0f)), 0.05f);
		}*/

		// Token: 0x06000068 RID: 104 RVA: 0x00003660 File Offset: 0x00001860
		/*public void FlashSpore()
		{
			this.DoFlash(0.001f, 0.1f, 0.75f, new Color?(new Color(0.95f, 0.9f, 0.15f)), 0.05f);
		}*/


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
