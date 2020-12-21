using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
	public class AutoFlip : MonoBehaviour
	{
		public enum FlipMode
		{
			None,
			ScaleBased,
			SpriteBased
		}

		public FlipMode flipMode = FlipMode.ScaleBased;
		bool flipLocalX = true;
		bool flipLocalY = false;
		bool flipLocalZ = false;

		public float XFlip
		{
			get
			{
				return flipLocalX ? -1 : 1;
			}
		}

		public float YFlip
		{
			get
			{
				return flipLocalY ? -1 : 1;
			}
		}

		public float ZFlip
		{
			get
			{
				return flipLocalZ ? -1 : 1;
			}
		}

		new SpriteRenderer parentRenderer;

		float originalScale;
		Vector3 originalLocalPosition;
		bool originalSpriteFlip;
		bool changed = false;

		SpriteRenderer _selfRenderer;
		public SpriteRenderer SelfRenderer
		{
			get
			{
				if (_selfRenderer == null)
				{
					_selfRenderer = GetComponent<SpriteRenderer>();
				}
				return _selfRenderer;
			}
		}

		void OnEnable()
		{
			ResetFlip();
			originalScale = transform.localScale.x;
			originalLocalPosition = transform.localPosition;
			if (SelfRenderer != null)
			{
				originalSpriteFlip = SelfRenderer.flipX;
			}
			if (parentRenderer == null)
			{
				if (transform.parent == null)
				{
					parentRenderer = GetComponentInParent<SpriteRenderer>();
				}
				else
				{
					parentRenderer = transform.parent.GetComponentInParent<SpriteRenderer>();
				}
			}

			switch (flipMode)
			{
				case FlipMode.ScaleBased:
					if (parentRenderer.flipX)
					{
						transform.SetXLocalScale(-transform.localScale.x);
					}
					else
					{
						transform.SetXLocalScale(transform.localScale.x);
					}
					break;
				case FlipMode.SpriteBased:
					if (SelfRenderer != null)
					{
						if (parentRenderer.flipX)
						{
							SelfRenderer.flipX = !originalSpriteFlip;
						}
						else
						{
							SelfRenderer.flipX = originalSpriteFlip;
						}
					}
					break;
			}
			if (parentRenderer.flipX)
			{
				transform.SetLocalPosition(XFlip * transform.localPosition.x, YFlip * transform.localPosition.y, ZFlip * transform.localPosition.z);
			}
			changed = true;
		}

		void OnDisable()
		{
			ResetFlip();
		}


		void ResetFlip()
		{
			if (changed)
			{
				changed = false;
				transform.SetXLocalScale(originalScale);
				//originalLocalPosition = transform.localPosition;
				transform.localPosition = originalLocalPosition;
				if (SelfRenderer != null)
				{
					SelfRenderer.flipX = originalSpriteFlip;
				}
			}
		}

	}
}
