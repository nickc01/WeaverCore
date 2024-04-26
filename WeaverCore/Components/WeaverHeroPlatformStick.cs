using UnityEngine;

namespace WeaverCore.Components
{
    using Modding;
    using System;
    using System.ComponentModel;
    using UnityEngine;

    public class WeaverHeroPlatformStick : MonoBehaviour
    {
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

        private void OnCollisionEnter2D(Collision2D collision)
        {
            GameObject gameObject = collision.gameObject;
            if (enabled && (gameObject.layer == 9 || gameObject.GetComponent<HeroController>() != null))
            {
                WeaverLog.Log("Enabled = " + enabled);
                WeaverLog.Log(this);
                WeaverLog.Log("HERO PLATFORM STICK START");
                HeroController component = gameObject.GetComponent<HeroController>();
                if (component.cState.transitioning)
                {
                    return;
                }
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
            GameObject gameObject = collision.gameObject;
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
                Rigidbody2D component2 = gameObject.GetComponent<Rigidbody2D>();
                if (component2 != null)
                {
                    component2.interpolation = RigidbodyInterpolation2D.Interpolate;
                }
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
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