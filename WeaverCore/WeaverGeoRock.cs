using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using WeaverCore.Assets;
using WeaverCore.Assets.Components;
using WeaverCore.Attributes;
using WeaverCore.Components;
using WeaverCore.Settings;
using WeaverCore.Utilities;

namespace WeaverCore
{
    /// <summary>
    /// A small rock that spawns geo when hit by the player
    /// </summary>
    public class WeaverGeoRock : GeoRock, ISerializationCallbackReceiver
    {
        [HideInInspector]
        [SerializeField]
        string geoRockData_id;

        [SerializeField]
        [HideInInspector]
        string geoRockData_sceneName;

        [SerializeField]
        [HideInInspector]
        int geoRockData_hitsLeft;

        [field: SerializeField]
        [field: Tooltip("The amount of hits required before the geo rock breaks")]
        public int TotalHits { get; protected set; } = 5;

        [field: SerializeField]
        [field: Tooltip("The amount of small geo to spawn per hit")]
        public int GeoPerHit { get; protected set; } = 2;

        [field: SerializeField]
        [field: Tooltip("The amount of small geo to spawn on the final hit")]
        public int FinalHitGeoPayout { get; protected set; } = 5;

        [field: SerializeField]
        [field: Tooltip("Once hit, this represents the amount of time before the rock can be hit again")]
        public float RecoilTime { get; protected set; } = 0.1f;

        [Space]
        [Header("Animations")]
        [SerializeField]
        [Tooltip("The gleam effect that the rock plays when idle")]
        protected string gleamAnimation = "Gleam 1";

        [SerializeField]
        [Tooltip("The animation that is played when the rock is broken")]
        protected string brokenAnimation = "Broken 1";

        [SerializeField]
        [Tooltip("The intensity of the jitter effect when hit")]
        protected Vector2 jitterIntensity = new Vector2(0.05f, 0.05f);

        [Space]
        [Header("Sounds and Effects")]
        [SerializeField]
        protected List<AudioClip> hitSounds = new List<AudioClip>();

        [SerializeField]
        List<GameObject> hitDustPrefabs = new List<GameObject>();

        [SerializeField]
        protected List<AudioClip> finalBreakSounds = new List<AudioClip>();

        [SerializeField]
        protected List<GameObject> finalHitDustPrefabs = new List<GameObject>();


        [Space]
        [Header("Save Settings")]
        [SerializeField]
        [Tooltip("The Save Settings object used to store the remaining hits for this geo rock")]
        protected SaveSpecificSettings saveSettings;

        [SerializeField]
        [Tooltip("The name of the int field in the Save Settings used to keep track of the remaining hits left before this geo rock breaks")]
        [SaveSpecificFieldName(typeof(int), nameof(saveSettings))]
        protected string hitsRemainingSaveSettingsField;

        [NonSerialized]
        WeaverAnimationPlayer _animator;
        public WeaverAnimationPlayer Animator => _animator ??= GetComponent<WeaverAnimationPlayer>();


