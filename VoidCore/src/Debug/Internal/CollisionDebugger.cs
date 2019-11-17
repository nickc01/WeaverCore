using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VoidCore.Hooks;

namespace VoidCore
{
    class Visualizer : MonoBehaviour
    {
        public Collider2D collider;
        public SpriteRenderer renderer;

        static int highestID = 0;

        void Start()
        {
            if (highestID == 0)
            {
                for (int i = int.MaxValue; i >= 0; i--)
                {
                    if (SortingLayer.IsValid(i))
                    {
                        highestID = i;
                        break;
                    }
                }
            }
            collider = GetComponentInParent<Collider2D>();
            renderer = GetComponent<SpriteRenderer>();
            if (collider.isTrigger)
            {
                renderer.color = Color.Lerp(new Color(1, 1, 1, 0), Color.blue, 0.5f);
            }
            else
            {
                renderer.color = Color.Lerp(new Color(1, 1, 1, 0), Color.red, 0.5f);
            }
            gameObject.transform.localScale = Vector3.one;
            renderer.sortingOrder = int.MaxValue - 1;
            renderer.sortingLayerID = highestID;
        }

        void Update()
        {
            //ModLog.Log("Update");
            Bounds bounds = collider.bounds;
            var scale = collider.transform.localScale;
            // ModLog.Log("BOUNDS = " + bounds);
            renderer.size = new Vector2(bounds.size.x * scale.x, bounds.size.y * scale.y);
            // ModLog.Log("OFFSET = " + collider.offset);
            transform.localPosition = collider.offset;
            transform.localScale = new Vector3(1f / scale.x, 1f / scale.y);
        }
    }


    internal class CollisionVisualizer : DebugHook<VoidCore>
    {
        static GameObject CollisionPrefab;
        static SpriteRenderer renderer;

        static Dictionary<GameObject,GameObject> visualizers = new Dictionary<GameObject, GameObject>();

        protected override void Initialize()
        {
            CollisionPrefab = new GameObject();
            GameObject.DontDestroyOnLoad(CollisionPrefab);
            renderer = CollisionPrefab.AddComponent<SpriteRenderer>();
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.sprite = ResourceLoader.LoadResourceSprite("Resources.Images.DebugBox.png", border: new Vector4(19, 19, 19, 19));
            CollisionPrefab.SetActive(false);
        }

        protected override void Uninitialize()
        {
            GameObject.Destroy(renderer.sprite.texture);
            GameObject.Destroy(CollisionPrefab);
        }

        protected override void DebugEnabled()
        {
            foreach (var gm in GMTracker.ActiveGameObjects)
            {
                GameObjectCreated(gm, true);
            }
            Events.GameObjectCreated += GameObjectCreated;
            Events.GameObjectRemoved += GameObjectDestroyed;
        }

        protected override void DebugDisabled()
        {
            foreach (var gm in GMTracker.ActiveGameObjects)
            {
                GameObjectDestroyed(gm, true);
            }
            Events.GameObjectCreated -= GameObjectCreated;
            Events.GameObjectRemoved -= GameObjectDestroyed;
        }

        void GameObjectCreated(GameObject gameObject, bool created)
        {
            if (gameObject.GetComponent<Collider2D>() != null)
            {
                var collisionDebugger = GameObject.Instantiate(CollisionPrefab);
                collisionDebugger.AddComponent<Visualizer>();
                collisionDebugger.transform.SetParent(gameObject.transform);
                collisionDebugger.SetActive(true);
                visualizers.Add(gameObject, collisionDebugger);
            }
        }

        void GameObjectDestroyed(GameObject gameObject, bool destroyed)
        {
            if (visualizers.ContainsKey(gameObject))
            {
                var collisionDebugger = visualizers[gameObject];
                visualizers.Remove(gameObject);
                GameObject.Destroy(collisionDebugger);
            }
            //var visualizer = gameObject.GetComponent<Visualizer>();
            //visualizers.Remove(visualizer);
            //GameObject.Destroy(visualizer);
        }
    }




    /*internal class CollisionDebugger : MonoBehaviour
    {
        static GameObject CollisionPrefab;
        public Collider2D collider;
        public SpriteRenderer renderer;

        [ModStart(typeof(VoidCore))]
        static void ModStart()
        {
            if (CollisionPrefab == null)
            {
                CollisionPrefab = new GameObject();
                DontDestroyOnLoad(CollisionPrefab);
                var renderer = CollisionPrefab.AddComponent<SpriteRenderer>();
                renderer.drawMode = SpriteDrawMode.Sliced;
                renderer.sprite = ResourceLoader.LoadResourceSprite("Resources.Images.DebugBox.png", border: new Vector4(19, 19, 19, 19));
                CollisionPrefab.SetActive(false);
                Events.GameObjectCreated += OnNewGameObject;
            }
        }



        static void OnNewGameObject(GameObject g,bool creating)
        {
            //ModLog.Log("NEW GAMEOBJECT = " + g.name);
            if (creating && Settings.DebugMode && g.GetComponent<Collider2D>() != null)
            {
                //ModLog.Log("HAS COLLIDER");
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
    }*/
}
