using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Interfaces;

namespace WeaverCore.Components
{
    public class CollisionCounter : MonoBehaviour, IOnPool
    {
        [NonSerialized]
        HashSet<Collider2D> collidedObjects = new HashSet<Collider2D>();

        public IEnumerable<Collider2D> CollidedObjects => collidedObjects;

        public int CollidedObjectCount => collidedObjects.Count;

        private void OnEnable()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        }

        private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
        {
            collidedObjects.Clear();
        }

        private void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        }

        private void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            collidedObjects.Add(collision);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            collidedObjects.Remove(collision);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            collidedObjects.Add(collision.collider);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            collidedObjects.Remove(collision.collider);
        }

        public void OnPool()
        {
            collidedObjects.Clear();
        }
    }
}