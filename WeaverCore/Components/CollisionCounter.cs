using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using WeaverCore.Interfaces;

namespace WeaverCore.Components
{
    /// <summary>
    /// Monitors and tracks collisions with 2D colliders.
    /// </summary>
    public class CollisionCounter : MonoBehaviour, IOnPool
    {
        [NonSerialized]
        System.Collections.Generic.List<Collider2D> collidedObjects = new System.Collections.Generic.List<Collider2D>();

        /// <summary>
        /// Gets the collection of colliders that the object has collided with.
        /// </summary>
        public IEnumerable<Collider2D> CollidedObjects => collidedObjects;

        /// <summary>
        /// Gets the count of currently collided objects.
        /// </summary>
        public int CollidedObjectCount => collidedObjects.Count(c => c != null);

        private void OnEnable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            collidedObjects.RemoveAll(obj => obj == null || obj.gameObject == null || !obj.gameObject.activeSelf);
        }

        private void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        }

        private void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        }

        /// <summary>
        /// Called when a 2D collider enters the trigger zone.
        /// </summary>
        /// <param name="collision">The collider that entered the trigger zone.</param>
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collidedObjects.Contains(collision))
            {
                collidedObjects.Add(collision);
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (!collidedObjects.Contains(collision))
            {
                collidedObjects.Add(collision);
            }
        }

        /// <summary>
        /// Called when a 2D collider exits the trigger zone.
        /// </summary>
        /// <param name="collision">The collider that exited the trigger zone.</param>
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collidedObjects.Contains(collision))
            {
                collidedObjects.Remove(collision);
            }
        }

        /// <summary>
        /// Called when a 2D collision occurs.
        /// </summary>
        /// <param name="collision">The collision data.</param>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collidedObjects.Contains(collision.collider))
            {
                collidedObjects.Add(collision.collider);
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (!collidedObjects.Contains(collision.collider))
            {
                collidedObjects.Add(collision.collider);
            }
        }

        /// <summary>
        /// Called when a 2D collision ends.
        /// </summary>
        /// <param name="collision">The collision data.</param>
        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collidedObjects.Contains(collision.collider))
            {
                collidedObjects.Remove(collision.collider);
            }
        }

        /// <summary>
        /// Clears the list of collided objects when the object is pooled.
        /// </summary>
        public void OnPool()
        {
            collidedObjects.Clear();
        }

        /// <summary>
        /// Finds the nearest target collider based on the given source position.
        /// </summary>
        /// <param name="sourcePos">The source position from which to find the nearest target.</param>
        /// <returns>The nearest target collider or null if none found.</returns>
        public Collider2D GetNearestTarget(Vector3 sourcePos)
        {
            float nearestDistance = float.PositiveInfinity;
            Collider2D nearestTarget = null;

            collidedObjects.RemoveAll(c => c == null);

            if (collidedObjects.Count > 0)
            {
                for (int i = collidedObjects.Count - 1; i > -1; i--)
                {
                    if (collidedObjects[i] == null || !collidedObjects[i].gameObject.activeSelf)
                    {
                        collidedObjects.RemoveAt(i);
                    }
                }

                foreach (var enemy in collidedObjects)
                {
                    if (!Physics2D.Linecast(sourcePos, enemy.transform.position, 256))
                    {
                        float sqrMagnitude = (sourcePos - enemy.transform.position).sqrMagnitude;
                        if (sqrMagnitude < nearestDistance)
                        {
                            nearestTarget = enemy;
                            nearestDistance = sqrMagnitude;
                        }
                    }
                }
                return nearestTarget;
            }

            return nearestTarget;
        }
    }
}