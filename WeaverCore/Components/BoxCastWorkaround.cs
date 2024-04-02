using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WeaverCore.Components
{

	/// <summary>
	/// Use this to get around the fact that Physics2D.BoxCast doesn't work when used in-game
	/// </summary>
	public class BoxCastWorkaround : MonoBehaviour
	{
        [field: SerializeField]
		[field: Tooltip("A list of all the layers the trigger colliders will use")]
		public LayerMask triggerMask { get; private set; }


		[field: SerializeField]
		[field: Tooltip("A list of valid layers that all triggered objects must have")]
		public LayerMask objectMask { get; private set; }

		BoxCollider2D boxCollider;
		Rigidbody2D rb;
		public HashSet<Collider2D> hitColliders = new HashSet<Collider2D>();

		public UnityEvent<Collider2D> OnColliderAdded;
		public UnityEvent<Collider2D> OnColliderRemoved;

		bool _boundsSet = false;
		Bounds _bounds;

		[SerializeField]
		bool isNonBouncer = true;

		[SerializeField]
		bool isNonThunker = true;

		public Bounds Bounds
		{
			get
			{
				if (!_boundsSet)
				{
					if (boxCollider == null)
					{
						boxCollider = GetComponent<BoxCollider2D>();
                    }
					return boxCollider.bounds;
				}
				else
				{
					return _bounds;
				}
			}
		}

        private void Start()
        {
			boxCollider = GetComponent<BoxCollider2D>();

			rb = GetComponent<Rigidbody2D>();

			if (rb == null)
			{
				rb = gameObject.AddComponent<Rigidbody2D>();
				rb.bodyType = RigidbodyType2D.Kinematic;
			}

            boxCollider.isTrigger = true;
            var triggerLayers = triggerMask.value;
			for (int i = 0; i < 32; i++)
			{
				if ((triggerLayers & (1 << i)) != 0)
				{
					var layerName = LayerMask.LayerToName(i);
					var childObj = new GameObject($"{layerName} TRIGGER WORKAROUND");
					childObj.transform.position = transform.position;
					childObj.transform.rotation = transform.rotation;
					childObj.transform.localScale = transform.localScale;
					childObj.layer = i;
					var childCollider = childObj.AddComponent<BoxCollider2D>();
					childCollider.offset = boxCollider.offset;
					childCollider.size = boxCollider.size;
					childCollider.isTrigger = true;
					childCollider.edgeRadius = boxCollider.edgeRadius;
					childCollider.autoTiling = boxCollider.autoTiling;

					if (isNonBouncer)
					{
						childCollider.gameObject.AddComponent<NonBouncer>();
					}

					if (isNonThunker)
					{
						childCollider.gameObject.AddComponent<NonThunker>();
					}

					childCollider.transform.SetParent(transform, true);
				}
			}

            _bounds = boxCollider.bounds;
			_boundsSet = true;
			GameObject.Destroy(boxCollider);
        }

        private void Reset()
        {
			if (GetComponent<BoxCollider2D>() == null)
			{
				gameObject.AddComponent<BoxCollider2D>();
			}
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
			WeaverLog.Log("RAW HIT = " + collision.gameObject);
			if ((objectMask.value & (1 << collision.gameObject.layer)) != 0 && hitColliders.Add(collision))
			{
				OnColliderAdded?.Invoke(collision);
                WeaverLog.Log($"TRIGGERED {collision} of layer {collision.gameObject.layer}");
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            WeaverLog.Log("RAW UNHIT = " + collision.gameObject);
            if (hitColliders.Remove(collision))
			{
				OnColliderRemoved?.Invoke(collision);
                WeaverLog.Log($"NOT TRIGGERED {collision} of layer {collision.gameObject.layer}");
            }
        }

        public static BoxCastWorkaround Spawn(Vector3 position, Vector2 offset, Vector2 size, LayerMask triggerMask, LayerMask objectMask)
		{
			var obj = new GameObject("Box Caster");
			obj.transform.position = position;
			var boxCollider = obj.AddComponent<BoxCollider2D>();
			boxCollider.isTrigger = true;
			boxCollider.offset = offset;
			boxCollider.size = size;
			var caster = obj.AddComponent<BoxCastWorkaround>();
			caster.triggerMask = triggerMask;
			caster.objectMask = objectMask;
			return caster;
		}
    }
}
