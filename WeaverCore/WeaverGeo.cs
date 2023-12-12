using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Components;
using WeaverCore.Interfaces;
using WeaverCore.Internal;
using WeaverCore.Utilities;
using static GeoControl;

namespace WeaverCore
{

    public class WeaverGeo : GeoControl, ISerializationCallbackReceiver, IOnPool
	{
        static Func<GeoControl, HeroController> heroGetter;
        static Action<GeoControl, HeroController> heroSetter;
        static Func<GeoControl, Rigidbody2D> bodyGetter;
        static Action<GeoControl> callAwake;

        static WeaverGeo _smallPrefab;
        static WeaverGeo _mediumPrefab;
        static WeaverGeo _largePrefab;

        public static WeaverGeo SmallPrefab
        {
            get
            {
                if (_smallPrefab == null)
                {
#if UNITY_EDITOR
                    _smallPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(UnityEditor.AssetDatabase.GUIDToAssetPath(UnityEditor.AssetDatabase.FindAssets("t:GameObject Weaver Geo Small")[0])).GetComponent<WeaverGeo>();
#else
                    _smallPrefab = CreatePrefabFromBase(Other_Preloads.smallGeoPrefab);
#endif
                }
                return _smallPrefab;
            }
        }

        public static WeaverGeo MediumPrefab
        {
            get
            {
                if (_mediumPrefab == null)
                {
#if UNITY_EDITOR
                    _mediumPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(UnityEditor.AssetDatabase.GUIDToAssetPath(UnityEditor.AssetDatabase.FindAssets("t:GameObject Weaver Geo Med")[0])).GetComponent<WeaverGeo>();
#else
                    _mediumPrefab = CreatePrefabFromBase(Other_Preloads.mediumGeoPrefab);
#endif
                }
                return _mediumPrefab;
            }
        }

        public static WeaverGeo LargePrefab
        {
            get
            {
                if (_largePrefab == null)
                {
#if UNITY_EDITOR
                    _largePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(UnityEditor.AssetDatabase.GUIDToAssetPath(UnityEditor.AssetDatabase.FindAssets("t:GameObject Weaver Geo Large")[0])).GetComponent<WeaverGeo>();
#else
                    _largePrefab = CreatePrefabFromBase(Other_Preloads.largeGeoPrefab);
#endif
                }
                return _largePrefab;
            }
        }

        static GameObject inactiveContainer;

