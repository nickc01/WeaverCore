using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    /// <summary>
    /// Used for playing laser animations and other effects on a Laser.
    /// </summary>
    public class LaserEmitter : MonoBehaviour
    {
        [NonSerialized]
        Laser laser;

        [SerializeField]
        [Tooltip("Prefab for the impact effect when the laser hits something.")]
        GameObject impactEffectPrefab;

        [SerializeField]
        [Tooltip("Color of the impact effect.")]
        Color impactColor = Color.white;

        [SerializeField]
        [Tooltip("Minimum and maximum scale for the impact effect.")]
        Vector2 scaleMinMax = new Vector2(1f, 1.25f);

        [SerializeField]
        [Tooltip("Minimum threshold for impact effect visibility.")]
        float minimumImpactAThreshold = 0.2f;

        bool displayImpacts = false;

        [SerializeField]
        [Tooltip("Enable random sprite flipping.")]
        bool randomSpriteFlip = true;

        [SerializeField]
        [Tooltip("Minimum and maximum animation speed for impact effects.")]
        Vector2 animSpeedMinMax = new Vector2(0.75f, 1.25f);

        System.Collections.Generic.List<GameObject> spawnedImpacts = new System.Collections.Generic.List<GameObject>();
        System.Collections.Generic.List<SpawnedImpactData> spawnedImpactData = new System.Collections.Generic.List<SpawnedImpactData>();

        [SerializeField]
        [Tooltip("Spread value during laser charge-up.")]
        float chargeUpSpread = 5f;

        [SerializeField]
        [Tooltip("Width value during laser charge-up.")]
        float chargeUpWidth = 1f;

        [SerializeField]
        [Tooltip("The spread value when the laser ends")]
        float endSpread = 0.5f;

        [SerializeField]
        [Tooltip("The width value when the laser ends")]
        float endWidth = 0.5f;

        [SerializeField]
        [Tooltip("THe curve used to interpolate to the end spread and end width")]
        AnimationCurve endCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Animations")]
        [SerializeField]
        WeaverAnimationData animationData;

        [SerializeField]
        [Tooltip("Animation name for laser charge-up.")]
        string chargeUpAnimationSTRING;

        [SerializeField]
        [Tooltip("Animation name for laser firing loop.")]
        string fireLoopAnimationSTRING;

        [SerializeField]
        [Tooltip("Animation name for laser end.")]
        string endAnimationSTRING;

        [field: Header("Timings")]
        [field: SerializeField]
        [field: Tooltip("Duration for laser charge-up.")]
        public float ChargeUpDuration { get; set; } = 0.75f;

        [field: SerializeField]
        [field: Tooltip("Duration for laser firing loop.")]
        public float FireDuration { get; set; } = 3f;

        /// <summary>
        /// Duration of the laser ending animation
        /// </summary>
        public float EndDuration => animationData.GetClipDuration(endAnimationSTRING);

        /// <summary>
        /// Is the laser currently being fired?
        /// </summary>
        public bool FiringLaser { get; private set; } = false;

        /// <summary>
        /// The laser component used for rendering the laser sprite
        /// </summary>
        public Laser Laser => laser ??= GetComponentInChildren<Laser>(true);

        /// <summary>
        /// Gets the duration of the charge up animation
        /// </summary>
        public float MinChargeUpDuration => animationData.GetClipDuration(chargeUpAnimationSTRING);

        float originalSpread;
        float originalWidth;

        public float DefaultSpread
        {
            get => originalSpread;
            set => originalSpread = value;
        }

        public float DefaultWidth
        {
            get => originalWidth;
            set => originalWidth = value;
        }

        Coroutine partialAnimationRoutine;

        struct SpawnedImpactData
        {
            public float RandomScale;
        }

        private void Awake()
        {
            if (enabled)
            {
                if (Laser.MainCollider != null)
                {
                    Laser.MainCollider.enabled = false;
                }
                Laser.MainRenderer.enabled = false;

                originalSpread = Laser.Spread;
                originalWidth = Laser.StartingWidth;
                laser.gameObject.SetActive(false);
            }
        }

        IEnumerator PlayAnimationLoop(string animationName, float duration)
        {
            var clip = animationData.GetClip(animationName);
            laser.MainRenderer.enabled = true;
            laser.Sprite = clip.Frames[0];

            var endTime = Time.time + duration;
            float secondsPerFrame = 1f / clip.FPS;

            int i = 0;

            while (true)
            {
                for (; i < clip.Frames.Count; i++)
                {
                    laser.Sprite = clip.Frames[i];
                    yield return new WaitForSeconds(secondsPerFrame);
                    if (Time.time >= endTime)
                    {
                        yield break;
                    }
                }
                if (clip.WrapMode == WeaverAnimationData.WrapMode.LoopSection)
                {
                    i = clip.LoopStart;
                }
                else
                {
                    i = 0;
                }
            }

        }

        IEnumerator PlayAnimation(string animationName, int startingFrame = 0, Action<float> onPercentDone = null)
        {
            var clip = animationData.GetClip(animationName);
            laser.MainRenderer.enabled = true;
            laser.Sprite = clip.Frames[0];

            float secondsPerFrame = 1f / clip.FPS;

            for (int i = startingFrame; i < clip.Frames.Count; i++)
            {
                laser.Sprite = clip.Frames[i];
                onPercentDone?.Invoke((i - startingFrame) / (float)(clip.Frames.Count - 1 - startingFrame));
                yield return new WaitForSeconds(secondsPerFrame);
            }
        }

        /// <summary>
        /// Fires the laser
        /// </summary>
        public void FireLaser()
        {
            StartCoroutine(FireLaserRoutine());
        }

        /// <summary>
        /// Fires the charge up animation of the laser only
        /// </summary>
        public void FireChargeUpOnly()
        {
            StartCoroutine(FireChargeUpOnlyRoutine());
        }

        /// <summary>
        /// Fires the charge up animation of the laser only
        /// </summary>
        public IEnumerator FireChargeUpOnlyRoutine()
        {
            yield return PlayChargeUpInRoutine(ChargeUpDuration);
            yield return PlayChargeUpOutRoutine();
        }

        /// <summary>
        /// Fades into the charge up animation
        /// </summary>
        /// <param name="playDuration">The duration of the charge-up animation</param>
        public IEnumerator PlayChargeUpInRoutine(float playDuration)
        {
            if (FiringLaser)
            {
                yield break;
            }
            laser.gameObject.SetActive(true);
            FiringLaser = true;
            laser.Spread = chargeUpSpread;
            laser.StartingWidth = chargeUpWidth;
            if (playDuration > 0)
            {
                yield return PlayAnimationLoop(chargeUpAnimationSTRING, playDuration);
            }
        }

        /// <summary>
        /// Fades out the charge up animation
        /// </summary>
        public IEnumerator PlayChargeUpOutRoutine()
        {
            yield return PlayAnimation(endAnimationSTRING, 2, onPercentDone: t =>
            {
                laser.Spread = Mathf.Lerp(originalSpread, endSpread, t);
                laser.StartingWidth = Mathf.Lerp(originalWidth, endWidth, t);
            });
            laser.MainRenderer.enabled = false;
            FiringLaser = false;
            laser.gameObject.SetActive(false);
        }

        /// <summary>
        /// Fires the laser with a quick charge-up animation
        /// </summary>
        public void FireLaserQuick()
        {
            StartCoroutine(FireLaserQuickRoutine());
        }

        /// <summary>
        /// Fires the laser with a quick charge-up animation
        /// </summary>
        public IEnumerator FireLaserQuickRoutine()
        {
            if (FiringLaser)
            {
                yield break;
            }
            laser.gameObject.SetActive(true);
            FiringLaser = true;
            laser.Spread = chargeUpSpread;
            laser.StartingWidth = chargeUpWidth;
            if (ChargeUpDuration > 0)
            {
                yield return PlayAnimation(chargeUpAnimationSTRING,0);
            }

            if (laser.MainCollider != null)
            {
                laser.MainCollider.enabled = true;
            }
            displayImpacts = true;
            laser.Spread = originalSpread;
            laser.StartingWidth = originalWidth;
            yield return PlayAnimationLoop(fireLoopAnimationSTRING, FireDuration);

            if (laser.MainCollider != null)
            {
                laser.MainCollider.enabled = false;
            }
            displayImpacts = false;
            ClearImpacts();

            yield return PlayAnimation(endAnimationSTRING, onPercentDone: t =>
            {
                laser.Spread = Mathf.Lerp(originalSpread, endSpread, t);
                laser.StartingWidth = Mathf.Lerp(originalWidth, endWidth, t);
            });
            laser.MainRenderer.enabled = false;
            FiringLaser = false;
            laser.gameObject.SetActive(false);
        }

        /// <summary>
        /// Starts charging up the laser. This function is useful if you want to control when the laser's events are triggered
        /// </summary>
        /// <returns>Returns the minimum duration of the animation</returns>
        public float ChargeUpLaser_P1()
        {
            if (partialAnimationRoutine != null)
            {
                StopCoroutine(partialAnimationRoutine);
            }
            laser.gameObject.SetActive(true);
            FiringLaser = true;
            laser.Spread = chargeUpSpread;
            laser.StartingWidth = chargeUpWidth;

            partialAnimationRoutine = StartCoroutine(PlayAnimationLoop(chargeUpAnimationSTRING, float.PositiveInfinity));

            return GetInitialClipDuration(chargeUpAnimationSTRING);
        }

        /// <summary>
        /// Fires the laser. MUST BE CALLED AFTER <see cref="ChargeUpLaser_P1"/>. This function is useful if you want to control when the laser's events are triggered
        /// </summary>
        /// <returns>Returns the minimum duration of the animation</returns>
        public float FireLaser_P2()
        {
            if (partialAnimationRoutine != null)
            {
                StopCoroutine(partialAnimationRoutine);
            }
            if (laser.MainCollider != null)
            {
                laser.MainCollider.enabled = true;
            }
            displayImpacts = true;
            laser.Spread = originalSpread;
            laser.StartingWidth = originalWidth;
            partialAnimationRoutine = StartCoroutine(PlayAnimationLoop(fireLoopAnimationSTRING, float.PositiveInfinity));

            return GetInitialClipDuration(fireLoopAnimationSTRING);
        }

        /// <summary>
        /// Ends the laser. MUST BE CALLED AFTER <see cref="FireLaser_P2"/>. This function is useful if you want to control when the laser's events are triggered
        /// </summary>
        /// <returns>Returns the duration of the animation</returns>
        public float EndLaser_P3(Action onDone = null)
        {
            if (partialAnimationRoutine != null)
            {
                StopCoroutine(partialAnimationRoutine);
            }
            if (laser.MainCollider != null)
            {
                laser.MainCollider.enabled = false;
            }
            displayImpacts = false;
            ClearImpacts();

            IEnumerator EndRoutine()
            {
                var oldSpread = laser.Spread;
                var oldStartingWidth = laser.StartingWidth;

                yield return PlayAnimation(endAnimationSTRING, onPercentDone: t =>
                {
                    laser.Spread = Mathf.Lerp(oldSpread, endSpread, t);
                    laser.StartingWidth = Mathf.Lerp(oldStartingWidth, endWidth, t);
                });
                onDone?.Invoke();
                laser.MainRenderer.enabled = false;
                FiringLaser = false;
                laser.gameObject.SetActive(false);
                partialAnimationRoutine = null;
            }

            partialAnimationRoutine = StartCoroutine(EndRoutine());

            return GetInitialClipDuration(endAnimationSTRING);
        }

        /// <summary>
        /// Fires the laser
        /// </summary>
        public IEnumerator FireLaserRoutine()
        {
            if (FiringLaser)
            {
                yield break;
            }
            laser.gameObject.SetActive(true);
            FiringLaser = true;
            laser.Spread = chargeUpSpread;
            laser.StartingWidth = chargeUpWidth;
            if (ChargeUpDuration > 0)
            {
                yield return PlayAnimationLoop(chargeUpAnimationSTRING, ChargeUpDuration);
            }

            if (laser.MainCollider != null)
            {
                laser.MainCollider.enabled = true;
            }
            displayImpacts = true;
            laser.Spread = originalSpread;
            laser.StartingWidth = originalWidth;
            yield return PlayAnimationLoop(fireLoopAnimationSTRING, FireDuration);

            if (laser.MainCollider != null)
            {
                laser.MainCollider.enabled = false;
            }
            displayImpacts = false;
            ClearImpacts();

            yield return PlayAnimation(endAnimationSTRING, onPercentDone: t =>
            {
                laser.Spread = Mathf.Lerp(originalSpread, endSpread, t);
                laser.StartingWidth = Mathf.Lerp(originalWidth, endWidth, t);
            });
            laser.MainRenderer.enabled = false;
            FiringLaser = false;
            laser.gameObject.SetActive(false);
        }

        IEnumerator InterruptRoutine()
        {
            yield return PlayAnimation(endAnimationSTRING, onPercentDone: t =>
            {
                laser.Spread = Mathf.Lerp(originalSpread, endSpread, t);
                laser.StartingWidth = Mathf.Lerp(originalWidth, endWidth, t);
            });
            laser.MainRenderer.enabled = false;
            laser.gameObject.SetActive(false);
        }

        /// <summary>
        /// Stops firing the laser after a period of time
        /// </summary>
        /// <param name="time">The delay before the laser is stopped</param>
        public void StopLaserAfter(float time)
        {
            IEnumerator Routine(float t)
            {
                yield return new WaitForSeconds(t);
                StopLaser();
            }
            StartCoroutine(Routine(time));
        }

        /// <summary>
        /// Stops firing the laser
        /// </summary>
        public void StopLaser()
        {
            if (!FiringLaser)
            {
                return;
            }
            StopAllCoroutines();
            if (displayImpacts)
            {
                StartCoroutine(InterruptRoutine());
            }
            else
            {
                laser.MainRenderer.enabled = false;
            }
            FiringLaser = false;
            displayImpacts = false;
            ClearImpacts();
            if (laser.MainCollider != null)
            {
                laser.MainCollider.enabled = false;
            }
            if (partialAnimationRoutine != null)
            {
                StopCoroutine(partialAnimationRoutine);
                partialAnimationRoutine = null;
            }
        }


        void ClearImpacts()
        {
            for (int i = spawnedImpacts.Count - 1; i >= 0; i--)
            {
                if (spawnedImpacts[i] != null)
                {
                    Pooling.Destroy(spawnedImpacts[i]);
                }
            }
            spawnedImpacts.Clear();
            spawnedImpactData.Clear();
        }

        private void OnDisable()
        {
            ClearImpacts();
        }

        private void LateUpdate()
        {
            if (laser == null)
            {
                Awake();
            }

            if (displayImpacts)
            {
                int spawnedImpactCounter = 0;
                for (int i = 0; i < laser.ColliderContactPoints.Count - 1; i++)
                {
                    var start = laser.transform.TransformPoint(laser.ColliderContactPoints[i]);
                    var end = laser.transform.TransformPoint(laser.ColliderContactPoints[i + 1]);

                    var midPoint = Vector3.Lerp(end, start, 0.5f);

                    var calculatedNormal = Quaternion.Euler(0f, 0f, -90f) * (end - start);
                    var normal = laser.ColliderContactNormals[i];
                    var distance = Mathf.Min(calculatedNormal.magnitude, normal.magnitude);

                    normal.Normalize();

                    var laserDirection = MathUtilities.PolarToCartesian(laser.transform.eulerAngles.z, 1f);

                    var dotProduct = Mathf.Abs(Vector3.Dot(laserDirection, normal));


                    if (dotProduct >= minimumImpactAThreshold)
                    {
                        if (spawnedImpactCounter >= spawnedImpacts.Count)
                        {
                            var impactEffect = Pooling.Instantiate(impactEffectPrefab);
                            spawnedImpacts.Add(impactEffect);
                            var randomScale = UnityEngine.Random.Range(scaleMinMax.x, scaleMinMax.y);
                            spawnedImpactData.Add(new SpawnedImpactData
                            {
                                RandomScale = randomScale
                            });
                            foreach (var animator in impactEffect.GetComponentsInChildren<WeaverAnimationPlayer>())
                            {
                                if (randomSpriteFlip)
                                {
                                    animator.SpriteRenderer.flipY = UnityEngine.Random.Range(0, 2) == 1;
                                }
                                animator.SpriteRenderer.color = impactColor;

                                animator.PlaybackSpeed = UnityEngine.Random.Range(animSpeedMinMax.x,animSpeedMinMax.y);
                            }
                        }
                        var impact = spawnedImpacts[spawnedImpactCounter];
                        var data = spawnedImpactData[spawnedImpactCounter];

                        impact.transform.position = midPoint;
                        impact.transform.rotation = Quaternion.Euler(0f, 0f, MathUtilities.CartesianToPolar(normal).x);

                        impact.transform.localScale = new Vector3(distance * data.RandomScale * dotProduct, distance * data.RandomScale * dotProduct, 1f);
                        spawnedImpactCounter++;
                    }
                }
                for (int j = spawnedImpacts.Count - 1; j >= spawnedImpactCounter; j--)
                {
                    Pooling.Destroy(spawnedImpacts[j]);
                    spawnedImpacts.RemoveAt(j);
                    spawnedImpactData.RemoveAt(j);
                }
            }
        }

        float GetInitialClipDuration(string clipName)
        {
            var clip = animationData.GetClip(clipName);

            return clip.Frames.Count * (1f / clip.FPS);
        }
    }
}
