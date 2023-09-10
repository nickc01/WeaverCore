using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Interfaces;
using WeaverCore.Internal;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{

    public class WeaverScuttlerControl : ScuttlerControl, IHittable, ISerializationCallbackReceiver
    {
        static Func<ScuttlerControl, AudioSource> sourceGetter;
        static Func<ScuttlerControl, Rigidbody2D> bodyGetter;
        static Action<ScuttlerControl, Coroutine> runRoutineSetter;
        static Func<ScuttlerControl, bool> reverseRunGetter;
        static Func<ScuttlerControl, float> accelerationGetter;
        static Func<ScuttlerControl, float> maxSpeedGetter;

        static Func<ScuttlerControl, IEnumerator> runMethod;

        [HideInInspector]
        [SerializeField]
        AudioClip bounceSound_Clip;

        [HideInInspector]
        [SerializeField]
        float bounceSound_PitchMin;

        [HideInInspector]
        [SerializeField]
        float bounceSound_PitchMax;

        [HideInInspector]
        [SerializeField]
        float bounceSound_Volume;


        [HideInInspector]
        [SerializeField]
        AudioClip deathSound1_Clip;

        [HideInInspector]
        [SerializeField]
        float deathSound1_PitchMin;

        [HideInInspector]
        [SerializeField]
        float deathSound1_PitchMax;

        [HideInInspector]
        [SerializeField]
        float deathSound1_Volume;


        [HideInInspector]
        [SerializeField]
        AudioClip deathSound2_Clip;

        [HideInInspector]
        [SerializeField]
        float deathSound2_PitchMin;

        [HideInInspector]
        [SerializeField]
        float deathSound2_PitchMax;

        [HideInInspector]
        [SerializeField]
        float deathSound2_Volume;


        [OnHarmonyPatch]
        static void Patch(HarmonyPatcher patcher)
        {
            sourceGetter = ReflectionUtilities.CreateFieldGetter<ScuttlerControl, AudioSource>("source");
            runRoutineSetter = ReflectionUtilities.CreateFieldSetter<ScuttlerControl, Coroutine>("runRoutine");
            bodyGetter = ReflectionUtilities.CreateFieldGetter<ScuttlerControl, Rigidbody2D>("body");
            reverseRunGetter = ReflectionUtilities.CreateFieldGetter<ScuttlerControl, bool>("reverseRun");
            accelerationGetter = ReflectionUtilities.CreateFieldGetter<ScuttlerControl, float>("acceleration");
            maxSpeedGetter = ReflectionUtilities.CreateFieldGetter<ScuttlerControl, float>("maxSpeed");

            runMethod = ReflectionUtilities.MethodToDelegate<Func<ScuttlerControl, IEnumerator>, ScuttlerControl>("Run");



            //Awake()
            {
                var orig = typeof(ScuttlerControl).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
                var prefix = typeof(WeaverScuttlerControl).GetMethod(nameof(Awake_Prefix), BindingFlags.NonPublic | BindingFlags.Static);

                patcher.Patch(orig, prefix, null);
            }

            //Land()
            {
                var orig = typeof(ScuttlerControl).GetMethod("Land", BindingFlags.NonPublic | BindingFlags.Instance);
                var prefix = typeof(WeaverScuttlerControl).GetMethod(nameof(Land_Prefix), BindingFlags.NonPublic | BindingFlags.Static);

                patcher.Patch(orig, prefix, null);
            }

            //Run()
            {
                var orig = typeof(ScuttlerControl).GetMethod("Run", BindingFlags.NonPublic | BindingFlags.Instance);
                var prefix = typeof(WeaverScuttlerControl).GetMethod(nameof(Run_Prefix), BindingFlags.NonPublic | BindingFlags.Static);

                patcher.Patch(orig, prefix, null);
            }
        }

        static bool Awake_Prefix(ScuttlerControl __instance)
        {
            if (__instance is WeaverScuttlerControl wsc)
            {
                __instance.journalUpdateMsgPrefab = Other_Preloads.JournalUpdateMsg;
                __instance.audioSourcePrefab = GG_Internal.AudioPlayerPrefab;
                __instance.screenFlash = Other_Preloads.HealthCocoonFlashPrefab;
            }

            return true;
        }

        static bool Land_Prefix(ScuttlerControl __instance, ref IEnumerator __result)
        {
            if (__instance is WeaverScuttlerControl wsc)
            {
                __result = NewLandRoutine(wsc);
                return false;
            }

            return true;
        }

        static bool Run_Prefix(ScuttlerControl __instance, ref IEnumerator __result)
        {
            if (__instance is WeaverScuttlerControl wsc)
            {
                __result = NewRunRoutine(wsc);
                return false;
            }

            return true;
        }

        static IEnumerator NewRunRoutine(WeaverScuttlerControl wsc)
        {
            //anim.Play(runAnim);
            var anim = wsc.GetComponent<WeaverAnimationPlayer>();
            anim.PlayAnimation(wsc.runAnim);
            sourceGetter(wsc).enabled = true;
            Vector3 velocity = bodyGetter(wsc).velocity;
            while (true)
            {
                float num = Mathf.Sign(Player.Player1.transform.position.x - wsc.transform.position.x) * (float)((!reverseRunGetter(wsc)) ? 1 : (-1));
                float currentDirection = num;
                wsc.transform.SetScaleX(Mathf.Abs(wsc.transform.localScale.x) * num);
                while (currentDirection == num)
                {
                    velocity.x += accelerationGetter(wsc) * (0f - num);
                    velocity.x = Mathf.Clamp(velocity.x, 0f - maxSpeedGetter(wsc), maxSpeedGetter(wsc));
                    velocity.y = bodyGetter(wsc).velocity.y;
                    bodyGetter(wsc).velocity = velocity;
                    yield return null;
                    num = Mathf.Sign(Player.Player1.transform.position.x - wsc.transform.position.x) * (float)((!reverseRunGetter(wsc)) ? 1 : (-1));
                }
                yield return null;
            }
            yield break;
        }

        static IEnumerator NewLandRoutine(WeaverScuttlerControl wsc)
        {
            //yield return StartCoroutine(anim.PlayAnimWait(landAnim));
            //wscsource.enabled = true;
            //var source = wsc.ReflectGetField("source") as AudioSource;
            //source.enabled = true;

            var anim = wsc.GetComponent<WeaverAnimationPlayer>();

            yield return anim.PlayAnimationTillDone(wsc.landAnim);

            sourceGetter(wsc).enabled = true;

            //runRoutine = StartCoroutine(Run());

            runRoutineSetter(wsc, wsc.StartCoroutine(runMethod(wsc)));
            yield break;
        }


        public bool Hit(HitInfo hit)
        {
#if !UNITY_EDITOR
        return false;
#else
            ManageHit((int)hit.AttackType, hit.Direction);
            return true;
#endif
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
#if UNITY_EDITOR
            bounceSound_Clip = bounceSound.Clip;
            bounceSound_PitchMin = bounceSound.PitchMin;
            bounceSound_PitchMax = bounceSound.PitchMax;
            bounceSound_Volume = bounceSound.Volume;

            deathSound1_Clip = deathSound1.Clip;
            deathSound1_PitchMin = deathSound1.PitchMin;
            deathSound1_PitchMax = deathSound1.PitchMax;
            deathSound1_Volume = deathSound1.Volume;

            deathSound2_Clip = deathSound2.Clip;
            deathSound2_PitchMin = deathSound2.PitchMin;
            deathSound2_PitchMax = deathSound2.PitchMax;
            deathSound2_Volume = deathSound2.Volume;
#endif
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
#if !UNITY_EDITOR
            bounceSound = new AudioEvent()
            {
                Clip = bounceSound_Clip,
                PitchMin = bounceSound_PitchMin,
                PitchMax = bounceSound_PitchMax,
                Volume = bounceSound_Volume
            };

            deathSound1 = new AudioEvent()
            {
                Clip = deathSound1_Clip,
                PitchMin = deathSound1_PitchMin,
                PitchMax = deathSound1_PitchMax,
                Volume = deathSound1_Volume
            };

            deathSound2 = new AudioEvent()
            {
                Clip = deathSound2_Clip,
                PitchMin = deathSound2_PitchMin,
                PitchMax = deathSound2_PitchMax,
                Volume = deathSound2_Volume
            };
#endif
        }
    }
}