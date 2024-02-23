using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
    /// <summary>
    /// Any colliders that come in contact with an object with this component will be automatically added to an object reflector
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class AutoReflectionAdder : MonoBehaviour
    {
        //static RaycastHit2D[] hitCache = new RaycastHit2D[8];

        static List<Renderer> validObjects = new List<Renderer>();
        static List<Renderer> invalidObjects = new List<Renderer>();

        [SerializeField]
        ObjectReflector reflector;

        [SerializeField]
        [Tooltip("The gameobject layers to include in reflection")]
        LayerMask collisionLayers;

        [SerializeField]
        [Tooltip("The names of all objects to exclude from reflection")]
        List<string> excludedObjects = new List<string>();

        [SerializeField]
        [Tooltip("A list of tags to exclude from reflection")]
        List<string> excludedTags = new List<string>();

        BoxCollider2D _collider;
        //HashSet<GameObject> addedObjects = new HashSet<GameObject>();

        private void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
            _collider.enabled = false;
        }

        private void FixedUpdate()
        {
            DoRaycast();
        }

        void DoRaycast()
        {
            var bounds = ColliderUtilities.GetBoundsSafe(_collider);

            Debug.DrawLine(bounds.min, bounds.max, Color.red, 1f);
            var hitTriggersPrevValue = Physics2D.queriesHitTriggers;
            Physics2D.queriesHitTriggers = true;

            var hitCache = HitCache.GetMultiCachedArray(8);
            var hitCount = Physics2D.BoxCastNonAlloc(bounds.center, bounds.size, 0f, Vector2.up, hitCache,9999, collisionLayers);
            Physics2D.queriesHitTriggers = hitTriggersPrevValue;
            if (hitCount > 0)
            {
                validObjects.Clear();
                invalidObjects.Clear();
                for (int i = 0; i < hitCount; i++)
                {
                    var gm = hitCache[i].transform.gameObject;
                    var rend = gm.GetComponent<Renderer>();
                    if (rend != null && rend.enabled && !excludedObjects.Contains(gm.name) && !excludedTags.Contains(gm.tag))
                    {
                        validObjects.Add(rend);
                        reflector?.AddObjectToReflect(rend);
                    }
                    else
                    {
                        reflector?.RemoveObjectToReflect(rend);
                    }
                }

                if (reflector != null)
                {
                    foreach (var obj in reflector.ReflectedObjects)
                    {
                        if (!validObjects.Contains(obj))
                        {
                            invalidObjects.Add(obj);
                        }
                    }

                    foreach (var invRend in invalidObjects)
                    {
                        reflector?.RemoveObjectToReflect(invRend);
                    }
                }

                if (hitCount == hitCache.Length)
                {
                    hitCache = new RaycastHit2D[hitCache.Length * 2];
                    DoRaycast();
                }
            }
        }
    }
}
