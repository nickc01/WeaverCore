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

        [Tooltip("Additional audio events to play when the object breaks.")]
        public List<AudioEvent> extraBreakSounds = new List<AudioEvent>();

        [Tooltip("If enabled, only one extra break sound is chosen at random. If disabled, every extra break sound is played.")]
        public bool extrasGetRandomlyPicked;

        [HideInInspector]
        [SerializeField]
        AudioClip[] extraBreakSounds_Clips;

        [HideInInspector]
        [SerializeField]
        float[] extraBreakSounds_PitchMin;

        [HideInInspector]
        [SerializeField]
        float[] extraBreakSounds_PitchMax;

        [HideInInspector]
        [SerializeField]
        float[] extraBreakSounds_Volume;

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
                    wb.PlayExtraBreakSounds();
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
            var breakAudioEvent = breakAudioEventGetter(this);

            breakAudioEvent_Clip = breakAudioEvent.Clip;
            breakAudioEvent_PitchMin = breakAudioEvent.PitchMin;
            breakAudioEvent_PitchMax = breakAudioEvent.PitchMax;
            breakAudioEvent_Volume = breakAudioEvent.Volume;

            if (extraBreakSounds == null || extraBreakSounds.Count == 0)
            {
                extraBreakSounds_Clips = Array.Empty<AudioClip>();
                extraBreakSounds_PitchMin = Array.Empty<float>();
                extraBreakSounds_PitchMax = Array.Empty<float>();
                extraBreakSounds_Volume = Array.Empty<float>();
            }
            else
            {
                var soundCount = extraBreakSounds.Count;

                extraBreakSounds_Clips = new AudioClip[soundCount];
                extraBreakSounds_PitchMin = new float[soundCount];
                extraBreakSounds_PitchMax = new float[soundCount];
                extraBreakSounds_Volume = new float[soundCount];

                for (int i = 0; i < soundCount; i++)
                {
                    var extraSound = extraBreakSounds[i];

                    extraBreakSounds_Clips[i] = extraSound.Clip;
                    extraBreakSounds_PitchMin[i] = extraSound.PitchMin;
                    extraBreakSounds_PitchMax[i] = extraSound.PitchMax;
                    extraBreakSounds_Volume[i] = extraSound.Volume;
                }
            }
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

            audioSourcePrefabSetter(this, GG_Preloads.AudioPlayerPrefab);

            var soundCount = extraBreakSounds_Clips?.Length ?? 0;
            extraBreakSounds = new List<AudioEvent>(soundCount);

            for (int i = 0; i < soundCount; i++)
            {
                extraBreakSounds.Add(new AudioEvent()
                {
                    Clip = extraBreakSounds_Clips[i],
                    PitchMin = GetArrayValue(extraBreakSounds_PitchMin, i),
                    PitchMax = GetArrayValue(extraBreakSounds_PitchMax, i),
                    Volume = GetArrayValue(extraBreakSounds_Volume, i)
                });
            }
#endif

        }

        void PlayExtraBreakSounds()
        {
            if (extraBreakSounds == null || extraBreakSounds.Count == 0)
            {
                return;
            }

            if (extrasGetRandomlyPicked)
            {
                extraBreakSounds[UnityEngine.Random.Range(0, extraBreakSounds.Count)].SpawnAndPlayOneShot(GG_Preloads.AudioPlayerPrefab, transform.position);
                return;
            }

            for (int i = 0; i < extraBreakSounds.Count; i++)
            {
                extraBreakSounds[i].SpawnAndPlayOneShot(GG_Preloads.AudioPlayerPrefab, transform.position);
            }
        }

        static float GetArrayValue(float[] values, int index)
        {
            if (values == null || index < 0 || index >= values.Length)
            {
                return 0f;
            }

            return values[index];
        }
    }
}
