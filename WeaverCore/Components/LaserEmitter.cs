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
        float endAnimationFPS = 20;

        [field: Header("Timings")]
        [field: SerializeField]
        public float ChargeUpDuration { get; set; } = 0.75f;

        [field: SerializeField]
        public float FireDuration { get; set; } = 3f;


        public bool FiringLaser { get; private set; } = false;

        float originalSpread;
        float originalWidth;

        struct SpawnedImpactData
        {
            public float RandomScale;
        }

        private void Awake()
        {
            if (enabled)
            {
                laser = GetComponentInChildren<Laser>();
                laser.MainCollider.enabled = false;
                laser.MainRenderer.enabled = false;

                originalSpread = laser.Spread;
                originalWidth = laser.StartingWidth;

                StartCoroutine(TestRoutine());
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

        IEnumerator PlayAnimationLoop(List<Texture> textures, float fps, float duration, int loopSegment = 0)
        {
            laser.MainRenderer.enabled = true;
            laser.Texture = textures[0];

            var endTime = Time.time + duration;
            float secondsPerFrame = 1f / fps;

            int i = 0;

            while (true)
            {
                for (; i < textures.Count; i++)
                {
                    laser.Texture = textures[i];
                    yield return new WaitForSeconds(secondsPerFrame);
                    if (Time.time >= endTime)
                    {
                        yield break;
                    }
                }
                i = loopSegment;
            }

        }

        IEnumerator PlayAnimation(List<Texture> textures, float fps)
        {
            laser.MainRenderer.enabled = true;
            laser.Texture = textures[0];

            float secondsPerFrame = 1f / fps;

            for (int i = 0; i < textures.Count; i++)
            {
                laser.Texture = textures[i];
                yield return new WaitForSeconds(secondsPerFrame);
            }
        }

        public void FireLaser()
        {
            StartCoroutine(FireLaserRoutine());
        }

        public IEnumerator FireLaserRoutine()
        {
            if (FiringLaser)
            {
                yield break;
            }
            FiringLaser = true;
            laser.Spread = chargeUpSpread;
            laser.StartingWidth = chargeUpWidth;
            yield return PlayAnimationLoop(chargeUpAnimation, chargeUpAnimationFPS, ChargeUpDuration, 1);

            laser.MainCollider.enabled = true;
            displayImpacts = true;
            laser.Spread = originalSpread;
            laser.StartingWidth = originalWidth;
            yield return PlayAnimationLoop(fireLoopAnimation, fireLoopAnimationFPS, FireDuration, 1);

            laser.MainCollider.enabled = false;
            displayImpacts = false;
            ClearImpacts();

            yield return PlayAnimation(endAnimation, endAnimationFPS);
            laser.MainRenderer.enabled = false;
            FiringLaser = false;
        }

        IEnumerator InterruptRoutine()
        {
            yield return PlayAnimation(endAnimation, endAnimationFPS);
            laser.MainRenderer.enabled = false;
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

                    var laserDirection = MathUtilties.PolarToCartesian(laser.transform.eulerAngles.z, 1f);

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
                        impact.transform.rotation = Quaternion.Euler(0f, 0f, MathUtilties.CartesianToPolar(normal).x);

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
    }
}
