using UnityEngine;
using CrystalCore.Hooks;

namespace CrystalCore
{
    class CollisionDebugger : GMTrackerHook<CrystalCore>
    {
        public Collider2D collider;
        public SpriteRenderer renderer;
        private static readonly int highestID = 1459018367;
        private static readonly Sprite sprite = ResourceLoader.LoadResourceSprite("Resources.Images.DebugBox.png", border: new Vector4(19, 19, 19, 19));
        private bool colliderActive = true;
        private bool colliderIsTrigger = false;

        private string objName;

        public override void OnDisable()
        {
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }

        public override void OnEnable()
        {
            if (renderer != null)
            {
                renderer.enabled = true;
            }
        }

        public override void Reset()
        {
            collider = null;
            if (renderer != null)
            {
                renderer.enabled = false;
            }
            renderer = null;
        }
        public override void Start()
        {
            objName = gameObject.name;
            if (Settings.DebugMode)
            {
                collider = GetComponent<Collider2D>();
                if (collider != null)
                {
                    renderer = GetWatcherComponent<SpriteRenderer>();
                    if (renderer == null)
                    {
                        renderer = AddWatcherComponent<SpriteRenderer>();
                        renderer.drawMode = SpriteDrawMode.Sliced;
                        renderer.sprite = sprite;
                    }
                    renderer.enabled = true;

                    colliderIsTrigger = true;

                    renderer.color = Color.Lerp(new Color(1, 1, 1, 0), Color.red, 0.5f);

                    WatcherObject.transform.localScale = Vector3.one;

                    renderer.sortingOrder = int.MaxValue - 1;

                    renderer.sortingLayerID = highestID;
                    if (!gameObject.activeInHierarchy)
                    {
                        OnDisable();
                    }
                }
                else
                {
                    StopHook();
                }
            }
            else
            {
                StopHook();
            }
        }
        public override void Update()
        {
            if (Settings.DebugMode && renderer != null && collider != null)
            {
                if (colliderActive != collider.enabled)
                {
                    colliderActive = !colliderActive;
                    if (colliderActive)
                    {
                        OnEnable();
                    }
                    else
                    {
                        OnDisable();
                    }
                }

                if (colliderIsTrigger != collider.isTrigger)
                {
                    colliderIsTrigger = !colliderIsTrigger;
                    if (colliderIsTrigger)
                    {
                        renderer.color = Color.Lerp(new Color(1, 1, 1, 0), Color.red, 0.5f);
                    }
                    else
                    {
                        renderer.color = Color.Lerp(new Color(1, 1, 1, 0), Color.blue, 0.5f);
                    }
                }

                Bounds bounds = collider.bounds;
                Vector3 scale = collider.transform.localScale;
                renderer.size = new Vector2(bounds.size.x, bounds.size.y);
                WatcherTransform.position = collider.transform.position + new Vector3(collider.offset.x, collider.offset.y);
                WatcherTransform.rotation = collider.transform.rotation;
            }
        }
    }
}