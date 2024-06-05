using UnityEngine;
using Modding;
using System;

namespace WeaverCore.Components
{
    public class WeaverHeroPlatformStick : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("If this field is set, then any collisions on this object will be forwarded to the proxy object. This is useful if you need to fix scaling issues")]
        WeaverHeroPlatformStick proxy;

        [NonSerialized]
        bool hooked = false;

        void Hook()
        {
            if (hooked) { return; }
            hooked = true;
            ModHooks.BeforeSceneLoadHook += ModHooks_BeforeSceneLoadHook;
        }

        private string ModHooks_BeforeSceneLoadHook(string scene)
        {
            //Disable the platform stick when a new scene is being load3ed
            enabled = false;
            return scene;
        }

        void UnHook()
        {
            if (!hooked) { return; }
            hooked = false;
            ModHooks.BeforeSceneLoadHook -= ModHooks_BeforeSceneLoadHook;
        }

        private void Awake()
        {
            Hook();
        }

        private void OnEnable()
        {
            Hook();
        }

        private void OnDisable()
        {
            UnHook();
        }

        private void OnDestroy()
        {
            UnHook();
        }

        [NonSerialized]
        Rigidbody2D playerRB;

        static Vector3 NormalizeScale(Vector3 scale)
        {
            return new Vector3(scale.x / Mathf.Abs(scale.x), scale.y / Mathf.Abs(scale.y), scale.z / Mathf.Abs(scale.z));
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (proxy != null)
            {
                proxy.OnCollisionEnter2D(collision);
                return;
            }
            GameObject gameObject = collision.gameObject;
            WeaverLog.Log("HIT OBJ = " + gameObject);
            if (enabled && (gameObject.layer == 9 || gameObject.GetComponent<HeroController>() != null))
            {
                HeroController component = gameObject.GetComponent<HeroController>();
                if (component != null && component.cState.transitioning)
                {
                    return;
                }
                
                gameObject.transform.SetParent(null);
                gameObject.transform.localScale = NormalizeScale(gameObject.transform.localScale);

                if (component != null)
                {
                    component.SetHeroParent(transform);
                }
                else
                {
                    gameObject.transform.SetParent(transform);
                }
                playerRB = gameObject.GetComponent<Rigidbody2D>();
                if (playerRB != null)
                {
                    playerRB.interpolation = RigidbodyInterpolation2D.None;
                }
            }
        }
        
        private void OnCollisionExit2D(Collision2D collision)
        {
            if (proxy != null)
            {
                proxy.OnCollisionExit2D(collision);
                return;
            }
            GameObject gameObject = collision.gameObject;
            WeaverLog.Log("NO HIT OBJ = " + gameObject);
            if (enabled && (gameObject.layer == 9 || gameObject.GetComponent<HeroController>() != null))
            {
                HeroController component = gameObject.GetComponent<HeroController>();
                if (component != null)
                {
                    component.SetHeroParent(null);
                }
                else
                {
                    gameObject.transform.SetParent(null);
                }
                gameObject.transform.localScale = NormalizeScale(gameObject.transform.localScale);
                Rigidbody2D component2 = gameObject.GetComponent<Rigidbody2D>();
                if (component2 != null)
                {
                    component2.interpolation = RigidbodyInterpolation2D.Interpolate;
                }
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (proxy != null)
            {
                proxy.OnCollisionExit2D(collision);
                return;
            }
            GameObject gameObject = collision.gameObject;
            if (enabled && (gameObject.layer == 9 || gameObject == HeroController.instance.gameObject))
            {
                if (playerRB != null)
                {
                    playerRB.interpolation = RigidbodyInterpolation2D.None;
                }
            }
        }

        public void ForceStickPlayer()
        {
            if (proxy != null)
            {
                proxy.ForceStickPlayer();
                return;
            }
            var component = HeroController.instance;
            /*if (component.cState.transitioning)
            {
                return;
            }*/
            if (component != null)
            {
                component.SetHeroParent(transform);
            }
            else
            {
                gameObject.transform.SetParent(transform);
            }
            playerRB = gameObject.GetComponent<Rigidbody2D>();
            if (playerRB != null)
            {
                playerRB.interpolation = RigidbodyInterpolation2D.None;
            }
        }

        public void ForceUnStickPlayer()
        {
            if (proxy != null)
            {
                proxy.ForceUnStickPlayer();
                return;
            }
            var component = HeroController.instance;
            if (component != null)
            {
                component.SetHeroParent(null);
            }
            else
            {
                gameObject.transform.SetParent(null);
            }
            playerRB = gameObject.GetComponent<Rigidbody2D>();
            if (playerRB != null)
            {
                playerRB.interpolation = RigidbodyInterpolation2D.Interpolate;
            }
        }
    }

}