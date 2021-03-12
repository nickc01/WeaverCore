using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

/*namespace WeaverCore.Components
{

	public class EnemyProjectile : MonoBehaviour, IOnPool
	{
		[NonSerialized]
		new Rigidbody2D rigidbody;
		[NonSerialized]
		new Collider2D collider;
		[NonSerialized]
		WeaverAnimationPlayer animator;


		[SerializeField]
		float scaleMin = 1f;
		[SerializeField]
		float scaleMax = 1f;
		[SerializeField]
		float stretchFactor = 1.4f;
		[SerializeField]
		float minSquashAmount = 1f;
		[SerializeField]
		float maxStretchAmount = 2f;
		[SerializeField]
		[Tooltip("Causes the projectile to die after a set amount of time")]
		bool destroyAfterTime = true;
		[SerializeField]
		[Tooltip("If Destroy After Time is set to true, this determines how long the projectile will live")]
		float lifetime = 5f;
		[SerializeField]
		[Tooltip("Determines what the projectile will do when it is destroyed")]
		OnDoneBehaviour whenDestroyed;


		[Space]
		[Header("Visuals")]
		[SerializeField]
		[Tooltip("The animation to play when the projectile spawns. Leave this blank if no animation should play")]
		string animationOnStart;

		[SerializeField]
		[Tooltip("The animation to play when the projetile is destroyed. Leave this blank if no animation should play")]
		string animationOnDestroy;

		[SerializeField]
		[Tooltip("The sound that is played when the projectile collides with something, leave blank for no sound to play")]
		AudioClip impactClip;

		float scaleAmount;
		bool projectileActive = false;

		protected virtual void Awake()
		{
			if (rigidbody == null)
			{
				rigidbody = GetComponent<Rigidbody2D>();

				collider = GetComponent<Collider2D>();
			}
			if (animator == null)
			{
				animator = GetComponent<WeaverAnimationPlayer>();
			}
			projectileActive = true;
			rigidbody.isKinematic = false;
			rigidbody.velocity = Vector3.zero;
			rigidbody.angularVelocity = 0f;
		}

		protected virtual void OnEnable()
		{
			scaleAmount = UnityEngine.Random.Range(scaleMin, scaleMax);
			if (destroyAfterTime)
			{
				StartCoroutine(LifetimeWaiter(lifetime));
			}
			if (!string.IsNullOrEmpty(animationOnStart))
			{
				if (animator == null)
				{
					throw new System.Exception("There is no animator on the object " + gameObject.name);
				}
				animator.PlayAnimation(animationOnStart);
			}
			OnProjectileSpawn();
		}

		protected virtual void OnDisable()
		{
			StopAllCoroutines();
		}

		IEnumerator LifetimeWaiter(float time)
		{
			yield return new WaitForSeconds(time);
			Destroy();
		}

		protected virtual void Update()
		{
			if (projectileActive)
			{
				float rotation = Mathf.Atan2(rigidbody.velocity.y, rigidbody.velocity.x) * Mathf.Rad2Deg;
				transform.SetZRotation(rotation);
				Squash();
			}
		}

		public virtual void OnPool()
		{
			if (collider != null)
			{
				collider.enabled = true;
			}
		}

		void Squash()
		{
			var stretchY = 1f - rigidbody.velocity.magnitude * stretchFactor * 0.01f;
			var stretchX = 1f + rigidbody.velocity.magnitude * stretchFactor * 0.01f;
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
			transform.localScale = new Vector3(stretchX, stretchY, transform.localScale.z);
		}


		public void Destroy()
		{
			if (projectileActive)
			{
				projectileActive = false;
				StartCoroutine(Destroyer());
			}

		}

		IEnumerator Destroyer()
		{
			transform.localScale = new Vector3(scaleAmount,scaleAmount,transform.localScale.z);
			projectileActive = false;
			rigidbody.isKinematic = true;
			rigidbody.velocity = Vector2.zero;
			rigidbody.angularVelocity = 0f;
			if (collider != null)
			{
				collider.enabled = false;
			}

			if (!string.IsNullOrEmpty(animationOnDestroy))
			{
				if (animator == null)
				{
					throw new System.Exception("There is no animator on the object " + gameObject.name);
				}
				animator.PlayAnimation(animationOnDestroy);
				OnProjectileDestroy();
				yield return animator.WaitforClipToFinish();
			}
			else
			{
				OnProjectileDestroy();
			}
			switch (whenDestroyed)
			{
				case OnDoneBehaviour.Nothing:
					break;
				case OnDoneBehaviour.Disable:
					gameObject.SetActive(false);
					break;
				case OnDoneBehaviour.Destroy:
					Destroy(gameObject);
					break;
				case OnDoneBehaviour.DestroyOrPool:
					var pool = GetComponent<PoolableObject>();
					if (pool == null)
					{
						Destroy(gameObject);
					}
					else
					{
						pool.ReturnToPool();
					}
					break;
			}
		}

		protected virtual void OnCollisionEnter2D(Collision2D collision)
		{
			if (projectileActive)
			{
				var contacts = collision.contacts;
				if (contacts.GetLength(0) > 0)
				{
					RotateOnCollide(contacts[0].normal);
				}
				Destroy();
			}
		}

		protected virtual void OnTriggerEnter2D(Collider2D collision)
		{
			if (projectileActive && collision.tag == "HeroBox")
			{
				//this.active = false;
				//base.StartCoroutine(this.Collision(Vector2.zero, false));
				Destroy();
			}
		}

		public void OrbitShieldHit(Transform shield)
		{
			if (projectileActive)
			{
				RotateOnCollide((transform.position - shield.position).normalized);
				Destroy();
			}
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

		/// <summary>
		/// Called when the projectile first spawns
		/// </summary>
		protected virtual void OnProjectileSpawn()
		{

		}

		/// <summary>
		/// Called when the projectile collides with something or is destroyed
		/// </summary>
		protected virtual void OnProjectileDestroy()
		{

		}
	}
}
*/
