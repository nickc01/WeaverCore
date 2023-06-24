using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    public class LaserEmitter : MonoBehaviour
    {
        [NonSerialized]
        Laser laser;

        [SerializeField]
        GameObject impactEffectPrefab;

        [SerializeField]
        Color impactColor = Color.white;

        [SerializeField]
        Vector2 scaleMinMax = new Vector2(1f,1.25f);

        [SerializeField]
        float minimumImpactAThreshold = 0.2f;

        bool displayImpacts = false;


        [SerializeField]
        bool randomSpriteFlip = true;

        [SerializeField]
        Vector2 animSpeedMinMax = new Vector2(0.75f,1.25f);

        List<GameObject> spawnedImpacts = new List<GameObject>();
        List<SpawnedImpactData> spawnedImpactData = new List<SpawnedImpactData>();

        [SerializeField]
        float chargeUpSpread = 5f;

        [SerializeField]
        float chargeUpWidth = 1f;

        [Header("Animations")]
        [SerializeField]
        WeaverAnimationData animationData;

        [SerializeField]
        string chargeUpAnimationSTRING;

        [SerializeField]
        string fireLoopAnimationSTRING;

        [SerializeField]
        string endAnimationSTRING;
        /*WeaverAnimationData animationData;


        [Header("Animations")]
        [SerializeField]
        WeaverAnimationData animationData;*/

        /*[SerializeField]
        List<Texture> chargeUpAnimation;
        [SerializeField]
        float chargeUpAnimationFPS = 20;

        [SerializeField]
        List<Texture> fireLoopAnimation;
        [SerializeField]
        float fireLoopAnimationFPS = 20;

        [SerializeField]
        List<Texture> endAnimation;
        [SerializeField]
        float endAnimationFPS = 20;*/

        [field: Header("Timings")]
        [field: SerializeField]
        public float ChargeUpDuration { get; set; } = 0.75f;

        [field: SerializeField]
        public float FireDuration { get; set; } = 3f;

        public float EndDuration => animationData.GetClipDuration(endAnimationSTRING);


        public bool FiringLaser { get; private set; } = false;

        public Laser Laser => laser ??= GetComponentInChildren<Laser>(true);

        public float MinChargeUpDuration => animationData.GetClipDuration(chargeUpAnimationSTRING);

        float originalSpread;
        float originalWidth;

        Coroutine partialAnimationRoutine;

        struct SpawnedImpactData
        {
            public float RandomScale;
        }

        private void Awake()
        {
            if (enabled)
            {
                Laser.MainCollider.enabled = false;
                Laser.MainRenderer.enabled = false;

                originalSpread = Laser.Spread;
                originalWidth = Laser.StartingWidth;
                laser.gameObject.SetActive(false);

                //StartCoroutine(TestRoutine());
            }
        }

        IEnumerator TestRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.5f);
                yield return FireLaserRoutine();
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

        IEnumerator PlayAnimation(string animationName, int startingFrame = 0)
        {
            var clip = animationData.GetClip(animationName);
            laser.MainRenderer.enabled = true;
            laser.Sprite = clip.Frames[0];

            float secondsPerFrame = 1f / clip.FPS;

            for (int i = startingFrame; i < clip.Frames.Count; i++)
            {
                laser.Sprite = clip.Frames[i];
                yield return new WaitForSeconds(secondsPerFrame);
            }
        }

        public void FireLaser()
        {
            StartCoroutine(FireLaserRoutine());
        }

        public void FireChargeUpOnly()
        {
            StartCoroutine(FireChargeUpOnlyRoutine());
        }

        public IEnumerator FireChargeUpOnlyRoutine()
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
            /*laser.MainCollider.enabled = true;
            displayImpacts = true;
            laser.Spread = originalSpread;
            laser.StartingWidth = originalWidth;
            yield return PlayAnimationLoop(fireLoopAnimation, fireLoopAnimationFPS, FireDuration, 1);*/

            yield return PlayAnimation(endAnimationSTRING,2);
            laser.MainRenderer.enabled = false;
            FiringLaser = false;
            laser.gameObject.SetActive(false);
        }

        public void FireLaserQuick()
        {
            StartCoroutine(FireLaserQuickRoutine());
        }

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

            laser.MainCollider.enabled = true;
            displayImpacts = true;
            laser.Spread = originalSpread;
            laser.StartingWidth = originalWidth;
            yield return PlayAnimationLoop(fireLoopAnimationSTRING, FireDuration);

            laser.MainCollider.enabled = false;
            displayImpacts = false;
            ClearImpacts();

            yield return PlayAnimation(endAnimationSTRING);
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
            laser.MainCollider.enabled = true;
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
        public float EndLaser_P3()
        {
            if (partialAnimationRoutine != null)
            {
                StopCoroutine(partialAnimationRoutine);
            }
            laser.MainCollider.enabled = false;
            displayImpacts = false;
            ClearImpacts();

            IEnumerator EndRoutine()
            {
                yield return PlayAnimation(endAnimationSTRING);
                laser.MainRenderer.enabled = false;
                FiringLaser = false;
                laser.gameObject.SetActive(false);
                partialAnimationRoutine = null;
            }

            partialAnimationRoutine = StartCoroutine(EndRoutine());

            return GetInitialClipDuration(endAnimationSTRING);
        }

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

            laser.MainCollider.enabled = true;
            displayImpacts = true;
            laser.Spread = originalSpread;
            laser.StartingWidth = originalWidth;
            yield return PlayAnimationLoop(fireLoopAnimationSTRING, FireDuration);

            laser.MainCollider.enabled = false;
            displayImpacts = false;
            ClearImpacts();

            yield return PlayAnimation(endAnimationSTRING);
            laser.MainRenderer.enabled = false;
            FiringLaser = false;
            laser.gameObject.SetActive(false);
        }

        IEnumerator InterruptRoutine()
        {
            yield return PlayAnimation(endAnimationSTRING);
            laser.MainRenderer.enabled = false;
            laser.gameObject.SetActive(false);
        }

        public void StopLaserAfter(float time)
        {
            IEnumerator Routine(float t)
            {
                yield return new WaitForSeconds(t);
                StopLaser();
            }
            StartCoroutine(Routine(time));
        }

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
            laser.MainCollider.enabled = false;
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
                Pooling.Destroy(spawnedImpacts[i]);
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

            //laser.transform.rotation = Quaternion.Slerp(laser.transform.rotation,Quaternion.Euler(0f,0f, MathUtilties.CartesianToPolar(Player.Player1.transform.position - laser.transform.position).x),2.5f * Time.deltaTime);

            if (displayImpacts)
            {
                //Debug.Log("DISPLAYING IMPACTS");
                int spawnedImpactCounter = 0;
                for (int i = 0; i < laser.ColliderContactPoints.Count - 1; i++)
                {
                    var start = laser.transform.TransformPoint(laser.ColliderContactPoints[i]);
                    var end = laser.transform.TransformPoint(laser.ColliderContactPoints[i + 1]);

                    var midPoint = Vector3.Lerp(end, start, 0.5f);

                    var normal = Quaternion.Euler(0f, 0f, -90f) * (end - start);
                    var distance = normal.magnitude;

                    normal.Normalize();

                    var laserDirection = MathUtilities.PolarToCartesian(laser.transform.eulerAngles.z, 1f);

                    var dotProduct = Mathf.Abs(Vector3.Dot(laserDirection, normal));


                    if (dotProduct >= minimumImpactAThreshold)
                    {
                        //Debug.Log("IMPACT BEING DISPLAYED");
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
