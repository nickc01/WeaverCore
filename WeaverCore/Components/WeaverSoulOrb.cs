using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    public class WeaverSoulOrb : SoulOrb
    {
        [SerializeField]
        System.Collections.Generic.List<AudioClip> soulPickupSounds = new System.Collections.Generic.List<AudioClip>();

        [SerializeField]
        OnDoneBehaviour onDone = OnDoneBehaviour.DestroyOrPool;

        static Func<SoulOrb, Transform> targetGetter;
        static Action<SoulOrb, Transform> targetSetter;

        public Transform Target
        {
            get => targetGetter(this);
            set => targetSetter(this, value);
        }

        [OnHarmonyPatch]
        static void HarmonyPatch(HarmonyPatcher patcher)
        {
            targetGetter = ReflectionUtilities.CreateFieldGetter<SoulOrb, Transform>(typeof(SoulOrb).GetField("target", BindingFlags.NonPublic | BindingFlags.Instance));
            targetSetter = ReflectionUtilities.CreateFieldSetter<SoulOrb, Transform>(typeof(SoulOrb).GetField("target", BindingFlags.NonPublic | BindingFlags.Instance));

            {
                var orig = typeof(SoulOrb).GetMethod("Zoom", BindingFlags.NonPublic | BindingFlags.Instance);
                var prefix = typeof(WeaverSoulOrb).GetMethod(nameof(ZoomPrefix), BindingFlags.Static | BindingFlags.NonPublic);
                patcher.Patch(orig, prefix, null);
            }
        }



        static bool ZoomPrefix(SoulOrb __instance)
        {
            if (__instance is WeaverSoulOrb wso)
            {
                var targetField = typeof(SoulOrb).GetField("target", BindingFlags.NonPublic | BindingFlags.Instance);
                if (((Transform)targetField.GetValue(wso)) == null)
                {
                    targetField.SetValue(wso, Player.Player1.transform);
                }
                UnboundCoroutine.Start(WaitUntilFoundPlayer(wso,isValid =>
                {
                    var sound = wso.soulPickupSounds.GetRandomElement();

                    if (sound != null)
                    {
                        WeaverAudio.PlayAtPoint(sound, wso.transform.position);
                    }

                    SpriteFlasher flasher = wso.GetComponent<SpriteFlasher>();

                    if (flasher != null)
                    {
                        flasher.flashSoulGet();
                    }

                    var dontRecycleOld = wso.dontRecycle;

                    wso.dontRecycle = true;

                    if (isValid)
                    {
                        wso.onDone.DoneWithObject(wso, 0.4f, () =>
                        {
                            if (wso != null)
                            {
                                wso.dontRecycle = dontRecycleOld;
                            }
                        });
                    }
                }));
            }

            return true;
        }

        static IEnumerator WaitUntilFoundPlayer(WeaverSoulOrb orb, Action<bool> onDone)
        {
            var rb = orb.GetComponent<Rigidbody2D>();
            yield return new WaitForFixedUpdate();
            yield return new WaitUntil(() => orb == null || orb.gameObject == null || rb.velocity == Vector2.zero);
            onDone?.Invoke(orb != null && orb.gameObject != null);
        }
    }
}