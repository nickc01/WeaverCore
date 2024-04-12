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

        //static System.Collections.Generic.List<Renderer> validObjects = new System.Collections.Generic.List<Renderer>();
        //static System.Collections.Generic.List<Renderer> invalidObjects = new System.Collections.Generic.List<Renderer>();

        [SerializeField]
        ObjectReflector reflector;

        [SerializeField]
        [Tooltip("The names of all objects to exclude from reflection")]
        System.Collections.Generic.List<string> excludedObjects = new System.Collections.Generic.List<string>();

        [SerializeField]
        [Tooltip("A list of tags to exclude from reflection")]
        System.Collections.Generic.List<string> excludedTags = new System.Collections.Generic.List<string>();

        //BoxCollider2D _collider;
        //HashSet<GameObject> addedObjects = new HashSet<GameObject>();

        /*Zprivate void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
            _collider.enabled = false;
        }*/

        /*private void FixedUpdate()
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
        }*/

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (reflector != null)
            {
                if (!excludedObjects.Contains(collision.gameObject.name) && !excludedTags.Contains(collision.gameObject.tag))
                {
                    reflector.AddObjectToReflect(collision.gameObject);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            reflector.RemoveObjectToReflect(collision.gameObject);
        }
    }
}