        int hits_remaining_internal = 0;
        public virtual int HitsRemaining
        {
            get
            {
                if (saveSettings == null)
                {
                    return hits_remaining_internal;
                }
                else
                {
                    if (saveSettings.TryGetFieldValue<int>(hitsRemainingSaveSettingsField,out var result))
                    {
                        if (result == -1)
                        {
                            return 0;
                        }
                        else if (result == 0)
                        {
                            return TotalHits;
                        }
                        else
                        {
                            return result;
                        }
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            set
            {
                if (saveSettings == null)
                {
                    hits_remaining_internal = value;
                }
                else
                {
                    if (saveSettings.HasField<int>(hitsRemainingSaveSettingsField))
                    {
                        if (value < 0)
                        {
                            value = 0;
                        }

                        if (value == 0)
                        {
                            value = -1;
                        }

                        saveSettings.SetFieldValue(hitsRemainingSaveSettingsField, value);
                    }
                    else
                    {
                        WeaverLog.LogWarning($"Couldn't save geo rock hit remaining count to field {hitsRemainingSaveSettingsField} in save settings object : {saveSettings}");
                    }
                }
            }
        }

        Coroutine jitterRoutine;
        GameObject damager;
        bool hitByDamager = false;

        public virtual void ResetHitRemaining()
        {
            if (saveSettings == null)
            {
                hits_remaining_internal = TotalHits;
            }
            else
            {
                if (saveSettings.HasField<int>(hitsRemainingSaveSettingsField))
                {
                    saveSettings.SetFieldValue(hitsRemainingSaveSettingsField, 0);
                }
            }
        }

        protected virtual void Reset()
        {
            finalBreakSounds.Clear();
            finalBreakSounds.Add(WeaverAssets.LoadWeaverAsset<AudioClip>("breakable_wall_hit_1"));
            finalBreakSounds.Add(WeaverAssets.LoadWeaverAsset<AudioClip>("breakable_wall_hit_2"));

            finalHitDustPrefabs.Clear();
            finalHitDustPrefabs.Add(WeaverAssets.LoadWeaverAsset<GameObject>("Dust Hit Med R"));
            finalHitDustPrefabs.Add(WeaverAssets.LoadWeaverAsset<GameObject>("Dust Hit Med Down R"));

            hitSounds.Clear();
            hitSounds.Add(WeaverAssets.LoadWeaverAsset<AudioClip>("geo_rock_hit_1"));
            hitSounds.Add(WeaverAssets.LoadWeaverAsset<AudioClip>("geo_rock_hit_2"));
            hitSounds.Add(WeaverAssets.LoadWeaverAsset<AudioClip>("geo_rock_hit_3"));

            hitDustPrefabs = finalHitDustPrefabs.ToList();
        }

        [OnHarmonyPatch]
        static void Patch(HarmonyPatcher patcher)
        {
            {
                var orig = typeof(GeoRock).GetMethod("LevelActivated", BindingFlags.NonPublic | BindingFlags.Instance);
                var prefix = typeof(WeaverGeoRock).GetMethod(nameof(LevelActivatedPrefix), BindingFlags.Static | BindingFlags.NonPublic);
                patcher.Patch(orig, prefix, null);
            }

            {
                var orig = typeof(GeoRock).GetMethod("SaveState", BindingFlags.NonPublic | BindingFlags.Instance);
                var prefix = typeof(WeaverGeoRock).GetMethod(nameof(SaveStatePrefix), BindingFlags.Static | BindingFlags.NonPublic);
                patcher.Patch(orig, prefix, null);
            }

            {
                var orig = typeof(GeoRock).GetMethod("UpdateHitsLeftFromFSM", BindingFlags.NonPublic | BindingFlags.Instance);
                var prefix = typeof(WeaverGeoRock).GetMethod(nameof(UpdateHitsLeftFromFSMPrefix), BindingFlags.Static | BindingFlags.NonPublic);
                patcher.Patch(orig, prefix, null);
            }
        }

        protected virtual void Awake()
        {
            hits_remaining_internal = TotalHits;
            StartCoroutine(MainRoutine());
        }

        protected IEnumerator MainRoutine()
        {
            var oldPos = transform.position;
            var rotation = transform.rotation.eulerAngles.z;
            if (HitsRemaining == 0)
            {
                Animator.PlayAnimation(brokenAnimation);
                GetComponent<BoxCollider2D>().enabled = false;
                SendMessage("SetActive", true, SendMessageOptions.DontRequireReceiver);
                yield break;
            }

            hitByDamager = false;

            while (true)
            {
                float waiter = UnityEngine.Random.Range(1.5f, 4f);
                transform.position = oldPos;
                for (float t = 0; t < waiter; t += Time.deltaTime)
                {
                    if (hitByDamager)
                    {
                        hitByDamager = false;

#if !UNITY_EDITOR
                        var attackType = PlayMakerUtilities.GetFsmInt(damager, "damages_enemy", "attackType");
                        var direction = (PlayMakerUtilities.GetFsmInt(damager, "damages_enemy", "direction") + 360f) % 360f;
#else
                        int attackType;
                        float direction;
                        {
                            var damagerComponent = damager.GetComponent<EnemyDamager>();

                            attackType = (int)damagerComponent.attackType;
                            direction = damagerComponent.hitDirection.ToDegrees();
                        }
#endif

                        if (attackType == 0 || attackType == 2)
                        {
                            Pooling.Instantiate(EffectAssets.NailStrikePrefab, transform.position, Quaternion.identity);

                            SpawnGeo(GeoPerHit);

                            if ((HitsRemaining -= 1) == 0)
                            {
                                yield return null;
                                WeaverAudio.PlayAtPoint(finalBreakSounds.GetRandomElement(), transform.position);
                                CameraShaker.Instance.Shake(Enums.ShakeType.EnemyKillShake);
                                RockParticles.Spawn(transform.position, Quaternion.Euler(0, 0, 0f), 0.25f, 200f);
                                SpawnGeo(FinalHitGeoPayout);

                                Animator.PlayAnimation(brokenAnimation);

                                foreach (var prefab in finalHitDustPrefabs)
                                {
                                    Pooling.Instantiate(prefab, transform.position + new Vector3(0f, 0.1f), Quaternion.Euler(-72.5f, -180f, -180f));
                                }

                                GetComponent<BoxCollider2D>().enabled = false;
                                SendMessage("SetActive", true, SendMessageOptions.DontRequireReceiver);
                                yield break;
                            }

                            WeaverAudio.PlayAtPoint(hitSounds.GetRandomElement(), transform.position);

                            float flingAngleMin, flingAngleMax, directionComparison, dustRotX, dustRotZ;

                            if (direction < 45f) //HIT RIGHT
                            {
                                directionComparison = 270f;
                                flingAngleMin = 20f;
                                flingAngleMax = 40f;
                                dustRotX = 0f;
                                dustRotZ = 270f;
                            }
                            else if (direction < 135f || (direction >= 225 && direction < 360f)) //HIT DOWN AND HIT UP
                            {
                                directionComparison = 180f;
                                flingAngleMin = 70f;
                                flingAngleMax = 110f;
                                dustRotX = -72.5f;
                                dustRotZ = -180f;
                            }
                            else //direction < 225f HIT LEFT
                            {
                                directionComparison = 90f;
                                flingAngleMin = 100;
                                flingAngleMax = 140f;
                                dustRotX = 180f;
                                dustRotZ = 270f;
                            }

                            StartJitter(oldPos, jitterIntensity);

                            if (Mathf.Abs(rotation - directionComparison) > 10f)
                            {
                                RockParticles.Spawn(transform.position, Quaternion.Euler(0, 0, Mathf.Lerp(flingAngleMin, flingAngleMax, 0.5f) + rotation - 90f), 0.25f, 50f);

                                foreach (var prefab in hitDustPrefabs)
                                {
                                    Pooling.Instantiate(prefab, transform.position + new Vector3(0f, 0f, 0.1f), Quaternion.Euler(dustRotX, -180f, dustRotZ));
                                }

                                yield return new WaitForSeconds(RecoilTime);
                            }

                            yield return new WaitForSeconds(RecoilTime);

                            StopJitter();
                        }
                    }
                    yield return null;
                }

                Animator.PlayAnimation(gleamAnimation);
            }
            //yield return new WaitForSeconds(waiter);
        }

        IEnumerator JitterRoutine(Vector3 startPos, Vector3 jitterIntensity)
        {
            while (true)
            {
                transform.position = startPos + new Vector3(jitterIntensity.x * UnityEngine.Random.Range(-1f,1f), jitterIntensity.y * UnityEngine.Random.Range(-1f, 1f), jitterIntensity.z * UnityEngine.Random.Range(-1f, 1f));
                yield return new WaitForFixedUpdate();
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (geoRockData == null)
            {
                geoRockData = new GeoRockData();
            }
            geoRockData_id = geoRockData.id;
            geoRockData_sceneName = geoRockData.sceneName;
            geoRockData_hitsLeft = geoRockData.hitsLeft;
#endif
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
#if !UNITY_EDITOR
            geoRockData = new GeoRockData
            {
                id = geoRockData_id,
                sceneName = geoRockData_sceneName,
                hitsLeft = geoRockData_hitsLeft
            };
#endif
        }

        protected List<WeaverGeo> SpawnGeo(int amount)
        {
            return WeaverGeo.FlingGeoPrefab(new FlingUtils.Config
            {
                Prefab = WeaverGeo.SmallPrefab.gameObject,
                AmountMin = amount,
                AmountMax = amount,
                SpeedMin = 23f,
                SpeedMax = 30f,
                AngleMin = 80f,
                AngleMax = 100f,
                OriginVariationX = 0.25f,
                OriginVariationY = 0.25f
            }, transform.position);
        }

        static bool LevelActivatedPrefix(GeoRock __instance)
        {
            if (__instance is WeaverGeoRock wgr)
            {
                //__instance.ReflectCallMethod("SetMyID");
                //__instance.ReflectCallMethod("UpdateHitsLeftFromFSM");
                typeof(GeoRock).GetMethod("SetMyID", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, null);
                typeof(GeoRock).GetMethod("UpdateHitsLeftFromFSM", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, null);
                return false;
            }
            else
            {
                return true;
            }
        }

        static bool SaveStatePrefix(GeoRock __instance)
        {
            if (__instance is WeaverGeoRock wgr)
            {
                //__instance.ReflectCallMethod("SetMyID");
                //__instance.ReflectCallMethod("UpdateHitsLeftFromFSM");
                typeof(GeoRock).GetMethod("SetMyID", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, null);
                typeof(GeoRock).GetMethod("UpdateHitsLeftFromFSM", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, null);
                return false;
            }
            else
            {
                return true;
            }
        }

        static bool UpdateHitsLeftFromFSMPrefix(GeoRock __instance)
        {
            if (__instance is WeaverGeoRock wgr)
            {
                //TODO - UPDATE HITS LEFT
                return false;
            }
            else
            {
                return true;
            }
        }

        void OnCollisionEnter2D(Collision2D collisionInfo)
        {
            StoreCollisionInfo(collisionInfo);
        }

        void OnTriggerEnter2D(Collider2D collisionInfo)
        {
            StoreTriggerInfo(collisionInfo);
        }
/*#if !UNITY_EDITOR
        void OnTriggerStay2D(Collider2D collisionInfo)
        {
            StoreTriggerInfo(collisionInfo);
        }
#endif*/

        private void StoreCollisionInfo(Collision2D collisionInfo)
        {
            StoreIfDamagingObject(collisionInfo.gameObject);
        }

        private void StoreTriggerInfo(Collider2D collisionInfo)
        {
            StoreIfDamagingObject(collisionInfo.gameObject);
        }

        private void StoreIfDamagingObject(GameObject go)
        {
            //PlayMakerFSM playMakerFSM = FSMUtility.LocateFSM(go, fsmName.Value);
#if !UNITY_EDITOR
            if (PlayMakerUtilities.GetPlaymakerFSMOnObject(go, "damages_enemy") != null && PlayMakerUtilities.GetFsmInt(go, "damages_enemy", "damageDealt") > 0)
            {
                damager = go;
                hitByDamager = true;
                //DO HIT EVENT HERE
            }
#else
            if (go.TryGetComponent<EnemyDamager>(out var damagerComponent) && damagerComponent.damage > 0)
            {
                damager = go;
                hitByDamager = true;
            }
#endif
        }

        protected void StartJitter(Vector3 startPos, Vector3 jitterIntensity)
        {
            StopJitter();
            jitterRoutine = StartCoroutine(JitterRoutine(startPos, jitterIntensity));
        }

        protected void StopJitter()
        {
            if (jitterRoutine != null)
            {
                StopCoroutine(jitterRoutine);
                jitterRoutine = null;
            }
        }
    }
}