        static WeaverGeo CreatePrefabFromBase(GameObject baseObj)
        {
            if (inactiveContainer == null)
            {
                inactiveContainer = new GameObject("GEO PREFAB CONTAINER");
                inactiveContainer.hideFlags = HideFlags.HideAndDontSave;
                GameObject.DontDestroyOnLoad(inactiveContainer);
                inactiveContainer.gameObject.SetActive(false);
            }

            var instance = GameObject.Instantiate(baseObj, inactiveContainer.transform);
            instance.SetActive(true);
            var sourceGeoControl = instance.GetComponent<GeoControl>();

            var weaverGeo = instance.AddComponent<WeaverGeo>();
            weaverGeo.size_idleAnim = sourceGeoControl.sizes.Select(s => s.idleAnim).ToList();
            weaverGeo.size_airAnim = sourceGeoControl.sizes.Select(s => s.airAnim).ToList();
            weaverGeo.size_value = sourceGeoControl.sizes.Select(s => s.value).ToList();
            weaverGeo.sizes = sourceGeoControl.sizes.ToArray();
            weaverGeo.pickupSounds = sourceGeoControl.pickupSounds.ToArray();
            weaverGeo.type = sourceGeoControl.type;

            var gravityScale = 0f;

            if (weaverGeo.type == 2)
            {
                gravityScale = 1.75f;
            }
            else
            {
                gravityScale = 1.5f;
            }

            weaverGeo.GetComponent<Rigidbody2D>().gravityScale = gravityScale;
            typeof(GeoControl).GetField("defaultGravity", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(weaverGeo, gravityScale);
            var pickupVibration = typeof(GeoControl).GetField("pickupVibration", BindingFlags.Instance | BindingFlags.Public);

            if (pickupVibration != null)
            {
                pickupVibration.SetValue(weaverGeo, pickupVibration.GetValue(sourceGeoControl));
            }

            weaverGeo.acidEffect = sourceGeoControl.acidEffect;
            weaverGeo.getterBug = sourceGeoControl.getterBug;
            weaverGeo.hitGround = false;

            GameObject.DestroyImmediate(sourceGeoControl);
            weaverGeo.gameObject.AddComponent<PoolableObject>();

            return weaverGeo;
        }


        [HideInInspector]
        [SerializeField]
        List<string> size_idleAnim = new List<string>();

        [HideInInspector]
        [SerializeField]
        List<string> size_airAnim = new List<string>();

        [HideInInspector]
        [SerializeField]
        List<int> size_value = new List<int>();

        WeaverAnimationPlayer _weaver_anim;
        WeaverAnimationPlayer Weaver_Anim => _weaver_anim ??= GetComponent<WeaverAnimationPlayer>();

        SpriteFlasher _weaver_flash;
        SpriteFlasher Weaver_Flash => _weaver_flash ??= GetComponent<SpriteFlasher>();

        static Func<GeoControl, Size> sizeGetter;

        bool hitGround = false;

        [NonSerialized]
        Rigidbody2D rb;
        public Rigidbody2D RB => rb == null ? rb = GetComponent<Rigidbody2D>() : rb;

        [OnHarmonyPatch]
		static void Patch(HarmonyPatcher patcher)
		{
            heroGetter = ReflectionUtilities.CreateFieldGetter<GeoControl, HeroController>("hero");
            heroSetter = ReflectionUtilities.CreateFieldSetter<GeoControl, HeroController>("hero");
            callAwake = ReflectionUtilities.MethodToDelegate<Action<GeoControl>>(typeof(GeoControl).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance));
            bodyGetter = ReflectionUtilities.CreateFieldGetter<GeoControl, Rigidbody2D>("body");

            {
                var orig = typeof(GeoControl).GetMethod(nameof(SetSize), BindingFlags.Public | BindingFlags.Instance);
                var postfix = typeof(WeaverGeo).GetMethod(nameof(WeaverSetSizePostfix), BindingFlags.Static | BindingFlags.NonPublic);

                patcher.Patch(orig, null, postfix);
            }

            {
                var orig = typeof(GeoControl).GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance);
                var prefix = typeof(WeaverGeo).GetMethod(nameof(WeaverOnEnablePrefix), BindingFlags.Static | BindingFlags.NonPublic);

                patcher.Patch(orig, prefix, null);
            }

            {
                var orig = typeof(GeoControl).GetMethod(nameof(SetFlashing), BindingFlags.Public | BindingFlags.Instance);
                var postfix = typeof(WeaverGeo).GetMethod(nameof(WeaverSetFlashingPostfix), BindingFlags.Static | BindingFlags.NonPublic);

                patcher.Patch(orig, null, postfix);
            }

            {
                var orig = typeof(GeoControl).GetMethod("OnCollisionEnter2D", BindingFlags.NonPublic | BindingFlags.Instance);
                var postfix = typeof(WeaverGeo).GetMethod(nameof(WeaverOnCollisionEnter2DPostfix), BindingFlags.Static | BindingFlags.NonPublic);

                patcher.Patch(orig, null, postfix);
            }

            {
                var orig = typeof(GeoControl).GetMethod("OnTriggerEnter2D", BindingFlags.NonPublic | BindingFlags.Instance);
                var prefix = typeof(WeaverGeo).GetMethod(nameof(WeaverOnTriggerEnter2DPrefix), BindingFlags.Static | BindingFlags.NonPublic);

                patcher.Patch(orig, prefix, null);
            }
        }

        [OnRuntimeInit]
        static void RuntimeInit()
        {
            sizeGetter = ReflectionUtilities.CreateFieldGetter<GeoControl, Size>("size");
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
#if UNITY_EDITOR
            size_idleAnim.Clear();
            size_idleAnim.AddRange(sizes.Select(s => s.idleAnim));
            size_airAnim.Clear();
            size_airAnim.AddRange(sizes.Select(s => s.airAnim));
            size_value.Clear();
            size_value.AddRange(sizes.Select(s => s.value));
#endif
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
#if !UNITY_EDITOR
            if (sizes.Length != size_idleAnim.Count)
            {
                sizes = new Size[size_idleAnim.Count];
            }
            for (int i = 0; i < size_idleAnim.Count; i++)
            {
                sizes[i] = new Size(size_idleAnim[i], size_airAnim[i], size_value[i]);
            }
#endif
        }

