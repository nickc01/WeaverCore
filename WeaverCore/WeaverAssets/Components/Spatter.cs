using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
	/// <summary>
	/// An orange infectious blob object that will destroy itself when hitting a wall or the player
	/// </summary>
	public class Spatter : MonoBehaviour
	{
		public Sprite[] sprites;

		public float scaleModifierMin = 0.7f;
		public float scaleModifierMax = 1.3f;
		public float splashScaleMin = 1.5f;
		public float splashScaleMax = 2f;
		public float fps = 30f;

		public float idleTime = 3f;

		CircleCollider2D circleCollider;
		Rigidbody2D body;
		SpriteRenderer spriteRenderer;

		private float stretchFactor = 1.4f;
		private float stretchMinX = 0.6f;
		private float stretchMaxY = 1.75f;
		private float scaleModifier;
		private bool collided = false;


		[SerializeField]
		OnDoneBehaviour WhenDone;

		void Start()
		{
			gameObject.layer = LayerMask.NameToLayer("Terrain Detector");
			scaleModifier = UnityEngine.Random.Range(scaleModifierMin, scaleModifierMax);
			if (circleCollider == null)
			{
				circleCollider = GetComponent<CircleCollider2D>();
				body = GetComponent<Rigidbody2D>();
				spriteRenderer = GetComponent<SpriteRenderer>();
			}
			StartCoroutine(MainRoutine());
		}

		IEnumerator MainRoutine()
		{
			for (float idleTimer = 0; idleTimer < idleTime && !collided; idleTimer += Time.deltaTime)
			{
				FaceAngle();
				ProjectileSquash();
				yield return null;
			}

			if (!collided)
			{
				Impact();
			}

			float animTimer = 0f;
			int frame = 0;

			while (frame < sprites.GetLength(0))
			{
				animTimer += Time.deltaTime;
				if (animTimer >= (1f / fps))
				{
					animTimer -= (1f / fps);
					frame++;
					if (frame < sprites.GetLength(0))
					{
						spriteRenderer.sprite = sprites[frame];
					}
					else
					{
						WhenDone.DoneWithObject(this);
					}
				}
			}
		}

		void OnDisable()
		{
			StopAllCoroutines();
		}

		private void FaceAngle()
		{
			Vector2 velocity = body.velocity;
			float z = Mathf.Atan2(velocity.y, velocity.x) * 57.295776f;
			transform.localEulerAngles = new Vector3(0f, 0f, z);
		}

		private void ProjectileSquash()
		{
			float num = 1f - body.velocity.magnitude * stretchFactor * 0.01f;
			float num2 = 1f + body.velocity.magnitude * stretchFactor * 0.01f;
			if (num2 < stretchMinX)
			{
				num2 = stretchMinX;
			}
			if (num > stretchMaxY)
			{
				num = stretchMaxY;
			}
			num *= scaleModifier;
			num2 *= scaleModifier;
			transform.localScale = new Vector3(num2, num, transform.localScale.z);
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			collided = true;
			var normal = GetSafeContactNormal(collision);
			float x = normal.x;
			float y = normal.y;
			if (y == -1f)
			{
				transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 180f);
			}
			else if (y == 1f)
			{
				transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0f);
			}
			else if (x == 1f)
			{
				transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 270f);
			}
			else if (x == -1f)
			{
				transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 90f);
			}
			else
			{
				transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z + 90f);
			}
			Impact();
		}

		private void OnTriggerEnter2D()
		{
			collided = true;
			transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0f);
			transform.position = new Vector3(transform.position.x, transform.position.y + 0.65f, transform.position.z);
			Impact();
		}

		private void Impact()
		{
			float num = UnityEngine.Random.Range(splashScaleMin, splashScaleMax);
			transform.localScale = new Vector2(num, num);
			circleCollider.enabled = false;
			body.isKinematic = true;
			body.velocity = new Vector2(0f, 0f);
			spriteRenderer.sprite = sprites[1];
		}

		private static ContactPoint2D[] contactsBuffer = new ContactPoint2D[1];
		static Vector2 GetSafeContactNormal(Collision2D collision)
		{
			int contacts = collision.GetContacts(contactsBuffer);
			if (contacts >= 1)
			{
				ContactPoint2D contactPoint2D = contactsBuffer[0];
				return contactPoint2D.normal;
			}
			Vector2 b = collision.collider.transform.TransformPoint(collision.collider.offset);
			Vector2 a = collision.otherCollider.transform.TransformPoint(collision.otherCollider.offset);
			return (a - b).normalized;
		}
	}
}
