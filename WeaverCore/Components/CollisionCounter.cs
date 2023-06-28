using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using WeaverCore.Interfaces;

namespace WeaverCore.Components
{
    public class CollisionCounter : MonoBehaviour, IOnPool
    {
        [NonSerialized]
        List<Collider2D> collidedObjects = new List<Collider2D>();

        public IEnumerable<Collider2D> CollidedObjects => collidedObjects;

        public int CollidedObjectCount => collidedObjects.Count;

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

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collidedObjects.Contains(collision))
            {
                collidedObjects.Add(collision);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collidedObjects.Contains(collision))
            {
                collidedObjects.Remove(collision);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collidedObjects.Contains(collision.collider))
            {
                collidedObjects.Add(collision.collider);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collidedObjects.Contains(collision.collider))
            {
                collidedObjects.Remove(collision.collider);
            }
        }

        public void OnPool()
        {
            collidedObjects.Clear();
        }

        public Collider2D GetNearestTarget(Vector3 sourcePos)
        {
            float nearestDistance = float.PositiveInfinity;
            Collider2D nearestTarget = null;

            if (collidedObjects.Count > 0)
            {
                for (int i = collidedObjects.Count - 1; i > -1; i--)
                {
                    if (collidedObjects[i] == null || !collidedObjects[i].gameObject.activeSelf)
                    {
                        collidedObjects.RemoveAt(i);
                    }
                }
                {
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
            }
            return nearestTarget;
        }
    }
}