        static void WeaverSetSizePostfix(GeoControl __instance, int index)
        {
            if (__instance is WeaverGeo wGeo)
            {
                if (wGeo.Weaver_Anim != null)
                {
                    wGeo.Weaver_Anim.PlayAnimation(wGeo.sizes[index].airAnim);
                }
            }
        }

        static void WeaverSetFlashingPostfix(GeoControl __instance)
        {
            if (__instance is WeaverGeo wGeo)
            {
                if (wGeo.Weaver_Flash != null)
                {
                    wGeo.StartCoroutine(wGeo.GeoFlashRoutine(0.25f));
                }
            }
        }

        static void WeaverOnCollisionEnter2DPostfix(GeoControl __instance, Collision2D collision)
        {
            if (__instance is WeaverGeo wGeo)
            {
                if (collision.gameObject.layer == LayerMask.NameToLayer("Terrain"))
                {
                    wGeo.hitGround = true;
                }
                if (wGeo.Weaver_Anim != null)
                {
                    if (wGeo.Weaver_Anim.AnimationData.TryGetClip(sizeGetter(wGeo).idleAnim, out var idleClip))
                    {
                        wGeo.Weaver_Anim.PlayAnimation(idleClip.Name);
                        wGeo.Weaver_Anim.SkipFrames(UnityEngine.Random.Range(0,idleClip.Frames.Count));
                    }
                    
                }
            }
        }

        static bool WeaverOnEnablePrefix(GeoControl __instance)
        {
            /*if (__instance is WeaverGeo wGeo)
            {
                if (heroGetter(wGeo) == null)
                {
                    heroSetter(wGeo, HeroController.instance);
                }

                if (bodyGetter(__instance) == null)
                {
                    callAwake(__instance);
                }
            }*/
            return true;
        }

        static bool WeaverOnTriggerEnter2DPrefix(GeoControl __instance, Collider2D collision)
        {
            /*Debug.Log("T A = " + __instance);
            Debug.Log("TRULY NULL = " + UnityUtilities.IsObjectTrulyNull(__instance));
            Debug.Log("ACTUAL NULL = " + (__instance is null));
            Debug.Log("NULL = " + (__instance == null));
            Debug.Log("EQUALS NULL = " + __instance.Equals(null));
            Debug.Log("IS = " + (__instance is WeaverGeo));
            Debug.Log("TYPE = " + __instance.GetType());*/
            if (__instance is WeaverGeo)
            {
                if (heroGetter(__instance) == null)
                {
                    heroSetter(__instance, HeroController.instance);
                }

                /*Debug.Log("T B");
                if (heroGetter(__instance) == null)
                {
                    heroSetter(__instance, HeroController.instance);
                }
                Debug.Log("T C");
                var hero = heroGetter(__instance);
                Debug.Log("T D = " + hero);
                //Debug.Log("TRIGGER ENTER 2D PREFIX");

                bool flag = false;
                float num = 0f;
                Debug.Log("T E");
                if (collision.tag == "HeroBox")
                {
                    Debug.Log("T F");
                    var size = (Size)typeof(GeoControl).GetField("size", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
                    Debug.Log("T G = " + size);
                    hero.AddGeo(size.value);
                    Debug.Log("T H");
                    //VibrationManager.PlayVibrationClipOneShot(pickupVibration);
                    num = Mathf.Max(num, (float)typeof(GeoControl).GetMethod("PlayCollectSound", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, null));
                    Debug.Log("T I = " + num);
                    flag = true;
                }
                else if (collision.tag == "Acid")
                {
                    Debug.Log("T J = " + __instance.acidEffect);
                    if ((bool)__instance.acidEffect)
                    {
                        Debug.Log("T K");
                        __instance.acidEffect.gameObject.SetActive(value: true);
                        Debug.Log("T L");
                        num = Mathf.Max(num, __instance.acidEffect.main.duration + __instance.acidEffect.main.startLifetime.constant);
                        Debug.Log("T M");
                    }
                    Debug.Log("T N");
                    flag = true;
                }
                Debug.Log("T O = " + flag);
                if (flag)
                {
                    Debug.Log("T P");
                    var getterRoutine = (Coroutine)typeof(GeoControl).GetField("getterRoutine", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
                    Debug.Log("T Q = " + getterRoutine);
                    if (getterRoutine != null)
                    {
                        Debug.Log("T R");
                        __instance.StopCoroutine(getterRoutine);
                        Debug.Log("T S");
                    }
                    Debug.Log("T T");
                    __instance.Disable(num);
                    Debug.Log("T U");
                }
                Debug.Log("T V");*/

                /*if (bodyGetter(__instance) == null)
                {
                    callAwake(__instance);
                }

                if (heroGetter(__instance) == null)
                {
                    heroSetter(__instance, HeroController.instance);
                }*/

                return true;
            }
            return true;
        }

