using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using WeaverCore;
using WeaverCore.Attributes;
using WeaverCore.Interfaces;
using WeaverCore.Internal;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    /// <summary>
    /// WeaverCore's implementation of breakable objects. Used for implementing grass and breakable props
    /// </summary>
    public class WeaverBreakable : Breakable, IHittable, ISerializationCallbackReceiver
    {
        static Func<Breakable, bool> isBrokenGetter;
        static Func<Breakable, AudioEvent> breakAudioEventGetter;
        static Action<Breakable, AudioEvent> breakAudioEventSetter;
        static Action<Breakable, AudioSource> audioSourcePrefabSetter;

        [HideInInspector]
        [SerializeField]
        AudioClip breakAudioEvent_Clip;

        [HideInInspector]
        [SerializeField]
        float breakAudioEvent_PitchMin;

        [HideInInspector]
        [SerializeField]
        float breakAudioEvent_PitchMax;

        [HideInInspector]
        [SerializeField]
        float breakAudioEvent_Volume;

        [OnHarmonyPatch]
        static void Patch(HarmonyPatcher patcher)
        {
            isBrokenGetter = ReflectionUtilities.CreateFieldGetter<Breakable, bool>("isBroken");
            breakAudioEventGetter = ReflectionUtilities.CreateFieldGetter<Breakable, AudioEvent>("breakAudioEvent");

            breakAudioEventSetter = ReflectionUtilities.CreateFieldSetter<Breakable, AudioEvent>("breakAudioEvent");
            audioSourcePrefabSetter = ReflectionUtilities.CreateFieldSetter<Breakable, AudioSource>("audioSourcePrefab");



            //Break()
            {
                var orig = typeof(Breakable).GetMethod(nameof(Break));
                var prefix = typeof(WeaverBreakable).GetMethod(nameof(Break_Prefix), BindingFlags.NonPublic | BindingFlags.Static);
                patcher.Patch(orig, prefix, null);
            }

            //Reset()
            {
                var orig = typeof(Breakable).GetMethod(nameof(Reset), BindingFlags.NonPublic | BindingFlags.Instance);
                var prefix = typeof(WeaverBreakable).GetMethod(nameof(Reset_Prefix), BindingFlags.NonPublic | BindingFlags.Static);
                patcher.Patch(orig, prefix, null);
            }
        }

        static bool Break_Prefix(Breakable __instance)
        {
            if (__instance is WeaverBreakable wb)
            {
                if (!isBrokenGetter(wb))
                {
                    //var soundPrefab = wb.breakSounds.GetRandomElement();
                    //var soundInstance = WeaverAudio.PlayAtPoint(soundPrefab, wb.transform.position);
                    //soundInstance.AudioSource.pitch = wb.pitchMinMax.RandomInRange();
                }
            }

            return true;
        }

        static bool Reset_Prefix(Breakable __instance)
        {
            if (__instance is WeaverBreakable wb)
            {
                void SetField(string fieldName, object value)
                {
                    typeof(Breakable).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).SetValue(wb, value);
                }

                SetField("strikeEffectPrefab", WeaverAssets.LoadWeaverAsset<GameObject>("Slash Impact").transform);

                SetField("nailHitEffectPrefab", WeaverAssets.LoadWeaverAsset<GameObject>("Nail Strike").transform);

                SetField("spellHitEffectPrefab", WeaverAssets.LoadWeaverAsset<GameObject>("Spell Hit").transform);

                SetField("dustHitRegularPrefab", WeaverAssets.LoadWeaverAsset<GameObject>("Dust Hit Med Down R").transform);

                SetField("dustHitDownPrefab", WeaverAssets.LoadWeaverAsset<GameObject>("Dust Hit Med R").transform);
            }

            return true;
        }

        public bool Hit(HitInfo hit)
        {
#if !UNITY_EDITOR
        return false;
#else
            ManageHit(hit.Direction, hit.AttackStrength, (int)hit.AttackType, hit.Attacker);
            return true;
#endif
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
#if UNITY_EDITOR
            breakAudioEvent_Clip = breakAudioEventGetter(this).Clip;
            breakAudioEvent_PitchMin = breakAudioEventGetter(this).PitchMin;
            breakAudioEvent_PitchMax = breakAudioEventGetter(this).PitchMax;
            breakAudioEvent_Volume = breakAudioEventGetter(this).Volume;
#endif
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
#if !UNITY_EDITOR
            breakAudioEventSetter(this, new AudioEvent()
            {
                Clip = breakAudioEvent_Clip,
                PitchMin = breakAudioEvent_PitchMin,
                PitchMax = breakAudioEvent_PitchMax,
                Volume = breakAudioEvent_Volume
            });

            audioSourcePrefabSetter(this, GG_Internal.AudioPlayerPrefab);
#endif

        }
    }
}