using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VoidCore
{
    internal class CollisionDebugger : MonoBehaviour
    {
        static GameObject CollisionPrefab;

        public Collider2D collider;
        public SpriteRenderer renderer;

        [ModStart(typeof(VoidCore))]
        static void ModStart()
        {
            Events.GameObjectCreated += OnNewGameObject;
            CollisionPrefab = new GameObject();
            DontDestroyOnLoad(CollisionPrefab);
            var renderer = CollisionPrefab.AddComponent<SpriteRenderer>();
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.sprite = ResourceLoader.LoadResourceSprite("Resources.Images.DebugBox.png", border: new Vector4(19, 19, 19, 19));
            CollisionPrefab.SetActive(false);
        }



        static void OnNewGameObject(GameObject g)
        {
            ModLog.Log("NEW GAMEOBJECT = " + g.name);
            if (Settings.DebugMode && g.GetComponent<Collider2D>() != null)
            {
                ModLog.Log("HAS COLLIDER");
                var collisionDebugger = Instantiate(CollisionPrefab);
                collisionDebugger.AddComponent<CollisionDebugger>();
                collisionDebugger.transform.SetParent(g.transform);
                collisionDebugger.SetActive(true);
            }
        }

        void Start()
        {
            //ModLog.Log("START FUNCTION");
            if (gameObject != CollisionPrefab)
            {
                Events.InternalOnDebugEnd += OnDebugStop;
            }
            collider = GetComponentInParent<Collider2D>();
            renderer = GetComponent<SpriteRenderer>();
            if (collider.isTrigger)
            {
                renderer.color = Color.Lerp(new Color(1,1,1,0), Color.blue, 0.5f);
            }
            else
            {
                renderer.color = Color.Lerp(new Color(1, 1, 1, 0), Color.red, 0.5f);
            }
            gameObject.transform.localScale = Vector3.one;
        }

        void Update()
        {
            //ModLog.Log("Update");
            Bounds bounds = collider.bounds;
           // ModLog.Log("BOUNDS = " + bounds);
            renderer.size = new Vector2(bounds.size.x, bounds.size.y);
            // ModLog.Log("OFFSET = " + collider.offset);
            transform.localPosition = collider.offset;
        }

        void OnDebugStop()
        {
            Destroy(gameObject);
        }

        void OnDestroy()
        {
            Events.InternalOnDebugEnd -= OnDebugStop;
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            ModLog.Log($"Collided With {collision.gameObject.name}");
        }

        void OnCollisionExit2D(Collision2D collision)
        {
            ModLog.Log($"Stopped Colliding With {collision.gameObject.name}");
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            ModLog.Log($"Triggered With {collider.gameObject.name}");
        }

        void OnTriggerExit2D(Collider2D collider)
        {
            ModLog.Log($"Stopped Triggering With {collider.gameObject.name}");
        }


    }
}