        IEnumerator GeoFlashRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (Weaver_Flash != null)
            {
                Weaver_Flash.FlashingSuperDash();
            }
        }

        void Update()
        {
            if (bodyGetter(this) == null)
            {
                callAwake(this);
            }

            if (heroGetter(this) == null)
            {
                heroSetter(this, HeroController.instance);
            }
#if !UNITY_EDITOR
            if (hitGround)
            {
                var oldVelocity = RB.velocity;

                if (oldVelocity.x >= 0)
                {
                    oldVelocity.x -= 2f * Time.deltaTime;
                    if (oldVelocity.x < 0)
                    {
                        oldVelocity.x = 0;
                    }
                }
                else
                {
                    oldVelocity.x += 2f * Time.deltaTime;
                    if (oldVelocity.x > 0)
                    {
                        oldVelocity.x = 0;
                    }
                }
                RB.velocity = oldVelocity;
            }
#endif
        }

        public static List<WeaverGeo> FlingLarge(int amount, Vector3 spawnPos)
        {
            return FlingGeoPrefab(new FlingUtils.Config
            {
                Prefab = LargePrefab.gameObject,
                AmountMin = amount,
                AmountMax = amount,
                SpeedMin = 25f,
                SpeedMax = 38f,
                AngleMin = 78f,
                AngleMax = 102f,
                OriginVariationX = 1f,
                OriginVariationY = 1f
            }, spawnPos);
        }

        public static List<WeaverGeo> FlingMedium(int amount, Vector3 spawnPos)
        {
            return FlingGeoPrefab(new FlingUtils.Config
            {
                Prefab = MediumPrefab.gameObject,
                AmountMin = amount,
                AmountMax = amount,
                SpeedMin = 25f,
                SpeedMax = 38f,
                AngleMin = 78f,
                AngleMax = 102f,
                OriginVariationX = 1f,
                OriginVariationY = 1f
            }, spawnPos);
        }

        public static List<WeaverGeo> FlingSmall(int amount, Vector3 spawnPos)
        {
            return FlingGeoPrefab(new FlingUtils.Config
            {
                Prefab = SmallPrefab.gameObject,
                AmountMin = amount,
                AmountMax = amount,
                SpeedMin = 25f,
                SpeedMax = 38f,
                AngleMin = 78f,
                AngleMax = 102f,
                OriginVariationX = 1f,
                OriginVariationY = 1f
            }, spawnPos);
        }

        public static List<WeaverGeo> FlingGeoPrefab(FlingUtils.Config flingInfo, Vector3 spawnPos)
        {
            return FlingUtilities.SpawnPooledAndFling(flingInfo, null, spawnPos).Select(g => g.GetComponent<WeaverGeo>()).ToList();
        }

        public static List<WeaverGeo> FlingGeo(int smallAmount, int mediumAmount, int largeAmount, Vector3 spawnPos)
        {
            var total = new List<WeaverGeo>();
            total.AddRange(FlingSmall(smallAmount, spawnPos));
            total.AddRange(FlingMedium(mediumAmount, spawnPos));
            total.AddRange(FlingLarge(largeAmount, spawnPos));
            return total;
        }

        public void OnPool()
        {
            hitGround = false;
        }
    }
}
