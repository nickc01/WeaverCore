using UnityEngine;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
	/// <summary>
	/// Represents a simple enemy projectile. By default, this script handles:
	/// 1. Spawning the projectile at a random scale between <see cref="scaleMin"/> and <see cref="scaleMax"/>
	/// 2. Squashing and stretching the sprite depending on how it's moving through the air
	/// 3. Handling collision with both the player and the environment
	/// </summary>
	[RequireComponent(typeof(Rigidbody2D))]
	public abstract class EnemyProjectile : MonoBehaviour, IOnPool
	{
		[SerializeField]
		[Tooltip("The minimum scale this object can be at")]
		float scaleMin = 0.8f;
		[SerializeField]
		[Tooltip("The maximum scale this object can be at")]
		float scaleMax = 0.8f;
		[SerializeField]
		[Tooltip("How much overall squash and stretch should be applied to the object when it's moving")]
		float stretchFactor = 1.4f;
		[SerializeField]
		[Tooltip("The minimum amount of squashing the projectile can take")]
		float minSquashAmount = 0.5f;
		[SerializeField]
		[Tooltip("The maximum amount of stretching the projectile can take")]
		float maxStretchAmount = 2f;

		float scaleAmount;
		Collider2D _collider;
		Rigidbody2D _rigidbody;

		bool originalCollisionState = false;

		float _scaleFactor = 1f;
		public float ScaleFactor
		{
			get { return _scaleFactor; }
			set { _scaleFactor = value; }
		}

		public Rigidbody2D Rigidbody
		{
			get
			{
				if (_rigidbody == null)
				{
					_rigidbody = GetComponent<Rigidbody2D>();
				}
				return _rigidbody;
			}
		}

		public Collider2D Collider
		{
			get
			{
				if (_collider == null)
				{
					_collider = GetComponent<Collider2D>();
				}
				return _collider;
			}
		}

		protected virtual void Awake()
		{
			scaleAmount = UnityEngine.Random.Range(scaleMin, scaleMax);
			Squash();
			if (Collider != null)
			{
				originalCollisionState = Collider.enabled;
			}
		}

		protected virtual void Update()
		{
			float rotation = Mathf.Atan2(Rigidbody.velocity.y, Rigidbody.velocity.x) * Mathf.Rad2Deg;
			transform.SetZRotation(rotation);
			Squash();
		}

		public void PointAwayFrom(Vector3 target)
		{
			var vectorAway = (transform.position - target).normalized;

			RotateOnCollide(vectorAway);
		}

		void RotateOnCollide(Vector2 normal)
		{
			if (normal.y >= 0.75f && Mathf.Abs(normal.x) < 0.5f)
			{
				transform.SetZRotation(0f);
			}
			else if (normal.y <= 0.75f && Mathf.Abs(normal.x) < 0.5f)
			{
				transform.SetZRotation(180f);
			}
			else if (normal.x >= 0.75f && Mathf.Abs(normal.y) < 0.5f)
			{
				transform.SetZRotation(270f);
			}
			else if (normal.x <= 0.75f && Mathf.Abs(normal.y) < 0.5f)
			{
				transform.SetZRotation(90f);
			}
		}

		void Squash()
		{
			var stretchY = 1f - Rigidbody.velocity.magnitude * stretchFactor * 0.01f;
			var stretchX = 1f + Rigidbody.velocity.magnitude * stretchFactor * 0.01f;
			if (stretchX < minSquashAmount)
			{
				stretchX = minSquashAmount;
			}
			if (stretchY > maxStretchAmount)
			{
				stretchY = maxStretchAmount;
			}
			stretchY *= scaleAmount;
			stretchX *= scaleAmount;
			transform.localScale = new Vector3(stretchX * ScaleFactor, stretchY * ScaleFactor, transform.localScale.z);
		}

		public virtual void OnPool()
		{
			Rigidbody.isKinematic = false;
			Rigidbody.velocity = Vector2.zero;
			Rigidbody.angularVelocity = 0f;
			if (Collider != null)
			{
				Collider.enabled = originalCollisionState;
			}
		}

		protected virtual void OnHit(GameObject collision)
		{
			Rigidbody.isKinematic = true;
			Rigidbody.velocity = Vector2.zero;
			Rigidbody.angularVelocity = 0f;
			if (Collider != null)
			{
				Collider.enabled = false;
			}
			PointAwayFrom(collision.transform.position);
		}

		public void ForceHit(GameObject collision)
		{
			OnHit(collision);
        }

		protected virtual void OnCollisionEnter2D(Collision2D collision)
		{
			OnHit(collision.gameObject);
		}

		protected virtual void OnCollisionStay2D(Collision2D collision)
		{
			OnHit(collision.gameObject);
		}

		protected virtual void OnTriggerEnter2D(Collider2D collision)
		{
			OnHit(collision.gameObject);
		}

		protected virtual void OnTriggerStay2D(Collider2D collision)
		{
			OnHit(collision.gameObject);
		}
	}
}
