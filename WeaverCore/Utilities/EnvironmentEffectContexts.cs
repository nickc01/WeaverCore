using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace WeaverCore.Utilities
{
    /// <summary>
    /// Base context for environment effect patches.
    /// </summary>
    public abstract class EnvironmentEffectContext
    {
        static readonly Dictionary<string, FieldInfo> fieldCache = new Dictionary<string, FieldInfo>();

        readonly Component component;
        readonly Type componentType;

        protected EnvironmentEffectContext(Component component)
        {
            this.component = component ?? throw new ArgumentNullException(nameof(component));
            componentType = component.GetType();
        }

        public Component Component => component;

        public GameObject GameObject => component.gameObject;

        public Transform Transform => component.transform;

        protected T GetValue<T>(string fieldName)
        {
            var field = GetField(fieldName);
            if (field == null)
            {
                return default;
            }

            var value = field.GetValue(component);
            if (value is T typedValue)
            {
                return typedValue;
            }

            return default;
        }

        protected void SetValue<T>(string fieldName, T value)
        {
            var field = GetField(fieldName);
            if (field != null)
            {
                field.SetValue(component, value);
            }
        }

        protected FieldInfo GetField(string fieldName)
        {
            var cacheKey = componentType.FullName + "|" + fieldName;

            if (!fieldCache.TryGetValue(cacheKey, out var field))
            {
                field = componentType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                fieldCache[cacheKey] = field;
            }

            return field;
        }

        protected void DisableAllChildren()
        {
            foreach (Transform child in Transform)
            {
                child.gameObject.SetActive(false);
            }
        }

        protected void SetRandomFlipX(GameObject gameObject)
        {
            if (gameObject == null || UnityEngine.Random.Range(1, 100) <= 50)
            {
                return;
            }

            var localScale = gameObject.transform.localScale;
            gameObject.transform.localScale = new Vector3(-localScale.x, localScale.y, localScale.z);
        }

        public GameObject SpawnPrefab(GameObject prefab, Vector3 positionOffset, Vector3 rotation, bool parentToEffect, bool randomFlipX)
        {
            if (prefab == null)
            {
                return null;
            }

            var instance = UnityEngine.Object.Instantiate(prefab, Transform.TransformPoint(positionOffset), Quaternion.Euler(rotation));
            if (parentToEffect)
            {
                instance.transform.SetParent(Transform, true);
            }
            if (randomFlipX)
            {
                SetRandomFlipX(instance);
            }
            return instance;
        }
    }

    /// <summary>
    /// Context for custom dash effects.
    /// </summary>
    public sealed class DashEffectContext : EnvironmentEffectContext
    {
        public DashEffectContext(Component component) : base(component)
        {
        }

        public bool IsHeroInPosition => HeroController.instance != null && HeroController.instance.isHeroInPosition;

        public AudioClip SplashClip => GetValue<AudioClip>("splashClip");

        AudioSource AudioSource
        {
            get => GetValue<AudioSource>("audioSource");
            set => SetValue("audioSource", value);
        }

        public void Reset()
        {
            if (GetValue<PlayerData>("pd") == null && GameManager.instance != null)
            {
                SetValue("pd", GameManager.instance.playerData);
            }
            if (AudioSource == null)
            {
                AudioSource = GameObject.GetComponent<AudioSource>();
            }

            DisableAllChildren();
            SetRecycleTimer(1f);
        }

        public void SetRecycleTimer(float recycleTime)
        {
            SetValue("recycleTimer", recycleTime);
        }

        public void PlayDashPuff()
        {
            var dashPuff = GetValue<GameObject>("heroDashPuff");
            if (dashPuff == null)
            {
                return;
            }

            dashPuff.SetActive(true);

            var animator = GetValue<Component>("heroDashPuff_anim");
            if (animator == null)
            {
                animator = Tk2dUtilities.FindTk2dSpriteAnimator(dashPuff);
                SetValue("heroDashPuff_anim", animator);
            }

            if (animator != null)
            {
                new Tk2dSpriteAnimatorWrapper(animator).PlayFromFrame(0);
            }
        }

        public void PlayDust()
        {
            GetValue<GameObject>("dashDust")?.SetActive(true);
        }

        public void PlayGrass()
        {
            GetValue<GameObject>("dashGrass")?.SetActive(true);
        }

        public void PlayBone()
        {
            GetValue<GameObject>("dashBone")?.SetActive(true);
        }

        public void PlayWaterCut()
        {
            GetValue<GameObject>("waterCut")?.SetActive(true);
        }

        public void PlayOneShot(AudioClip clip)
        {
            if (clip != null && AudioSource != null)
            {
                AudioSource.PlayOneShot(clip);
            }
        }
    }

    /// <summary>
    /// Context for custom jump effects.
    /// </summary>
    public sealed class JumpEffectContext : EnvironmentEffectContext
    {
        public JumpEffectContext(Component component) : base(component)
        {
        }

        GameObject HeroObject
        {
            get => GetValue<GameObject>("heroObject");
            set => SetValue("heroObject", value);
        }

        Rigidbody2D HeroRigidBody
        {
            get => GetValue<Rigidbody2D>("heroRigidBody");
            set => SetValue("heroRigidBody", value);
        }

        public void Reset()
        {
            if (GetValue<PlayerData>("pd") == null && GameManager.instance != null)
            {
                SetValue("pd", GameManager.instance.playerData);
            }

            DisableAllChildren();
            SetValue("fallTimer", 0.1f);
            SetValue("recycleTimer", 1f);
            SetValue("dripTimer", 0f);
            SetValue("dripEndTimer", 0f);
            SetValue("dripping", false);
            SetValue("checkForFall", false);
            SetValue("trailAttached", false);
        }

        public void SetRecycleTimer(float recycleTime)
        {
            SetValue("recycleTimer", recycleTime);
        }

        public void EnableFallCheck()
        {
            SetValue("checkForFall", true);
            EnsureHeroReferences();
        }

        public void PlayDust()
        {
            GetValue<GameObject>("dustEffects")?.SetActive(true);
        }

        public void PlayGrass()
        {
            GetValue<GameObject>("grassEffects")?.SetActive(true);
        }

        public void PlayBone()
        {
            GetValue<GameObject>("boneEffects")?.SetActive(true);
        }

        public void PlaySplash(bool randomFlip)
        {
            var splash = GetValue<GameObject>("splash");
            if (splash == null)
            {
                return;
            }

            splash.SetActive(true);
            if (randomFlip)
            {
                SetRandomFlipX(splash);
            }
        }

        public void PlayTrail()
        {
            GetValue<GameObject>("dustTrail")?.SetActive(true);
            SetValue("trailAttached", true);
            EnsureHeroReferences();
        }

        public void PlayJumpPuff()
        {
            EnsureHeroReferences();

            var heroRigidBody = HeroRigidBody;
            var jumpPuff = GetValue<GameObject>("jumpPuff");

            if (heroRigidBody == null || jumpPuff == null)
            {
                return;
            }

            var z = heroRigidBody.velocity.x * -3f + 2.6f;
            jumpPuff.transform.localEulerAngles = new Vector3(0f, 0f, z);
            jumpPuff.SetActive(true);

            var animator = GetValue<Component>("jumpPuffAnimator");
            if (animator == null)
            {
                animator = Tk2dUtilities.FindTk2dSpriteAnimator(jumpPuff);
                SetValue("jumpPuffAnimator", animator);
            }

            if (animator != null)
            {
                new Tk2dSpriteAnimatorWrapper(animator).PlayFromFrame(0);
            }
        }

        public void SplashOut()
        {
            EnsureHeroReferences();

            if (HeroObject == null)
            {
                return;
            }

            var spatterWhitePrefab = GetValue<GameObject>("spatterWhitePrefab");
            if (spatterWhitePrefab == null)
            {
                return;
            }

            SetValue("dripEndTimer", 0.4f);
            SetValue("dripping", true);

            var position = HeroObject.transform.position;
            var velocity = default(Vector2);

            for (var i = 1; i <= 11; i++)
            {
                var obj = spatterWhitePrefab.Spawn(position);
                var speed = UnityEngine.Random.Range(5f, 12f);
                var angle = UnityEngine.Random.Range(80f, 110f);
                velocity.x = speed * Mathf.Cos(angle * ((float)Math.PI / 180f));
                velocity.y = speed * Mathf.Sin(angle * ((float)Math.PI / 180f));

                var rigidBody = obj.GetComponent<Rigidbody2D>();
                if (rigidBody != null)
                {
                    rigidBody.velocity = velocity;
                }
            }
        }

        void EnsureHeroReferences()
        {
            if (HeroObject != null && HeroRigidBody != null)
            {
                return;
            }

            var heroController = HeroController.instance;
            if (heroController == null)
            {
                return;
            }

            HeroObject = heroController.gameObject;
            HeroRigidBody = HeroObject.GetComponent<Rigidbody2D>();
        }
    }

    /// <summary>
    /// Context for custom soft landing effects.
    /// </summary>
    public sealed class SoftLandEffectContext : EnvironmentEffectContext
    {
        public SoftLandEffectContext(Component component) : base(component)
        {
        }

        public bool IsHeroInPosition => HeroController.instance != null && HeroController.instance.isHeroInPosition;

        public AudioClip SoftLandClip => GetValue<AudioClip>("softLandClip");

        public AudioClip WetLandClip => GetValue<AudioClip>("wetLandClip");

        AudioSource AudioSource
        {
            get => GetValue<AudioSource>("audioSource");
            set => SetValue("audioSource", value);
        }

        public void Reset()
        {
            if (GetValue<PlayerData>("pd") == null && GameManager.instance != null)
            {
                SetValue("pd", GameManager.instance.playerData);
            }
            if (AudioSource == null)
            {
                AudioSource = GameObject.GetComponent<AudioSource>();
            }

            DisableAllChildren();
            SetRecycleTimer(1f);
        }

        public void SetRecycleTimer(float recycleTime)
        {
            SetValue("recycleTimer", recycleTime);
        }

        public void PlayDust()
        {
            GetValue<GameObject>("dustEffects")?.SetActive(true);
        }

        public void PlayGrass()
        {
            GetValue<GameObject>("grassEffects")?.SetActive(true);
        }

        public void PlayBone()
        {
            GetValue<GameObject>("boneEffects")?.SetActive(true);
        }

        public void PlaySplash(bool randomFlip)
        {
            var splash = GetValue<GameObject>("splash");
            if (splash == null)
            {
                return;
            }

            splash.SetActive(true);
            if (randomFlip)
            {
                SetRandomFlipX(splash);
            }
        }

        public void PlayOneShot(AudioClip clip)
        {
            if (clip != null && AudioSource != null)
            {
                AudioSource.PlayOneShot(clip);
            }
        }
    }

    /// <summary>
    /// Context for custom hard landing effects.
    /// </summary>
    public sealed class HardLandEffectContext : EnvironmentEffectContext
    {
        static readonly Vector3 hardLandSpawnOffset = new Vector3(0f, -0.9f, 0f);

        public HardLandEffectContext(Component component) : base(component)
        {
        }

        public GameObject ParticleRockPrefab => GetValue<GameObject>("particleRockPrefab");

        public GameObject SpatterWhitePrefab => GetValue<GameObject>("spatterWhitePrefab");

        public void Reset()
        {
            ResetBuiltInObject("dustObj");
            ResetBuiltInObject("grassObj");
            ResetBuiltInObject("boneObj");
            ResetBuiltInObject("spaObj");
            ResetBuiltInObject("metalObj");
            ResetBuiltInObject("wetObj");

            SetRecycleTime(1.5f);
        }

        public void PlayAverageShake()
        {
            var cameras = GameCameras.instance;
            if (cameras == null)
            {
                return;
            }

            var cameraShakeFSM = cameras.ReflectGetField("cameraShakeFSM");
            if (cameraShakeFSM == null)
            {
                return;
            }

            var sendEventMethod = cameraShakeFSM.GetType().GetMethod("SendEvent", new[] { typeof(string) });
            sendEventMethod?.Invoke(cameraShakeFSM, new object[] { "AverageShake" });
        }

        public void SetRecycleTime(float recycleTime)
        {
            SetValue("recycleTime", Time.time + recycleTime);
        }

        public void PlayDust()
        {
            ActivateObject("dustObj");
        }

        public void PlayGrass()
        {
            ActivateObject("grassObj");
        }

        public void PlayBone()
        {
            ActivateObject("boneObj");
        }

        public void PlaySpa()
        {
            ActivateObject("spaObj");
        }

        public void PlayMetal()
        {
            ActivateObject("metalObj");
        }

        public void PlayWet()
        {
            ActivateObject("wetObj");
        }

        public void SpawnRockParticles(GameObject prefabOverride = null)
        {
            var prefab = prefabOverride != null ? prefabOverride : ParticleRockPrefab;
            if (prefab == null)
            {
                return;
            }

            FlingUtils.SpawnAndFling(new FlingUtils.Config
            {
                Prefab = prefab,
                AmountMin = 2,
                AmountMax = 3,
                SpeedMin = 12f,
                SpeedMax = 15f,
                AngleMin = 95f,
                AngleMax = 140f
            }, Transform, hardLandSpawnOffset);

            FlingUtils.SpawnAndFling(new FlingUtils.Config
            {
                Prefab = prefab,
                AmountMin = 2,
                AmountMax = 3,
                SpeedMin = 12f,
                SpeedMax = 15f,
                AngleMin = 40f,
                AngleMax = 85f
            }, Transform, hardLandSpawnOffset);
        }

        public void SpawnSpaSpatter(GameObject prefabOverride = null)
        {
            var prefab = prefabOverride != null ? prefabOverride : SpatterWhitePrefab;
            if (prefab == null)
            {
                return;
            }

            FlingUtils.SpawnAndFling(new FlingUtils.Config
            {
                Prefab = prefab,
                AmountMin = 50,
                AmountMax = 50,
                SpeedMin = 10f,
                SpeedMax = 30f,
                AngleMin = 85f,
                AngleMax = 95f,
                OriginVariationX = 1f,
                OriginVariationY = 0f
            }, Transform, hardLandSpawnOffset);
        }

        void ActivateObject(string fieldName)
        {
            GetValue<GameObject>(fieldName)?.SetActive(true);
        }

        void ResetBuiltInObject(string fieldName)
        {
            var gameObject = GetValue<GameObject>(fieldName);
            if (gameObject == null)
            {
                return;
            }

            gameObject.SetActive(false);
            gameObject.SetActiveChildren(true);
        }
    }

    /// <summary>
    /// Context for custom fall trail behavior.
    /// </summary>
    public sealed class FallTrailContext
    {
        readonly HeroController heroController;
        readonly Collision2D collision;

        public FallTrailContext(HeroController heroController, Collision2D collision)
        {
            this.heroController = heroController ?? throw new ArgumentNullException(nameof(heroController));
            this.collision = collision;
        }

        public HeroController HeroController => heroController;

        public Collision2D Collision => collision;

        public GameObject GameObject => heroController.gameObject;

        public Transform Transform => heroController.transform;

        public void PlayBuiltInFallTrail()
        {
            var fallTrailFSM = heroController.ReflectGetField("fsm_fallTrail");
            if (fallTrailFSM == null)
            {
                return;
            }

            var sendEventMethod = fallTrailFSM.GetType().GetMethod("SendEvent", new[] { typeof(string) });
            sendEventMethod?.Invoke(fallTrailFSM, new object[] { "PLAY" });
        }

        public GameObject SpawnPrefab(GameObject prefab, Vector3 positionOffset, Vector3 rotation, bool parentToHero, bool randomFlipX)
        {
            if (prefab == null)
            {
                return null;
            }

            var instance = UnityEngine.Object.Instantiate(prefab, Transform.TransformPoint(positionOffset), Quaternion.Euler(rotation));
            if (parentToHero)
            {
                instance.transform.SetParent(Transform, true);
            }
            if (randomFlipX && UnityEngine.Random.Range(1, 100) > 50)
            {
                var localScale = instance.transform.localScale;
                instance.transform.localScale = new Vector3(-localScale.x, localScale.y, localScale.z);
            }
            return instance;
        }
    }
}
