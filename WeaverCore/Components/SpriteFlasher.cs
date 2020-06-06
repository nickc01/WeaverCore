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
		

		public Color FlashColor
		{
			get => flashColor;
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
			get => flashIntensity;
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
					flasherMaterial = WeaverAssets.MaterialAssets.SpriteFlash;
				}
				if (previousMaterial == null)
				{
					previousMaterial = renderer.material;
				}
				renderer.material = flasherMaterial;
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
			renderer = GetComponent<SpriteRenderer>();
			if (renderer == null)
			{
				throw new SpriteFlasherException($"The GameObject {gameObject.name} does not have a SpriteRenderer Component");
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


		public void DoFlash(float BeginTime, float EndTime, float Intensity = 0.8f, Color? FlashColor = default, float StayTime = 0.05f)
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

			IEnumerator FlashRoutine()
			{
				FlashIntensity = 0.0f;
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

			routine = StartCoroutine(FlashRoutine());
		}


		public void FlashNormalHit() => DoFlash(0.01f, 0.35f, 0.85f, Color.white, 0.01f);
	}
}
